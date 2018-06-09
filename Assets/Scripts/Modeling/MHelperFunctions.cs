﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MHelperFunctions
{

	public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){

		Vector3 lineVec3 = linePoint2 - linePoint1;
		Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
		Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

		float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);


		if(Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
		{
			float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
			intersection = linePoint1 + (lineVec1 * s);
			return true;
		}
		else
		{
			intersection = Vector3.zero;
			return false;
		}
	}

	public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){

		closestPointLine1 = Vector3.zero;
		closestPointLine2 = Vector3.zero;

		float a = Vector3.Dot(lineVec1, lineVec1);
		float b = Vector3.Dot(lineVec1, lineVec2);
		float e = Vector3.Dot(lineVec2, lineVec2);

		float d = a*e - b*b;

		//lines are not parallel
		if(d != 0.0f){

			Vector3 r = linePoint1 - linePoint2;
			float c = Vector3.Dot(lineVec1, r);
			float f = Vector3.Dot(lineVec2, r);

			float s = (b*f - c*e) / d;
			float t = (a*f - c*b) / d;

			closestPointLine1 = linePoint1 + lineVec1 * s;
			closestPointLine2 = linePoint2 + lineVec2 * t;

			return true;
		}

		else{
			return false;
		}
	}  

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

    // 线圈上任意一点
    public static Vector3 RandomPointInCircle(Vector3 center, Vector3 normal, float radius)
    {
        while (true)
        {
            Vector3 dir = new Vector3(Random.value, Random.value, Random.value);
            if (!MHelperFunctions.Parallel(dir, normal))
            {
                return (dir - Vector3.Dot(dir, normal) * normal).normalized * radius + center;
            }
        }
    }

    // 判断向量是否平行
    public static bool Parallel(Vector3 v1, Vector3 v2)
    {
        return FloatEqual(Mathf.Abs(Vector3.Dot(v1.normalized, v2.normalized)),1);
    }

    // 判断向量是否垂直
    public static bool Perpendicular(Vector3 v1, Vector3 v2)
    {
        return FloatEqual(Vector3.Dot(v1, v2), 0);
    }

    // 计算两向量之间的锐角角度
    public static float CalcAngle(Vector3 v1, Vector3 v2)
    {
        float angle = Mathf.Acos(Vector3.Dot(v1.normalized, v2.normalized)) * Mathf.Rad2Deg;
        if (angle > 90) angle = 180 - angle;
        return angle;
    }

    public static Vector3 Min(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Min(a, Vector3.Min(b, c));
    }

    public static Vector3 Max(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Max(a, Vector3.Max(b, c));
    }

    public static List<Vector3> GenerateArcPoint(Vector3 start, Vector3 end, Vector3 normal, Vector3 center)
    {
        float angle = Mathf.PI / 24;
        Vector3 cross = Vector3.Cross((end - center).normalized, (start - center).normalized);
        float totalAngle = Mathf.Acos(Vector3.Dot((end - center).normalized, (start - center).normalized));
        if (Vector3.Dot(cross, normal) > 0) totalAngle = Mathf.PI * 2 - totalAngle;
        List<Vector3> points = new List<Vector3>();
        Vector3 p = start;
        for(float count = angle; count < totalAngle; count += angle)
        {
            points.Add(p);
            p = MHelperFunctions.CalcRotate(p - center, normal, angle) + center;
        }
        points.Add(end);
        return points;
    }

    // 计算直线与平面的交点
    public static Vector3 IntersectionLineWithFace(Vector3 lineDir, Vector3 linePoint, Vector3 normal, Vector3 facePoint)
    {
        lineDir.Normalize();
        normal.Normalize();
        return linePoint + lineDir * (Vector3.Dot(facePoint - linePoint, normal) / Vector3.Dot(lineDir, normal));
    }

    // 计算点到三角形的距离
    public static float DistanceP2T(Vector3 point, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        List<Vector3> points = new List<Vector3> { p1, p2, p3 };
        Vector3 normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;
        Vector3 projectionPoint = PointProjectionInFace(point, normal, p1);
        Vector3 rotateAxis;
        float rotateAngle;
        CalcRotateAxisAndAngle(out rotateAxis, out rotateAngle, normal, new Vector3(0, 0, 1));
        List<Vector3> rotatePoints = new List<Vector3>();
        foreach (Vector3 p in points)
        {
            rotatePoints.Add(CalcRotate(p, rotateAxis, rotateAngle));
        }
        projectionPoint = CalcRotate(projectionPoint, rotateAxis, rotateAngle);
        if (InPolygon(projectionPoint, rotatePoints))
        {
            return DistanceP2F(point, normal, points[0]);
        }
        else
        {
            float min = float.MaxValue;
            for (int i = 0; i < 3; i++)
            {
                min = Mathf.Min(min, DistanceP2S(point, points[i], points[(i + 1) % 3]));
            }
            return min;
        }
    }

    // 计算点到线段的距离
    public static float DistanceP2S(Vector3 point, Vector3 start, Vector3 end)
    {
        float d1 = Vector3.Dot(end - start, point - start);
        if (MHelperFunctions.FloatZero(d1) <= 0)
        {
            return Vector3.Distance(point, start);
        }
        float d2 = Vector3.Dot(end - start, end - start);
        if (d1 >= d2)
        {
            return Vector3.Distance(point, end);
        }
        float r = d1 / d2;
        return Vector3.Distance(point, r * end + (1 - r) * start);
    }

    // 计算点到平面的距离
    public static float DistanceP2F(Vector3 point, Vector3 faceNormal, Vector3 facePoint)
    {
        return Mathf.Abs(Vector3.Dot(faceNormal.normalized, point - facePoint));
    }

    //计算点到直线的距离
	public static float DistanceP2L(Vector3 point, Vector3 lineVector, Vector3 linePoint)
	{
        Vector3 p = PointProjectionInLine(point, lineVector, linePoint);
		return Vector3.Distance(point, p);
	}

    //计算直线到直线的距离
    public static float DistanceL2L(Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        Vector3 temp1, temp2;
        if (Parallel(lineVec1, lineVec2))
        {
            return DistanceP2L(linePoint1, lineVec2, linePoint2);
        }
        else if (LineLineIntersection(out temp1, linePoint1, lineVec1, linePoint2, lineVec2))
        {
            return 0;
        }
        else
        {
            if (ClosestPointsOnTwoLines(out temp1, out temp2, linePoint1, lineVec1, linePoint2, lineVec2))
            {
                return DistanceP2L(temp2, lineVec1, linePoint1);
            }
            else
            {
                return 0;
            }
        }
    }

    // 计算点在平面上的投影点
    public static Vector3 PointProjectionInFace(Vector3 point, Vector3 faceNormal, Vector3 facePoint)
    {
        return point - faceNormal.normalized * Vector3.Dot(faceNormal.normalized, point - facePoint);
    }

    // 计算点在直线上的投影点
    public static Vector3 PointProjectionInLine(Vector3 point, Vector3 lineDirection, Vector3 linePoint)
    {
        return linePoint + Vector3.Dot(lineDirection.normalized, point - linePoint) * lineDirection.normalized;
    }
    
    // 根据旋转前后向量计算旋转轴和旋转角
    public static void CalcRotateAxisAndAngle(out Vector3 rotateAxis, out float rotateAngle, Vector3 oldVec, Vector3 newVec)
    {
        rotateAxis = Vector3.Cross(oldVec.normalized, newVec.normalized).normalized;
        rotateAngle = Mathf.Acos(Vector3.Dot(oldVec.normalized, newVec.normalized));
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

    // 分割网格
    public static List<Mesh> MeshSplit(Mesh mesh, Vector3 normal, Vector3 planePoint, out List<List<Vector3>> splits)
    {
        List<Vector3> vertices1 = new List<Vector3>();
        List<Vector3> vertices2 = new List<Vector3>();
        List<int> triangles1 = new List<int>();
        List<int> triangles2 = new List<int>();
        List<Vector3> splitPointPair = new List<Vector3>();
        foreach(Vector3 p in mesh.vertices)
        {
            int r = FloatZero(Vector3.Dot(p - planePoint, normal));
            if(r == 0)
            {
                vertices1.Add(p);
                vertices2.Add(p);
            }
            else if(r > 0)
            {
                vertices1.Add(p);
            }
            else
            {
                vertices2.Add(p);
            }
        }
        int count = mesh.triangles.Length / 3;
        for(int i = 0; i < count; i++)
        {
            int i1 = mesh.triangles[3 * i];
            int i2 = mesh.triangles[3 * i + 1];
            int i3 = mesh.triangles[3 * i + 2];
            Vector3 p1 = mesh.vertices[i1];
            Vector3 p2 = mesh.vertices[i2];
            Vector3 p3 = mesh.vertices[i3];

            int r1 = FloatZero(Vector3.Dot(p1 - planePoint, normal));
            int r2 = FloatZero(Vector3.Dot(p2 - planePoint, normal));
            int r3 = FloatZero(Vector3.Dot(p3 - planePoint, normal));

            if(r1 == 0 && r2 == 0 && r3 == 0)
            {
                triangles1.Add(vertices1.IndexOf(p1));
                triangles1.Add(vertices1.IndexOf(p2));
                triangles1.Add(vertices1.IndexOf(p3));
                triangles2.Add(vertices2.IndexOf(p1));
                triangles2.Add(vertices2.IndexOf(p2));
                triangles2.Add(vertices2.IndexOf(p3));
            }
            else if(r1 >= 0 && r2 >= 0 && r3 >= 0)
            {
                triangles1.Add(vertices1.IndexOf(p1));
                triangles1.Add(vertices1.IndexOf(p2));
                triangles1.Add(vertices1.IndexOf(p3));
            }
            else if(r1 <= 0 && r2 <= 0 && r3 <= 0)
            {
                triangles2.Add(vertices2.IndexOf(p1));
                triangles2.Add(vertices2.IndexOf(p2));
                triangles2.Add(vertices2.IndexOf(p3));
            }
            else if(r1 < 0 && r2 >= 0 && r3 >= 0)
            {
                SplitTriangleMesh(p1, p2, p3,
                    ref vertices1, ref triangles1,
                    ref vertices2, ref triangles2,
                    ref splitPointPair, normal, planePoint);
            }
            else if(r1 > 0 && r2 <= 0 && r3 <= 0)
            {
                SplitTriangleMesh(p1, p2, p3,
                    ref vertices2, ref triangles2,
                    ref vertices1, ref triangles1,
                    ref splitPointPair, normal, planePoint);
            } else if(r2 < 0 && r1 >= 0 && r3 >= 0)
            {
                SplitTriangleMesh(p2, p3, p1,
                    ref vertices1, ref triangles1,
                    ref vertices2, ref triangles2,
                    ref splitPointPair, normal, planePoint);
            } else if(r2 > 0 && r1 <= 0 && r3 <= 0)
            {
                SplitTriangleMesh(p2, p3, p1,
                    ref vertices2, ref triangles2,
                    ref vertices1, ref triangles1,
                    ref splitPointPair, normal, planePoint);
            } else if(r3 < 0 && r1 >= 0 && r2 >= 0)
            {
                SplitTriangleMesh(p3, p1, p2,
                    ref vertices1, ref triangles1,
                    ref vertices2, ref triangles2,
                    ref splitPointPair, normal, planePoint);
            } else if(r3 > 0 && r1 <= 0 && r2 <= 0)
            {
                SplitTriangleMesh(p3, p1, p2,
                    ref vertices2, ref triangles2,
                    ref vertices1, ref triangles1,
                    ref splitPointPair, normal, planePoint);
            }
            else
            {
                Debug.Log("r1: " + r1 + ", r2: " + r2 + ", r3: " + r3);
            }
        }
        Mesh mesh1 = new Mesh();
        Mesh mesh2 = new Mesh();
        mesh1.vertices = vertices1.ToArray();
        mesh1.triangles = triangles1.ToArray();
        mesh2.vertices = vertices2.ToArray();
        mesh2.triangles = triangles2.ToArray();
        mesh1.RecalculateNormals();
        mesh2.RecalculateNormals();
        List<Mesh> meshs = new List<Mesh>();
        meshs.Add(mesh1);
        meshs.Add(mesh2);
        // TODO: 根据splitPointPair生成splits
        splits = new List<List<Vector3>>();
        return meshs;
    }

    private static void SplitTriangleMesh(Vector3 p1, Vector3 p2, Vector3 p3,
        ref List<Vector3> vertices1, ref List<int> triangles1, 
        ref List<Vector3> vertices2, ref List<int> triangles2,
        ref List<Vector3> splitPointPair, Vector3 normal, Vector3 planePoint) 
    {
        Vector3 sec12 = IntersectionLineWithFace(p2 - p1, p1, normal, planePoint);
        Vector3 sec13 = IntersectionLineWithFace(p3 - p1, p1, normal, planePoint);
        splitPointPair.Add(sec12);
        splitPointPair.Add(sec13);
        vertices1.Add(sec12);
        vertices1.Add(sec13);
        vertices2.Add(sec12);
        vertices2.Add(sec13);
        triangles1.Add(vertices1.IndexOf(sec12));
        triangles1.Add(vertices1.IndexOf(p2));
        triangles1.Add(vertices1.IndexOf(sec13));
        triangles1.Add(vertices1.IndexOf(sec13));
        triangles1.Add(vertices1.IndexOf(p2));
        triangles1.Add(vertices1.IndexOf(p3));
        triangles2.Add(vertices2.IndexOf(p1));
        triangles2.Add(vertices2.IndexOf(sec12));
        triangles2.Add(vertices2.IndexOf(sec13));
    }
}
