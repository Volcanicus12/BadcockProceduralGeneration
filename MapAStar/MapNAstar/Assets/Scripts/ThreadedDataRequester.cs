using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedDataRequester : MonoBehaviour
{
    static ThreadedDataRequester instance;
    Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();//queue of commands

    void Awake()
    {
        instance = FindObjectOfType<ThreadedDataRequester>();//instance allows us to access "data" variable even though it is in a static
    }

    //map threading
    public static void RequestData(Func<object> generateData, Action<object> callback)//stuff in <> is what the method expects to get
    {
        ThreadStart threadStart = delegate//delegate is just an entrypoint for the thread
        {
            instance.DataThread(generateData, callback);
        };

        new Thread(threadStart).Start();
    }

    void DataThread(Func<object> generateData, Action<object> callback)
    {
        object data = generateData();//our object is equal to what is returned by the generateData method
        //struct holds mapdata and callback
        lock (dataQueue)//when a thread reaches here it stops all threads from accessing this area
        {
            dataQueue.Enqueue(new ThreadInfo(callback, data));
        }
    }

    

    void Update()
    {
        if (dataQueue.Count > 0)
        {
            for (int i = 0; i < dataQueue.Count; i++)
            {
                ThreadInfo threadInfo = dataQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

       
    }

    struct ThreadInfo//<T>//T makes it generic so we can use it for mesh too
    {
        public readonly Action<object> callback;//readonly makes them immutable/unchangeable
        public readonly object parameter;

        public ThreadInfo(Action<object> callback, object parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
