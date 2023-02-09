using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDst = 450;//const makes it so it can't be changed in run time
    public Transform viewer; //reference to viewer pos

    public static Vector2 viewerPosition;
    int chunkSize;
    int chunksVisibleInViewDst;

    //terrain chunk managers
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        chunkSize = MapGenerator.mapChunkSize - 1;//bc our actual map chunk size is 1 less than what we say
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
    }

    void Update()//constantly seeing who is nearby
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
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
                    if (terrainChunkDictionary[viewedChunkCoord].isVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else//if new terrain
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform));
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

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);//need v3 bc using 3d space

            //instantiate plane object
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;//moves to right place
            meshObject.transform.localScale = Vector3.one * size / 10f;//use 10 bc primitive plane is 10 units...v3.one is just a v3 where everything is set to 1
            meshObject.transform.parent = parent;//sets parent
            SetVisible(false);//all chunks start invis
        }

        //constantly seeing what chunks to load
        public void UpdateTerrainChunk()
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));//gives smallest distance between viewer and bounding box
            bool visible = viewerDstFromNearestEdge <= maxViewDst;
            SetVisible(visible);//if visible is true then makes it visible, but if false then doesn't
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

}
