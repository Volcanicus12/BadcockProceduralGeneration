using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;


//ai autocomplete for visual studios

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColorMap, Mesh};
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    //mesh combining
    public const int mapChunkSize = 241;
    [Range(0,6)]
    public int editorPreviewLOD;

    //gen map
    public float noiseScale;//literally just a zoom
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    //for mesh
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    //for seeds
    public int seed;
    public Vector2 offset;

    public bool autoUpdate;//used when I change width, height, or noiseScale

    public TerrainType[] regions;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();//queue of commands
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)//this way we can either do noise or color
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
    }

    //map threading
    public void RequestMapData(Vector2 center, Action<MapData> callback)//stuff in <> is what the method expects to get
    {
        ThreadStart threadStart = delegate//delegate is just an entrypoint for the thread
        {
            MapDataThread(center, callback);
        };
            
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        //struct holds mapdata and callback
        lock (mapDataThreadInfoQueue)//when a thread reaches here it stops all threads from accessing this area
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if(mapDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }



    //map building stuff
    MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, center + offset, normalizeMode);//center + offset is for creation site to be correct

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for(int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight >= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                    }
                    else
                    {
                        break;//breaks if a value reached is less than region's height
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);
    }

    void OnValidate()
    {
        if(lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }

    }
    struct MapThreadInfo<T>//T makes it generic so we can use it for mesh too
    {
        public readonly Action<T> callback;//readonly makes them immutable/unchangeable
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
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

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}