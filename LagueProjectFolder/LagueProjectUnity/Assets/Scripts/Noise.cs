using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];//[,] represents map size...x and y

        //random mapping
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000,100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }


        //mapping
        if(scale <= 0)
        {
            scale = 0.0001f;
        }


        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        //zoom to center instead of corner
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;



        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;//higher frequency means further apart sample points...height values change more rapidly
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) *2 - 1;//makes it so that we can be in range -1 to 1
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;//decreases octave
                    frequency *= lacunarity;//increases each octave
                }
                
                //normalizes noiseHeight back to 0 or 1
                if(noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if(noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }


        //loops through all values again to normalize noiseMap
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);//inverse lerp returns value between 0 and 1
            }
        }

                return noiseMap;
    }
}
