using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkConfig : MonoBehaviour
{
    [SerializeField]
    private bool isNetworkMode;

    [SerializeField]
    private bool isEditorClientOnlyNetwork;

    [SerializeField]
    private float fixedDeltaTimeRate;

    [SerializeField]
    private string ipAddress = "localhost";

    [SerializeField]
    private int port = 9050;

    public bool IsNetworkMode
    {
        get
        {
            return isNetworkMode;
        }
    }

    public bool IsEditorClientOnlyNetwork
    {
        get
        {
            return isEditorClientOnlyNetwork;
        }
    }

    public float FixedDeltaTimeRate
    {
        get
        {
            return fixedDeltaTimeRate;
        }
    }

    public string IpAddress
    {
        get
        {
            return ipAddress;
        }
    }

    public int Port
    {
        get
        {
            return port;
        }
    }

    public static NetworkConfig Inst;
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

    public void ChangeIpAddress(string v)
    {
        ipAddress = v;
    }
}
