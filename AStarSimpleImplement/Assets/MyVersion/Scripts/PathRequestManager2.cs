using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathRequestManager2 : MonoBehaviour
{
    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();//queue of requests
    PathRequest currentPathRequest;

    static PathRequestManager2 instance;

    Pathfinding2 pathfinding;//pathfinding2 access

    bool isProcessingPath;

    void Awake()
    {
        instance = this;//makes the static method accessible
        pathfinding = GetComponent<Pathfinding2>();
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);//make a new request

        instance.pathRequestQueue.Enqueue(newRequest);

        instance.TryProcessNext();
    }

    //sees if we are currently processing a path and if not then ask script to process next
    void TryProcessNext()
    {
        if (!isProcessingPath && pathRequestQueue.Count > 0)//if not already processing and queue not empty
        {
            currentPathRequest = pathRequestQueue.Dequeue();//takes item out of queue
            isProcessingPath = true;
            pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        }
    }

    //called by pathfinding script when done processing path
    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        currentPathRequest.callback(path, success);
        isProcessingPath = false;
        TryProcessNext();
    }

    struct PathRequest//structure of a path request
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 _start, Vector3 _end, Action<Vector3[], bool> _callback)
        {
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
        }
    }
}
