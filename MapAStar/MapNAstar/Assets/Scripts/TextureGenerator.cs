using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class TextureGenerator 
{
    //creates texture from color map
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;//fixes blurry map...instead of by linear
        texture.wrapMode = TextureWrapMode.Clamp;//fixes the texture wrapping
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    //returns texture based on 2D height map
    public static Texture2D TextureFromHeightMap(HeightMap heightMap)
    {
        int width = heightMap.values.GetLength(0);
        int height = heightMap.values.GetLength(1);

        Color[] colorMap = new Color[width * height];//makes color array
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(heightMap.minValue, heightMap.maxValue, heightMap.values[x, y]));//color map is 1 dimensional array and noiseMap is 2 dimensional
            }
        }

        return TextureFromColorMap(colorMap, width, height);
    }
    
}
