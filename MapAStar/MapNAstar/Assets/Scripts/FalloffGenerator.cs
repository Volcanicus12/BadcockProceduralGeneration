using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator //makes it so outside of map is just water
{
    public static float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        //populate map with values
        for (int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;//use float so we don't round down to zero
                float y = j / (float)size * 2 - 1;

                //find out which is closer to edge of square...x or y
                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    //Evaluate is to make it so the water barrier is more placeable
    static float Evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
