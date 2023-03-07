using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float [,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
    {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;//if LOD is == 0 then set to 1, but otherwise do LOD*2

        //gets map details
        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2 * meshSimplificationIncrement;//this is basically an extension of where the border will go so we can calculate normals, but won't be shown in final mesh
        int meshSizeUnsimplified = borderedSize - 2;//use this for things like calculating topleftX and Z

        float topLeftX = (meshSizeUnsimplified - 1) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1) / 2f;

        int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;


        MeshData meshData = new MeshData(verticesPerLine);

        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                //just used for whether or not a vertex is border or mesh...remember that borders are negative and mesh is positive
                if (isBorderVertex)
                {
                    vertexIndicesMap[x, y] = borderVertexIndex;
                    borderVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        //loops through height map
        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {
                //get vertexIndex from vertexIndices map
                int vertexIndex = vertexIndicesMap[x, y];

                Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);//var-meshSimpInc is used bc x or y = 0 is not part of the mesh...makes it so our UV's are still properly centered
                float height = heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if (x < borderedSize - 1 && y < borderedSize - 1) //ignores right and bottom edges of the map when making triangles
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x + meshSimplificationIncrement, y];
                    int c = vertexIndicesMap[x, y + meshSimplificationIncrement];
                    int d = vertexIndicesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];

                    meshData.AddTriangle(a,d,c);
                    meshData.AddTriangle(d,a,b);
                }

                vertexIndex++;//keeps track of where we are in our 1D array (MeshData)


            }
        }

        meshData.BakeNormals();

        return meshData;//return meshData to help with threading implementation
    }
}


public class MeshData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Vector3[] bakedNormals;//speeds up normal calculations

    Vector3[] borderVertices;
    int[] borderTriangles;

    int triangleIndex;
    int borderTriangleIndex;

    public MeshData(int verticesPerLine)
    {
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        uvs = new Vector2[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

        borderVertices = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[24 * verticesPerLine];//24 comes from 6 lines * 4 vertices * vertices per line
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)//see if this vertex is a border or not
        {//add to border vertice array
            borderVertices[-vertexIndex - 1] = vertexPosition;//-1 so we start at 0
        }
        else//if it is a regular vertex
        {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c)
    {
        if(a < 0 || b < 0 || c < 0)
        {//it is a border triangle
            borderTriangles[borderTriangleIndex] = a;
            borderTriangles[borderTriangleIndex + 1] = b;
            borderTriangles[borderTriangleIndex + 2] = c;
            borderTriangleIndex += 3;
        }
        else
        {//regular triangle
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }

        
    }

    Vector3[] CalculateNormals()//this is our own version of RecalculateNormals(). It is meant to fix the seams between meshes
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];//stores vertex normals
        int triangleCount = triangles.Length / 3;//tells us how many triangles we have
    
        //loop through triangles
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;//gets us location in triangle array because each triangle has 3 vertices in the array
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex+1];
            int vertexIndexC = triangles[normalTriangleIndex+2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        //loop through border triangles
        int borderTriangleCount = borderTriangles.Length / 3;//tells us how many triangles we have
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;//gets us location in triangle array because each triangle has 3 vertices in the array
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)//make sure that vertexIndexABC is >= 0
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if(vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if(vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
            
        }


        //normalize each value in array
        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    //when given 3 vertex indices this will return normal vector of triangle
    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];//if indexA < 0 border vertices otherwise vertices
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

        //use cross product to get surface normal
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        //mesh.RecalculateNormals();
        mesh.normals = bakedNormals;
        return mesh;
    }
}