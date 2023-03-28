using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Line
{
    const float verticalLineGradient = 1e5f;

    float gradient;
    float y_Intercept;

    Vector2 pointOnLine_1;
    Vector2 pointOnLine_2;

    float gradientPerpendicular;

    bool approachSide;

    public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine)
    {//d = delta
        float dx = pointOnLine.x - pointPerpendicularToLine.x;
        float dy = pointOnLine.y - pointPerpendicularToLine.y;

        if (dx == 0)//if dx is 0 then it is a straight line and would return undefined if we did the else
        {
            gradientPerpendicular = verticalLineGradient;
        }
        else
        {
            gradientPerpendicular = dy / dx;
        }

        //a line times its perpendicular is -1
        if (gradientPerpendicular == 0)
        {
            gradient = verticalLineGradient;
        }
        else
        {
            gradient = -1 / gradientPerpendicular;
        }

        y_Intercept = pointOnLine.y - gradient * pointOnLine.x;

        pointOnLine_1 = pointOnLine;
        pointOnLine_2 = pointOnLine + new Vector2(1, gradient);//anywhere on the line

        approachSide = false;
        approachSide = GetSide(pointPerpendicularToLine);
    }

    //this is for finding out if we are beyond boundary
    bool GetSide(Vector2 p)
    {
        return (p.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) > (p.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);//wathc later video to understand this
    }

    //see if we have crossed perpendicular point
    public bool HasCrossedLine(Vector2 p)
    {
        return GetSide(p) != approachSide;
    }

    public void DrawWithGizmos(float length)
    {
        Vector3 lineDir = new Vector3(1, 0, gradient).normalized;
        Vector3 lineCenter = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up;//gets center line and raises high enough to see
        Gizmos.DrawLine(lineCenter - lineDir * length / 2f, lineCenter + lineDir * length / 2f);//draws a line between two points
    }
}
