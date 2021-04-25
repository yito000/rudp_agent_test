using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentManager : MonoBehaviour
{
    private List<GameObject> agentList = new List<GameObject>();
    private Dictionary<int, GameObject> agentDict = new Dictionary<int, GameObject>();
    private int currentId = 0;

    [SerializeField]
    private GameObject ServerAgentTemplate;

    [SerializeField]
    private GameObject ClientAgentTemplate;

    public static AgentManager Inst;
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

    public GameObject GetActor(int id)
    {
        if (!agentDict.ContainsKey(id))
        {
            return null;
        }

        return agentDict[id];
    }

    public (int, GameObject) SpawnServerAgent()
    {
        var newActor = Instantiate(ServerAgentTemplate);
        agentDict[currentId] = newActor;
        agentList.Add(newActor);

        newActor.GetComponent<NetworkObject>().Id = currentId;

        // TODO start: instant code
        var a = GameObject.FindGameObjectsWithTag("Actor");
        newActor.GetComponent<Agent>().Setup(a[0], a[1]);
        newActor.GetComponent<NavMeshAgent>().speed = UnityEngine.Random.Range(3.0f, 4.0f);
        // TODO end

        return (currentId++, newActor);
    }

    public (int, GameObject) SpawnClientAgent(int id)
    {
        var newActor = Instantiate(ClientAgentTemplate);
        agentDict[id] = newActor;
        agentList.Add(newActor);

        newActor.GetComponent<NetworkObject>().Id = id;

        return (id, newActor);
    }

    public bool RemoveActor(int id)
    {
        if (!agentDict.ContainsKey(id))
        {
            return false;
        }

        Destroy(agentDict[id]);

        agentList.Remove(agentDict[id]);
        agentDict.Remove(id);

        return true;
    }

    public void RemoveAllActors()
    {
        foreach (var a in agentList)
        {
            Destroy(a);
        }

        agentList.Clear();
        agentDict.Clear();
    }

    public List<GameObject> AgentList
    {
        get
        {
            return agentList;
        }
    }

    public Dictionary<int, GameObject> AgentDict
    {
        get
        {
            return agentDict;
        }
    }
}
