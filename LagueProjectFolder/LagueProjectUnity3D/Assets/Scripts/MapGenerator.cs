using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;


//ai autocomplete for visual studios

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, Mesh, FalloffMap};//ColorMap, Mesh, FalloffMap};
    public DrawMode drawMode;

    //mesh combining
    //public const int mapChunkSize = 95;//96 is divisible by every even number but 10 up to 12...-1...we can't use LOD of 5 if we flatshade


    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureData;

    public Material terrainMaterial;

    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int editorPreviewLOD;//Level of detail

    public bool autoUpdate;//used when I change width, height, or noiseScale

    float[,] falloffMap;//stores falloff map

    Queue<MapThreadInfo<HeightMap>> heightMapThreadInfoQueue = new Queue<MapThreadInfo<HeightMap>>();//queue of commands
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    void Start()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);
    }

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();//if we are in editor then draw new map when values updated
        }
    }

    void OnTextureValuesUpdated()//if texture data updated
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    

    public void DrawMapInEditor()
    {
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);//this is up here because it needs to happen earlier in the same thread to work

        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)//this way we can either do noise or color
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap.values));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine)));
        }
    }

    //map threading
    public void RequestHeightMap(Vector2 center, Action<HeightMap> callback)//stuff in <> is what the method expects to get
    {
        ThreadStart threadStart = delegate//delegate is just an entrypoint for the thread
        {
            HeightMapThread(center, callback);
        };
            
        new Thread(threadStart).Start();
    }

    void HeightMapThread(Vector2 center, Action<HeightMap> callback)
    {
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, center);
        //struct holds mapdata and callback
        lock (heightMapThreadInfoQueue)//when a thread reaches here it stops all threads from accessing this area
        {
            heightMapThreadInfoQueue.Enqueue(new MapThreadInfo<HeightMap>(callback, heightMap));
        }
    }

    public void RequestMeshData(HeightMap heightMap, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate {
            MeshDataThread(heightMap, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(HeightMap heightMap, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if(heightMapThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < heightMapThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<HeightMap> threadInfo = heightMapThreadInfoQueue.Dequeue();
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


    void OnValidate()
    {

        if(meshSettings != null)//unsubscribe and then resubscribe
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;//does nothing if not subscribed yet, but unsubs if already subbed
            meshSettings.OnValuesUpdated += OnValuesUpdated;//subscribes
        }
        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if(textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
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


