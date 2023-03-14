using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;//this is the ai package of unity and needs to be put in to allow this to work


public class Chaser : MonoBehaviour
{
    public GameObject target;//the character
    private NavMeshAgent agent;//gives us a ref to our agent that is AI moving
    Vector3 lastSeen;//keeps track of where player was last frame

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        lastSeen = target.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(lastSeen != target.transform.position)
        {
            lastSeen = target.transform.position;
        }

        agent.SetDestination(lastSeen);//.point is a v3
    }
}
