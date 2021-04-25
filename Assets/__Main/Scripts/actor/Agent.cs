using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

[RequireComponent(typeof(NavMeshAgent))]
public class Agent : MonoBehaviour
{
    private GameObject target1;
    private GameObject target2;

    private NavMeshAgent naviMeshAgent;
    private int currentIndex = 0;
    private GameObject[] posses = new GameObject[2];

    void OnEnable()
    {
        naviMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void Setup(GameObject t1, GameObject t2)
    {
        target1 = t1;
        target2 = t2;

        posses[0] = target1;
        posses[1] = target2;
    }

    void Update()
    {
        var dist = (transform.position - posses[currentIndex].transform.position).magnitude;
        if (dist < 1.5f)
        {
            currentIndex = currentIndex ^ 1;
        }

        naviMeshAgent.SetDestination(posses[currentIndex].transform.position);
    }
}
