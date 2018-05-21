using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MHelperFunctions
{

    // 浮点数的精度判等
    public static bool FloatEqual(float a, float b)
    {
        return Mathf.Abs(a - b) < MDefinitions.FLOAT_PRECISION;
    }

    // 浮点数与零的比较
    public static int FloatZero(float f)
    {
        if (Mathf.Abs(f) < MDefinitions.FLOAT_PRECISION)
        {
            return 0;
        }
        if (f < 0) return -1;
        else return 1;
    }

    // 判断向量是否平行
    public static bool Parallel(Vector3 v1, Vector3 v2)
    {
        v1 = Vector3.Normalize(v1);
        v2 = Vector3.Normalize(v2);
        return FloatEqual(Mathf.Abs(Vector3.Dot(v1, v2)),1);
    }

    // 判断向量是否垂直
    public static bool Perpendicular(Vector3 v1, Vector3 v2)
    {
        v1 = Vector3.Normalize(v1);
        v2 = Vector3.Normalize(v2);
        return FloatEqual(Vector3.Dot(v1, v2), 0);
    }

    // 计算点到平面的距离
    public static float DistanceP2F(Vector3 point, Vector3 faceNormal, Vector3 facePoint)
    {
        return Mathf.Abs(Vector3.Dot(faceNormal, point - facePoint));
    }

    // 计算点在平面上的投影点
    public static Vector3 PointProjectionInFace(Vector3 point, Vector3 faceNormal, Vector3 facePoint)
    {
        return point - faceNormal * Vector3.Dot(faceNormal, point - facePoint);
    }
    
    // 根据旋转前后向量计算旋转轴和旋转角
    public static void CalcRotateAxisAndAngle(out Vector3 rotateAxis, out float rotateAngle, Vector3 oldVec, Vector3 newVec)
    {
        rotateAxis = Vector3.Normalize(Vector3.Cross(oldVec, newVec));
        rotateAngle = Mathf.Acos(Vector3.Dot(oldVec, newVec));
    }

    // 根据旋转轴和旋转角计算给定向量（或点）旋转后的结果
    public static Vector3 CalcRotate(Vector3 r, Vector3 rotateAxis, float rotateAngle)
    {
        float cos = Mathf.Cos(rotateAngle);
        float sin = Mathf.Sin(rotateAngle);
        return r * cos + (1 - cos) * Vector3.Dot(rotateAxis, r) * rotateAxis + sin * Vector3.Cross(rotateAxis, r);
    }

    // 判断点是否在多边形内（点为在多边形平面上的投影点）
    public static bool InPolygon(Vector3 point, List<Vector3> points)
    {
        bool res = false;
        int i, j;
        int count = points.Count;

        for (i = 0, j = count - 1; i < count; j = i++)
        {
            if (((points[i].y > point.y) != (points[j].y > point.y)) &&
                (point.x < (points[j].x - points[i].x) * (point.y - points[i].y) / (points[j].y - points[i].y) + points[i].x))
            {
                res = !res;
            }
        }
        return res;
    }

    // 计算三个点围成面积
    public static float TriangleSurface(Vector3 a, Vector3 b, Vector3 c)
    {
        float la = Vector3.Distance(a, b);
        float lb = Vector3.Distance(b, c);
        float lc = Vector3.Distance(c, a);

        float p = (la + lb + lc) / 2;
        return Mathf.Sqrt(p * (p - la) * (p - lb) * (p - lc));
    }
}
