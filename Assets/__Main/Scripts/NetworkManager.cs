using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class NetworkManager : MonoBehaviour
{
    private Client client;
    private Server server;

    public static NetworkManager Inst;
    void Awake()
    {
        if (Inst != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Inst = this;
        }
    }

    void OnEnable()
    {
#if UNITY_EDITOR
        if (NetworkConfig.Inst.IsNetworkMode)
        {
            if (!NetworkConfig.Inst.IsEditorClientOnlyNetwork)
            {
                server = gameObject.AddComponent<Server>();
            }
            client = gameObject.AddComponent<Client>();
        }
#else
        if (NetworkConfig.Inst.IsNetworkMode)
        {
            if (IsHeadless())
            {
                server = gameObject.AddComponent<Server>();

                DisableRenderers();
            }
            else
            {
                client = gameObject.AddComponent<Client>();
            }
        }
#endif
    }

    public void ConnectServer()
    {
        if (client != null)
        {
            client.Connect();
        }
    }

    public void SpawnActorCommand()
    {
        if (client != null)
        {
            client.CommandSpawnActor();
        }
    }

    public bool IsHeadless()
    {
        return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
    }

    void DisableRenderers()
    {
        var agents = GameObject.FindGameObjectsWithTag("Agent");
        var objs = GameObject.FindGameObjectsWithTag("Object");
        foreach (var a in agents)
        {
            a.GetComponent<Renderer>().enabled = false;
        }

        foreach (var o in objs)
        {
            o.GetComponent<Renderer>().enabled = false;
        }
    }
}
