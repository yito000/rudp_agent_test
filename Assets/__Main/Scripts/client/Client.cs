using System;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Linq;

public class Client : MonoBehaviour
{
    private EventBasedNetListener listener;
    private NetManager client;
    private NetPeer serverPeer;

    private GameObject[] actors;
    private Dictionary<int, GameObject> actorsDict;
    private Dictionary<NetworkDefine.OpCode, Action<NetDataReader>> events = 
        new Dictionary<NetworkDefine.OpCode, Action<NetDataReader>>();

    void OnEnable()
    {
        events.Add(NetworkDefine.OpCode.ActorList, (dataReader) => {
            ReceiveEvent.OnReceiveActorList(dataReader);
        });
        events.Add(NetworkDefine.OpCode.MoveActors, (dataReader) => {
            ReceiveEvent.OnMoveActors(dataReader, actorsDict);
        });
        events.Add(NetworkDefine.OpCode.SpawnActor, (dataReader) => {
            ReceiveEvent.OnSpawnActor(dataReader);
        });

#if UNITY_EDITOR
        if (!NetworkConfig.Inst.IsEditorClientOnlyNetwork)
        {
            Connect();
        }
#endif
    }

    void Update()
    {
        if (client != null)
            client.PollEvents();
    }

    void OnDisable()
    {
        Disconnect();
    }

    public void Connect()
    {
#if UNITY_EDITOR
        if (!NetworkConfig.Inst.IsEditorClientOnlyNetwork && client != null)
        {
            return;
        }
#endif

        Disconnect();

        listener = new EventBasedNetListener();
        client = new NetManager(listener);
        client.Start();
        client.Connect(NetworkConfig.Inst.IpAddress, NetworkConfig.Inst.Port, "");

        listener.PeerConnectedEvent += peer =>
        {
            Debug.Log("connect server");
            serverPeer = peer;
        };

        listener.PeerDisconnectedEvent += (peer, info) => {
            Disconnect();
        };

        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            var opcode = (NetworkDefine.OpCode)dataReader.GetInt();
            events[opcode](dataReader);

            dataReader.Recycle();
        };

        var a = GameObject.FindGameObjectsWithTag("Actor");
        actors = new GameObject[a.Length];

        Array.Copy(a, actors, a.Length);

        actorsDict = actors.ToDictionary(a => a.GetComponent<NetworkObject>().Id, a => a);
    }

    public void Disconnect()
    {
#if UNITY_EDITOR
        if (!NetworkConfig.Inst.IsEditorClientOnlyNetwork)
        {
            return;
        }
#endif

        if (client != null)
        {
            client.Stop();
            client = null;
            serverPeer = null;

            actors = null;
            actorsDict.Clear();

            AgentManager.Inst.RemoveAllActors();
        }
    }

    public void CommandSpawnActor()
    {
        if (client == null)
            return;

        var op = BitConverter.GetBytes((int)NetworkDefine.ClientCommand.SpawnActor);

        NetDataWriter writer = new NetDataWriter();
        writer.Put(op);

        serverPeer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private class ReceiveEvent
    {
        public static void OnReceiveActorList(NetDataReader dataReader)
        {
            const int unit_size = 4;
            var length = dataReader.GetInt();
            var b = new byte[length];
            dataReader.GetBytes(b, 0, length);

            Debug.Log("on receive actor list");

            var offset = 0;

            int max = length / unit_size;
            for (int i = 0; i < max; i++)
            {
                var id = BitConverter.ToInt32(b, offset);
                if (!AgentManager.Inst.AgentDict.ContainsKey(id))
                {
#if !UNITY_EDITOR
                    var a = AgentManager.Inst.SpawnClientAgent(id);
#else
                    if (NetworkConfig.Inst.IsEditorClientOnlyNetwork)
                    {
                        var a = AgentManager.Inst.SpawnClientAgent(id);
                    }
#endif
                }
                else
                {
                    Debug.Log("warning already spawn same actor");
                }

                offset += unit_size;
            }
        }

        public static void OnMoveActors(NetDataReader dataReader, Dictionary<int, GameObject> actorsDict)
        {
            const int unit_size = 16;
            var length = dataReader.GetInt();
            var b = new byte[length];
            dataReader.GetBytes(b, 0, length);

            var offset = 0;

            Action<int, Dictionary<int, GameObject>, byte[]> setPosition = (id, dict, b) => {
                var x = BitConverter.ToSingle(b, offset + 4);
                var y = BitConverter.ToSingle(b, offset + 8);
                var z = BitConverter.ToSingle(b, offset + 12);

                var a = dict[id];
                var v = new Vector3(x, y, z);
                //Debug.Log(v);

#if !UNITY_EDITOR
                a.transform.position = v;
#else
                if (NetworkConfig.Inst.IsEditorClientOnlyNetwork)
                {
                    a.transform.position = v;
                }
#endif
            };

            int max = length / unit_size;
            for (int i = 0; i < max; i++)
            {
                var id = BitConverter.ToInt32(b, offset);
                if (actorsDict.ContainsKey(id))
                {
                    setPosition(id, actorsDict, b);
                }

                if (AgentManager.Inst.AgentDict.ContainsKey(id))
                {
                    setPosition(id, AgentManager.Inst.AgentDict, b);
                }

                offset += unit_size;
            }
        }

        public static void OnSpawnActor(NetDataReader dataReader)
        {
            var id = dataReader.GetInt();
            var x = dataReader.GetFloat();
            var y = dataReader.GetFloat();
            var z = dataReader.GetFloat();
            var v = new Vector3(x, y, z);

#if !UNITY_EDITOR
            var a = AgentManager.Inst.SpawnClientAgent(id);
            a.Item2.transform.position = v;
#else
            if (NetworkConfig.Inst.IsEditorClientOnlyNetwork)
            {
                var a = AgentManager.Inst.SpawnClientAgent(id);
                a.Item2.transform.position = v;
            }
#endif
        }
    }
}
