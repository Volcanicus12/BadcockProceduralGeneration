using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    public enum NormalizeMode {Local, Global}//local uses local min max and global estimates global min max

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCenter)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];//[,] represents map size...x and y

        //random mapping
        System.Random prng = new System.Random(settings.seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;


        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCenter.x;
            float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCenter.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistance;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        //zoom to center instead of corner
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;



        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;//higher frequency means further apart sample points...height values change more rapidly
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;//makes it so that we can be in range -1 to 1..OG
                    //float perlinValue = Random.Range(sampleX, sampleY) * 2 - 1;//this one just uses random
                    //float perlinValue = Mathf.Cos(sampleX) * 2 - 1;
                    //float perlinValue = Mathf.Cos(sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistance;//decreases octave
                    frequency *= settings.lacunarity;//increases each octave
                }

                //normalizes noiseHeight back to 0 or 1
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;

                if (settings.normalizeMode == NormalizeMode.Global)
                {
                    float normalizeHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 2f);//reverses earlier perlin...we then / by 1.75 bc noisemap won't ever reach maxPossible height so we get dividing
                    noiseMap[x, y] = Mathf.Clamp(normalizeHeight, 0, int.MaxValue);
                }
            }
        }


        //loops through all values again to normalize noiseMap

        if (settings.normalizeMode == NormalizeMode.Local)
        { 
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);//inverse lerp returns value between 0 and 1 //this line would work if we were not using an endless terrain system, but we are and it causes seams to not match
                }
            }
        }

                return noiseMap;
    }
}

[System.Serializable]
public class NoiseSettings
{
    public Noise.NormalizeMode normalizeMode;

    //gen map
    public float scale = 50;//literally just a zoom
    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = 0.6f;//amplitude
    public float lacunarity = 2;//frequency

    //for seeds
    public int seed;
    public Vector2 offset;

    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 0.01f);//max func will choose whatever is greates...0.01 or scale
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);
    }
}