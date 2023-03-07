using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    public enum NormalizeMode {Local, Global}//local uses local min max and global estimates global min max

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];//[,] represents map size...x and y

        //random mapping
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;


        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000,100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }


        //mapping
        if(scale <= 0)
        {
            scale = 0.0001f;
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

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;//higher frequency means further apart sample points...height values change more rapidly
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;//makes it so that we can be in range -1 to 1..OG
                    //float perlinValue = Random.Range(sampleX, sampleY) * 2 - 1;//this one just uses random
                    //float perlinValue = Mathf.Cos(sampleX) * 2 - 1;
                    //float perlinValue = Mathf.Cos(sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;//decreases octave
                    frequency *= lacunarity;//increases each octave
                }
                
                //normalizes noiseHeight back to 0 or 1
                if(noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if(noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }


        //loops through all values again to normalize noiseMap
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if(normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);//inverse lerp returns value between 0 and 1 //this line would work if we were not using an endless terrain system, but we are and it causes seams to not match
                }
                else
                {
                    float normalizeHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight/2f);//reverses earlier perlin...we then / by 1.75 bc noisemap won't ever reach maxPossible height so we get dividing
                    noiseMap[x, y] = Mathf.Clamp(normalizeHeight, 0, int.MaxValue);
                }
                
                
            }
        }

                return noiseMap;
    }
}
