using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObject : MonoBehaviour
{
    [SerializeField]
    private int id = -1;

    public int Id
    {
        get
        {
            return id;
        }
        set
        {
            id = value;
        }
    }
}
