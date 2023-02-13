using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    const float scale = 1f;//makes it easy to scale map

    const float viewerMoveThresholdForChunkUpdate = 25f;//acts as distance we must travel before bothering to change chunks...makes it so we don't have to update check every frame
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;//square it bc it is quicker than square root operation

    public LODInfo[] detailLevels;
    public static float maxViewDst;//const makes it so it can't be changed in run time
    public Transform viewer; //reference to viewer pos
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleInViewDst;

    //terrain chunk managers
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;

        chunkSize = MapGenerator.mapChunkSize - 1;//bc our actual map chunk size is 1 less than what we say
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        UpdateVisibleChunks();//do this to make sure that first chunks get drawn
    }

    void Update()//constantly seeing who is nearby
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale ;
        if((viewerPositionOld - viewerPosition). sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)//if the viewer has moved the necessary amount then do a visibility update
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        //set all chunks from last update to invisible
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();//clears list

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);//gets coord where viewer is
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        
        //loop through surrounding chunks
        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))//if contains key...already been generated
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    //if (terrainChunkDictionary[viewedChunkCoord].isVisible())
                    //{
                    //    terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    //}this introduced an error that basically meant we updated visible here and then later in a callback
                }
                else//if new terrain
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }

    //represents terrain chunk object
    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;//bounding box

        //MapData mapData;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;

        MapData mapData;
        bool mapDataReceived;

        int previousLODIndex = -1;//-1 to force at least one update

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);//need v3 bc using 3d space

            //instantiate plane object
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();//you can do this bc add component returns the component that it adds
            meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer.material = material;

            meshObject.transform.position = positionV3 * scale;//moves to right place
            meshObject.transform.parent = parent;//sets parent
            meshObject.transform.localScale = Vector3.one * scale;
            SetVisible(false);//all chunks start invis

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i=0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }


            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }


        //threading
        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            //gets color textures on
            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();//update terrain when receive map data(this is for the initial load of the game)
        }

        //constantly seeing what chunks to load
        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));//gives smallest distance between viewer and bounding box
                bool visible = viewerDstFromNearestEdge <= maxViewDst;

                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++)//don't look at last one bc it will always be greater than max view dst
                    {
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)//if hasn't requested a mesh
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    terrainChunksVisibleLastUpdate.Add(this);//just adds self to visible if it is visible
                }
                SetVisible(visible);//if visible is true then makes it visible, but if false then doesn't
            }
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool isVisible()
        {
            return meshObject.activeSelf;
        }
    }


    //lod is level of detail
    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;
        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();//makes it so that when we receive mesh data we call the updateCallback (so we get an initial chunk update)
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDstThreshold;
    }

}
