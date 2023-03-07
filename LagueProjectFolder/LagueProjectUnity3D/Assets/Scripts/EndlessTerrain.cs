using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;//acts as distance we must travel before bothering to change chunks...makes it so we don't have to update check every frame
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;//square it bc it is quicker than square root operation
    const float colliderGenerationDistanceThreshold = 5;//how close player must be to edge of chunk before it generates collider

    public int colliderLODIndex;
    public LODInfo[] detailLevels;
    public static float maxViewDst;//const makes it so it can't be changed in run time
    public Transform viewer; //reference to viewer pos
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    float meshWorldSize;
    int chunksVisibleInViewDst;

    //terrain chunk managers
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;

        meshWorldSize = mapGenerator.meshSettings.meshWorldSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);

        UpdateVisibleChunks();//do this to make sure that first chunks get drawn
    }

    void Update()//constantly seeing who is nearby
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        
        if(viewerPosition != viewerPositionOld)
        {
            foreach (TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }
        
        if((viewerPositionOld - viewerPosition). sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)//if the viewer has moved the necessary amount then do a visibility update
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        //set all chunks from last update to invisible
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);//gets coord where viewer is
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);
        
        //loop through surrounding chunks
        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))//update if not already updated
                {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))//if contains key...already been generated
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else//if new terrain
                    {
                        terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, meshWorldSize, detailLevels, colliderLODIndex, transform, mapMaterial));
                    }
                }
                
            }
        }
    }

    //represents terrain chunk object
    public class TerrainChunk
    {
        public Vector2 coord;

        GameObject meshObject;
        Vector2 sampleCenter;
        Bounds bounds;//bounding box

        //MapData mapData;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        int colliderLODIndex;

        HeightMap mapData;
        bool mapDataReceived;

        int previousLODIndex = -1;//-1 to force at least one update
        bool hasSetCollider;

        public TerrainChunk(Vector2 coord, float meshWorldSize, LODInfo[] detailLevels,int colliderLODIndex, Transform parent, Material material)
        {
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.colliderLODIndex = colliderLODIndex;

            sampleCenter = coord * meshWorldSize / mapGenerator.meshSettings.meshScale;
            Vector2 position = coord * meshWorldSize;
            bounds = new Bounds(position, Vector2.one * meshWorldSize);

            //instantiate plane object
            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();//you can do this bc add component returns the component that it adds
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
			meshRenderer.material = material;

            meshObject.transform.position = new Vector3(position.x, 0, position.y);//moves to right place
            meshObject.transform.parent = parent;//sets parent
            SetVisible(false);//all chunks start invis

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i=0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                lodMeshes[i].updateCallback += UpdateTerrainChunk;
                if (i == colliderLODIndex)
                {
                    lodMeshes[i].updateCallback += UpdateCollisionMesh;
                }
            }


            mapGenerator.RequestHeightMap(sampleCenter, OnMapDataReceived);
        }


        //threading
        void OnMapDataReceived(HeightMap mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            UpdateTerrainChunk();//update terrain when receive map data(this is for the initial load of the game)
        }

        //constantly seeing what chunks to load
        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));//gives smallest distance between viewer and bounding box

                bool wasVisible = isVisible();
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
                            //meshCollider.sharedMesh = lodMesh.mesh;//old line

                        }
                        else if (!lodMesh.hasRequestedMesh)//if hasn't requested a mesh
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                }

                if (wasVisible != visible)
                {
                    if (visible)
                    {
                        visibleTerrainChunks.Add(this);//just adds self to visible if it is visible
                    }
                    else
                    {
                        visibleTerrainChunks.Remove(this);
                    }
                    SetVisible(visible);//if visible is true then makes it visible, but if false then doesn't
                }
            }
        }

        public void UpdateCollisionMesh()
        {
            if (!hasSetCollider)//only do this stuff if we haven't set a collider yet
            {
                float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

                if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold)
                {
                    if (!lodMeshes[colliderLODIndex].hasRequestedMesh)//request a mesh if hasn't been requested
                    {
                        lodMeshes[colliderLODIndex].RequestMesh(mapData);
                    }
                }

                if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
                {
                    if (lodMeshes[colliderLODIndex].hasMesh)
                    {
                        meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                        hasSetCollider = true;
                    }
                }
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
        public event System.Action updateCallback;
        public LODMesh(int lod)
        {
            this.lod = lod;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();//makes it so that when we receive mesh data we call the updateCallback (so we get an initial chunk update)
        }

        public void RequestMesh(HeightMap mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        [Range(0,MeshSettings.numSupportedLODs-1)]
        public int lod;
        public float visibleDstThreshold;

        public float sqrVisibleDstThreshold
        {
            get
            {
                return visibleDstThreshold * visibleDstThreshold;
            }
        }
    }

}
