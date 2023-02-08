using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ai autocomplete for visual studios

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColorMap};
    public DrawMode drawMode;

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;//literally just a zoom
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    //for seeds
    public int seed;
    public Vector2 offset;

    public bool autoUpdate;//used when I change width, height, or noiseScale

    public TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for(int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight <= regions[i].height)
                    {
                        colorMap[y * mapWidth + x] = regions[i].color;
                        break;//break bc we found that we are within the right range
                    }
                }
            }
        }
        
        MapDisplay display = FindObjectOfType< MapDisplay > ();
        if(drawMode == DrawMode.NoiseMap)//this way we can either do noise or color
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if(drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        }
        
    }

    void OnValidate()
    {
        if(mapWidth < 1)
        {
            mapWidth = 1;
        }
        if(mapHeight < 1)
        {
            mapHeight = 1;
        }
        if(lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

    }
}

[System.Serializable]//makes it so it shows up in inspector
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
    
}