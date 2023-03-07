using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : UpdatableData
{
    public float uniformScale = 2f;//makes it easy to scale map

    public bool useFlatShading;

    //for mesh
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    //has to do with the offset stuff
    public bool useFalloff;//allows us to choose whether or not we want to apply the falloff map
}
