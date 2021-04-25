using System;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Linq;

public class Server : MonoBehaviour
{
    private EventBasedNetListener listener;
    private NetManager server;

    private List<NetPeer> peers = new List<NetPeer>();
    private GameObject[] actors; // TODO: instant code
    private Dictionary<int, GameObject> actorsDict; // TODO: instant code
    private Dictionary<NetworkDefine.ClientCommand, Action<NetDataReader>> events = 
        new Dictionary<NetworkDefine.ClientCommand, Action<NetDataReader>>();

    void OnEnable()
    {
        listener = new EventBasedNetListener();
        server = new NetManager(listener);
        server.Start(NetworkConfig.Inst.Port);

        listener.ConnectionRequestEvent += request =>
        {
            request.AcceptIfKey("");
        };

        listener.PeerConnectedEvent += peer =>
        {
            Debug.Log("connect peer");
            peers.Add(peer);

            SyncActorList(peer);
        };

        listener.PeerDisconnectedEvent += (peer, info) =>
        {
            Debug.Log("disconnect peer");
            peers.Remove(peer);
        };

        events.Add(NetworkDefine.ClientCommand.SpawnActor, (dataReader) => {
            NewActor();
        });

        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            var opcode = (NetworkDefine.ClientCommand)dataReader.GetInt();
            events[opcode](dataReader);

            dataReader.Recycle();
        };

        // TODO: instant code start
        var a = GameObject.FindGameObjectsWithTag("Actor");
        actors = new GameObject[a.Length];

        Array.Copy(a, actors, a.Length);

        actorsDict = actors.ToDictionary(a => a.GetComponent<NetworkObject>().Id, a => a);
        // TODO: instant code end
    }

    void OnDisable()
    {
        if (server != null)
            server.Stop();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            NewActor();
        }

        server.PollEvents();
    }

    void LateUpdate()
    {
        SyncActorPositions();
    }

    private void NewActor()
    {
        var a = AgentManager.Inst.SpawnServerAgent();
        SyncNewActor(a.Item1, a.Item2);
    }

    private void SyncActorList(NetPeer peer)
    {
        const int unit_size = 4;
        var agentsDict = AgentManager.Inst.AgentDict;
        var dataSize = unit_size * agentsDict.Count;
        var b = new byte[dataSize + 4 + 4];
        var op = BitConverter.GetBytes((int)NetworkDefine.OpCode.ActorList);
        var asize = BitConverter.GetBytes(dataSize);

        Buffer.BlockCopy(op, 0, b, 0, 4);
        Buffer.BlockCopy(asize, 0, b, 4, 4);

        int offset = 8;
        foreach (var a in agentsDict)
        {
            var id = BitConverter.GetBytes(a.Key);
            Buffer.BlockCopy(id, 0, b, offset, 4);

            offset += unit_size;
        }

        NetDataWriter writer = new NetDataWriter();
        writer.Put(b);

        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void SyncActorPositions()
    {
        const int unit_size = 16;
        var agentsDict = AgentManager.Inst.AgentDict;
        var dataSize = unit_size * (actorsDict.Count + agentsDict.Count);
        var b = new byte[dataSize + 4 + 4];
        var op = BitConverter.GetBytes((int)NetworkDefine.OpCode.MoveActors);
        var asize = BitConverter.GetBytes(dataSize);

        Buffer.BlockCopy(op, 0, b, 0, 4);
        Buffer.BlockCopy(asize, 0, b, 4, 4);

        int offset = 8;
        Action<KeyValuePair<int, GameObject>, byte[]> serializePosition = (a, b) => {
            var id = BitConverter.GetBytes(a.Key);
            var x = BitConverter.GetBytes(a.Value.transform.position.x);
            var y = BitConverter.GetBytes(a.Value.transform.position.y);
            var z = BitConverter.GetBytes(a.Value.transform.position.z);

            Buffer.BlockCopy(id, 0, b, offset, 4);
            Buffer.BlockCopy(x, 0, b, offset + 4, 4);
            Buffer.BlockCopy(y, 0, b, offset + 8, 4);
            Buffer.BlockCopy(z, 0, b, offset + 12, 4);
        };

        foreach (var a in actorsDict)
        {
            serializePosition(a, b);
            offset += unit_size;
        }

        foreach (var a in agentsDict)
        {
            serializePosition(a, b);
            offset += unit_size;
        }

        NetDataWriter writer = new NetDataWriter();
        writer.Put(b);

        foreach (var p in peers)
        {
            p.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }

    private void SyncNewActor(int id, GameObject actor)
    {
        var b = new byte[20];
        var op = BitConverter.GetBytes((int)NetworkDefine.OpCode.SpawnActor);
        var actorId = BitConverter.GetBytes(id);
        var x = BitConverter.GetBytes(actor.transform.position.x);
        var y = BitConverter.GetBytes(actor.transform.position.y);
        var z = BitConverter.GetBytes(actor.transform.position.z);

        Buffer.BlockCopy(op, 0, b, 0, 4);
        Buffer.BlockCopy(actorId, 0, b, 4, 4);
        Buffer.BlockCopy(x, 0, b, 8,  4);
        Buffer.BlockCopy(y, 0, b, 12, 4);
        Buffer.BlockCopy(z, 0, b, 16, 4);

        NetDataWriter writer = new NetDataWriter();
        writer.Put(b);

        foreach (var p in peers)
        {
            p.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }
}
