using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//these assets allow us to just make assets instead of overwriting ones we have
[CreateAssetMenu()]
public class NoiseData : UpdatableData//lets us make new assets
{
    public Noise.NormalizeMode normalizeMode;

    //gen map
    public float noiseScale;//literally just a zoom
    public int octaves;
    [Range(0, 1)]
    public float persistance;//amplitude
    public float lacunarity;//frequency

    //for seeds
    public int seed;
    public Vector2 offset;

    protected override void OnValidate()//protected makes this variable not get forgotten and callable by all derived classes
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

        base.OnValidate();//calls onvalidate in updatabledata as well
    }
}
