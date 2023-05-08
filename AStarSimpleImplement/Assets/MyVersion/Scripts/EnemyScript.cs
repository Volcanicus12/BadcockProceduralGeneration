using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    //do we want to draw raycasting?
    public bool drawRaycast;

    //save the character object location
    public GameObject characterObject;

    //script references
    Pathfinding2 pathfindingScript;
    PathRequestManager2 pathRequestManager;
    CharacterScript charScript;
    Grid2 grid;
    Unit2 unit;



    void Awake()
    {
        pathfindingScript = this.GetComponent<Pathfinding2>();
        pathRequestManager = this.GetComponent<PathRequestManager2>();
        charScript = characterObject.GetComponent<CharacterScript>();
        grid = this.GetComponent<Grid2>();
        unit = this.GetComponent<Unit2>();
    }

    //see if character is in LOS
    public void CanSeeCharacter(){
        //raycast to search for a player
        Ray ray = new Ray(transform.position, (characterObject.transform.position - transform.position).normalized);//ray goes from enemy to character
        RaycastHit hit;

        //see what we hit
        if (Physics.Raycast(ray, out hit, charScript.enemyRadius)){//if get a hit
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Character"))//if the raycast is hitting the character, instead of an obstacle/ground
            {
                //pathfind to them
                grid.CreateGrid();//get component is costly...put in awake or start
                pathfindingScript.StartFindPath(this.transform.position, characterObject.transform.position);
                pathRequestManager.RequestPath(transform.position, characterObject.transform.position, unit.OnPathFound);
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
