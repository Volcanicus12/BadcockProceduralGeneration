using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject//this is now the asset scripter and noise/terrain data will inherit from here
{
    public event System.Action OnValuesUpdated;//event is just to note if a change happens
    public bool autoUpdate;//do we want to update if a change happens

    #if UNITY_EDITOR
    //makes it so code is only compiled if in unity editor
    protected virtual void OnValidate()//called when a value is changed in inspector or scripts compiled
    {
        if (autoUpdate)
        {
            UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;//subscribe for instant dong
        }
    }

    public void NotifyOfUpdatedValues()
    {
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;//unsub so it doesn't keep calling...delays calling of method til shader is done compiling
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();//if onvaluesupdated isn't null then call that event
        }
    }
    #endif
}
