using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//these assets allow us to just make assets instead of overwriting ones we have
[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData//lets us make new assets
{
    public NoiseSettings noiseSettings;

    //for mesh
    public float heightMultiplier;
    public AnimationCurve heightCurve;

    //has to do with the offset stuff
    public bool useFalloff;//allows us to choose whether or not we want to apply the falloff map

    public float minHeight//this is an accessor
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(0);
        }
    }

    public float maxHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(1);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()//protected makes this variable not get forgotten and callable by all derived classes
    {
        noiseSettings.ValidateValues();

        base.OnValidate();//calls onvalidate in updatabledata as well
    }
#endif
}
