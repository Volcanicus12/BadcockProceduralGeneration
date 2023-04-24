using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    //do we want to draw raycasting?
    public bool drawRaycast;

    //save the character object location
    public GameObject characterObject;

    //see if character is in LOS
    public void CanSeeCharacter(){
        //raycast to search for a player
        Ray ray = new Ray(transform.position, (characterObject.transform.position - transform.position).normalized);//ray goes from enemy to character
        RaycastHit hit;

        //see what we hit
        if (Physics.Raycast(ray, out hit, characterObject.GetComponent<CharacterScript>().enemyRadius)){//if get a hit
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Character"))//if the raycast is hitting the character, instead of an obstacle/ground
            {
                //pathfind to them
                this.GetComponent<Grid2>().CreateGrid();
                this.GetComponent<Pathfinding2>().StartFindPath(this.transform.position, characterObject.transform.position);
                PathRequestManager2.RequestPath(transform.position, characterObject.transform.position, GetComponent<Unit2>().OnPathFound);
            }

        }
    }





    //draw raycast
    void OnDrawGizmos()
    {
        if (drawRaycast)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, transform.position + (characterObject.transform.position - transform.position) / 2);
        }
    }
}
