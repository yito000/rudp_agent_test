using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class NetworkManager : MonoBehaviour
{
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
                gameObject.AddComponent<Server>();
            }
            gameObject.AddComponent<Client>();
        }
#else
        if (NetworkConfig.Inst.IsNetworkMode)
        {
            if (IsHeadless())
            {
                gameObject.AddComponent<Server>();

                DisableRenderers();
            }
            else
            {
                gameObject.AddComponent<Client>();
            }
        }
#endif
    }

    public void SpawnActorCommand()
    {
        var c = gameObject.GetComponent<Client>();
        if (c != null)
        {
            c.CommandSpawnActor();
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
