using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterScript : MonoBehaviour
{
    //do we want gizmos drawn?
    public bool drawGizmos;
    public float enemyRadius;
    private Vector3 lastLocation;

    void Awake()
    {
        //set last Location
        lastLocation = transform.position;
    }

    void Start()
    {
        SenseEnemy();
    }

    //use fixed update for consistency in checks
    void FixedUpdate()
    {
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
                Debug.Log("egg");
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
    }
}
