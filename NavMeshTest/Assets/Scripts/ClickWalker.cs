using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;//this is the ai package of unity and needs to be put in to allow this to work


public class ClickWalker : MonoBehaviour
{
    public NavMeshAgent agent;//gives us a ref to our agent that is AI moving

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1)){//1 is right click
            //raycast to get click
            Ray movePosition = Camera.main.ScreenPointToRay(Input.mousePosition);//hard searches for main camera and sees where we clicked
            if (Physics.Raycast(movePosition, out var hitInfo))//out variable is a name for var output from method
            {
                agent.SetDestination(hitInfo.point);//.point is a v3
            }
        }
    }
}
