using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

using Debug = UnityEngine.Debug;//we will only ever use the unity engine debug


public class EnemyManager : MonoBehaviour
{
    public GameObject enemy;//make it so our enemies can spawn
    public GameObject player;

    private Stopwatch timer;//initial monster spawn time

    private GameObject enemyFolder;//place to put all the enemies

    private System.Random rand;//used to get random numbers for spawn

    //spawn ranges
    public int spawnMinRange;
    public int spawnMaxRange;

    //spawn times
    private int lastSecond;
    public int timeBetweenSpawns;

    void Start()
    {
        timer = new Stopwatch();
        timer.Start();
        enemyFolder = GameObject.Find("enemies");

        rand = new System.Random();

        lastSecond = 0;//don't want instant spawn
    }



    void Update()
    {
        if(lastSecond != timer.Elapsed.Seconds)//if we haven't already spawned for this time
        {
            if (timer.Elapsed.Seconds % timeBetweenSpawns == 0)//if we are divisible by the time between spawns
            {
                lastSecond = timer.Elapsed.Seconds;
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        Debug.Log("SPAWN NOW");

        //get spawn location
        //positive or negative direction
        float posNegX = rand.Next(-100, 101);
        float posNegY = rand.Next(-100, 101);

        while (posNegX == 0 && posNegY == 0)//they can not both reduce to 0...unlikely, but possible
        {
            posNegX = rand.Next(-100, 101);
            posNegY = rand.Next(-100, 101);
        }

        //get them to just be + or -
        if (posNegX != 0)
        {
            posNegX = posNegX / Mathf.Abs(posNegX);
        }
        if (posNegY != 0)
        {
            posNegY = posNegY / Mathf.Abs(posNegY);
        }

        //get locations
        float xLoc = rand.Next(spawnMinRange, spawnMaxRange + 1) * posNegX;
        float yLoc = rand.Next(spawnMinRange, spawnMaxRange + 1) * posNegY;

        Vector3 spawnLoc = player.transform.position + new Vector3(xLoc, 100, yLoc);//spawn randomly within the range and 100 up (100 is general taller than map number)

        GameObject newEnemy = Instantiate(enemy, spawnLoc, Quaternion.Euler(0, 0, 0));//make enemy, 5 forward from player, with given rotation
        newEnemy.transform.SetParent(enemyFolder.transform);//set parent so everything in same place
    }
}
