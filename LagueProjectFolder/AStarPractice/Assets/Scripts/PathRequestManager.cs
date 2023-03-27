using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    PathRequest currentPathRequest;

    //need static variables to get info from static requestPath
    static PathRequestManager instance;

    bool isProcessingPath;//are we processing right now

    //references
    Pathfinding pathfinding;

    void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)//action stores a method til it is ready to be sent
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);//make request
        instance.pathRequestQueue.Enqueue(newRequest);//stick request in queue
        instance.TryProcessNext();
    }

    //see if we are currently processing and if not then we process next one
    void TryProcessNext()
    {
        if (!isProcessingPath && pathRequestQueue.Count > 0)//if not processing something and queue isn't empty
        {
            currentPathRequest = pathRequestQueue.Dequeue();//gets and removes item from queue
            isProcessingPath = true;
            pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        }
    }

    //called when pathfinding finishes finding a path
    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        currentPathRequest.callback(path, success);
        isProcessingPath = false;
        TryProcessNext();
    }

    //structure to hold all RequestPath info
    struct PathRequest {
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
