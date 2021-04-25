using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkDefine
{
    public enum OpCode
    {
        ActorList,
        MoveActors,
        SpawnActor
    }

    public enum ClientCommand
    {
        SpawnActor
    }
}
