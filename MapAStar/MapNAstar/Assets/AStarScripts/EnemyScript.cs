using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    //do we want to draw raycasting?
    public bool drawRaycast;

    //save the character object location
    private GameObject characterObject;

    //script references
    Pathfinding2 pathfindingScript;
    PathRequestManager2 pathRequestManager;
    CharacterScript charScript;
    Grid2 grid;
    Unit2 unit;



    void Awake()
    {
        characterObject = GameObject.Find("RigidBodyFPSController");
        pathfindingScript = this.GetComponent<Pathfinding2>();
        pathRequestManager = this.GetComponent<PathRequestManager2>();
        charScript = characterObject.GetComponent<CharacterScript>();
        grid = this.GetComponent<Grid2>();
        unit = this.GetComponent<Unit2>();


        //raycast to search for ground
        Ray ray = new Ray(transform.position, Vector3.down);//ray goes from enemy to ground
        RaycastHit hit;

        //see what we hit
        if (Physics.Raycast(ray, out hit))
        {//if get a hit
            transform.position = hit.point + new Vector3(0, transform.localScale.y, 0);
        }
    }

    void FixedUpdate()
    {
        //destroy self if we are too far from player
        float xDist = Mathf.Abs(transform.position.x - characterObject.transform.position.x);
        float yDist = Mathf.Abs(transform.position.y - characterObject.transform.position.y);
        float totalDist = Mathf.Sqrt(Mathf.Pow(xDist,2) + Mathf.Pow(yDist,2));

        if (totalDist > charScript.enemyRadius)
        {
            Destroy(gameObject);
        }
        else
        {//see if we are touching the player...if we are then end the game
            if (grid.TouchingPlayer())
            {
                Debug.Log("tag");
                Debug.Break();//pauses game instead of completely closing it
            }
        }
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
