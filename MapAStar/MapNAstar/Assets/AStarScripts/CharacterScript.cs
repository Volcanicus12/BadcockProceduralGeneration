using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterScript : MonoBehaviour
{
    //do we want gizmos drawn?
    public bool drawGizmos;
    public float enemyRadius;
    private Vector3 lastLocation;

    private bool movedToGround;//helps us move character to ground at start of game
    //public GameObject enemy;

    void Awake()
    {
        //set last Location
        lastLocation = transform.position;

        movedToGround = false;
    }

    void Start()
    {
        //SenseEnemy();
    }

    //use fixed update for consistency in checks
    void FixedUpdate()
    {
        if (movedToGround == false)//I wanted this in start, but can't bc map builds after we would look
        {
            //raycast to search for ground
            Ray ray = new Ray(transform.position, Vector3.down);//ray goes from char to ground
            RaycastHit hit;

            //see what we hit
            if (Physics.Raycast(ray, out hit))
            {//if get a hit
                transform.position = hit.point + new Vector3(0, transform.localScale.y, 0);
                movedToGround = true;
            }
        }


        //check for an enemy every frame only if we move enough
        if (lastLocation != transform.position)
        {
            lastLocation = transform.position; 
            SenseEnemy();
        }
    }




    private void SenseEnemy()
    {
        //see what objects are in our range
        Collider[] thingsInSphere = Physics.OverlapSphere(transform.position, enemyRadius);
        foreach (Collider collisions in thingsInSphere)//iterate through what we hit
        {
            if (collisions.gameObject.layer == LayerMask.NameToLayer("Enemy"))//if we are in range of an enemy
            {
                collisions.gameObject.GetComponent<EnemyScript>().CanSeeCharacter();//call CanSeeCharacter
            }
        }
    }

    //drawing character gizmos
    void OnDrawGizmos()
    {
        if (drawGizmos) {
            Color enemyRadiusColor = Color.yellow;
            enemyRadiusColor.a = 0.5f;
            Gizmos.color = enemyRadiusColor;
            Gizmos.DrawSphere(transform.position, enemyRadius);
        }

        //Debug.DrawRay(transform.position, Vector3.down * (50 + 10));
    }
}
