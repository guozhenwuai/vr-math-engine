using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MHelperFunctions
{

    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);


        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
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

    public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2) {

        closestPointLine1 = Vector3.zero;
        closestPointLine2 = Vector3.zero;

        float a = Vector3.Dot(lineVec1, lineVec1);
        float b = Vector3.Dot(lineVec1, lineVec2);
        float e = Vector3.Dot(lineVec2, lineVec2);

        float d = a * e - b * b;

        //lines are not parallel
        if (d != 0.0f) {

            Vector3 r = linePoint1 - linePoint2;
            float c = Vector3.Dot(lineVec1, r);
            float f = Vector3.Dot(lineVec2, r);

            float s = (b * f - c * e) / d;
            float t = (a * f - c * b) / d;

            closestPointLine1 = linePoint1 + lineVec1 * s;
            closestPointLine2 = linePoint2 + lineVec2 * t;

            return true;
        }

        else {
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
        return FloatEqual(Mathf.Abs(Vector3.Dot(v1.normalized, v2.normalized)), 1);
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

    public static float CalcRealAngle(Vector3 v1, Vector3 v2)
    {
        return Mathf.Acos(Vector3.Dot(v1.normalized, v2.normalized)) * Mathf.Rad2Deg;
    }

    public static float CalcRadAngle(Vector3 v1, Vector3 v2)
    {
        return Mathf.Acos(Vector3.Dot(v1.normalized, v2.normalized));
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
        for (float count = angle; count < totalAngle; count += angle)
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

    //计算直线到一点的向量
    public static Vector3 VectorL2P(Vector3 point, Vector3 lineDirection, Vector3 linePoint)
    {
        return point - linePoint - lineDirection.normalized * Vector3.Dot(lineDirection.normalized, point - linePoint);
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

    public static Vector3 CalcDirectionByAngle(Vector3 oldVec, Vector3 newVec, float angle)
    {
        Vector3 rotateAxis = Vector3.Cross(oldVec.normalized, newVec.normalized).normalized;
        return CalcRotate(oldVec, rotateAxis, angle);
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

    public static bool PointsInLine(List<Vector3> points)
    {
        if (points.Count < 2) return false;
        if (points.Count == 2) return true;
        Vector3 dir = points[1] - points[0];
        int count = points.Count;
        for (int i = 1; i < count - 1; i++)
        {
            if (!Parallel(dir, points[i + 1] - points[i])) return false;
        }
        return true;
    }

    // 分割网格
    public static List<Mesh> MeshSplit(Mesh mesh, Vector3 normal, Vector3 planePoint, out List<List<Vector3>> splits)
    {
        List<Vector3> vertices1 = new List<Vector3>();
        List<Vector3> vertices2 = new List<Vector3>();
        List<int> triangles1 = new List<int>();
        List<int> triangles2 = new List<int>();
        Dictionary<Vector3, List<Vector3>> pointGraph = new Dictionary<Vector3, List<Vector3>>();
        foreach (Vector3 p in mesh.vertices)
        {
            int r = FloatZero(Vector3.Dot(p - planePoint, normal));
            if (r == 0)
            {
                vertices1.Add(p);
                vertices2.Add(p);
            }
            else if (r > 0)
            {
                vertices1.Add(p);
            }
            else
            {
                vertices2.Add(p);
            }
        }
        int count = mesh.triangles.Length / 3;
        for (int i = 0; i < count; i++)
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

            if (r1 == 0 && r2 == 0 && r3 == 0)
            {
                triangles1.Add(vertices1.IndexOf(p1));
                triangles1.Add(vertices1.IndexOf(p2));
                triangles1.Add(vertices1.IndexOf(p3));
                triangles2.Add(vertices2.IndexOf(p1));
                triangles2.Add(vertices2.IndexOf(p2));
                triangles2.Add(vertices2.IndexOf(p3));
            }
            else if (r1 >= 0 && r2 >= 0 && r3 >= 0)
            {
                triangles1.Add(vertices1.IndexOf(p1));
                triangles1.Add(vertices1.IndexOf(p2));
                triangles1.Add(vertices1.IndexOf(p3));
            }
            else if (r1 <= 0 && r2 <= 0 && r3 <= 0)
            {
                triangles2.Add(vertices2.IndexOf(p1));
                triangles2.Add(vertices2.IndexOf(p2));
                triangles2.Add(vertices2.IndexOf(p3));
            }
            else if (r1 < 0 && r2 >= 0 && r3 >= 0)
            {
                SplitTriangleMesh(p1, p2, p3,
                    ref vertices1, ref triangles1,
                    ref vertices2, ref triangles2,
                    ref pointGraph, normal, planePoint);
            }
            else if (r1 > 0 && r2 <= 0 && r3 <= 0)
            {
                SplitTriangleMesh(p1, p2, p3,
                    ref vertices2, ref triangles2,
                    ref vertices1, ref triangles1,
                    ref pointGraph, normal, planePoint);
            } else if (r2 < 0 && r1 >= 0 && r3 >= 0)
            {
                SplitTriangleMesh(p2, p3, p1,
                    ref vertices1, ref triangles1,
                    ref vertices2, ref triangles2,
                    ref pointGraph, normal, planePoint);
            } else if (r2 > 0 && r1 <= 0 && r3 <= 0)
            {
                SplitTriangleMesh(p2, p3, p1,
                    ref vertices2, ref triangles2,
                    ref vertices1, ref triangles1,
                    ref pointGraph, normal, planePoint);
            } else if (r3 < 0 && r1 >= 0 && r2 >= 0)
            {
                SplitTriangleMesh(p3, p1, p2,
                    ref vertices1, ref triangles1,
                    ref vertices2, ref triangles2,
                    ref pointGraph, normal, planePoint);
            } else if (r3 > 0 && r1 <= 0 && r2 <= 0)
            {
                SplitTriangleMesh(p3, p1, p2,
                    ref vertices2, ref triangles2,
                    ref vertices1, ref triangles1,
                    ref pointGraph, normal, planePoint);
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
        splits = GroupSplits(pointGraph);
        return meshs;
    }

    public static List<List<Vector3>> GroupLoop(Dictionary<Vector3, List<KeyValuePair<Vector3, int>>> splitGraph, List<List<Vector3>> splitPoints)
    {
        List<List<Vector3>> res = new List<List<Vector3>>();
        FindLoop(splitGraph, splitPoints, res);
        return res;
    }

    public static void FindLoop(Dictionary<Vector3, List<KeyValuePair<Vector3, int>>> splitGraph, List<List<Vector3>> splitPoints, List<List<Vector3>> res)
    {
        Delete1Degree(splitGraph);
        if(splitGraph.Count < 2)
        {
            splitGraph.Clear();
            return;
        }
        KeyValuePair<Vector3, List<KeyValuePair<Vector3, int>>> pair = new KeyValuePair<Vector3, List<KeyValuePair<Vector3, int>>>();
        foreach (KeyValuePair<Vector3, List<KeyValuePair<Vector3, int>>> pp in splitGraph)
        {
            pair = pp;
            break;
        }
        List<Vector3> temp = new List<Vector3>();
        List<Vector3> points = new List<Vector3>();
        int lastIndex = -1;
        Vector3 p = pair.Key;
        temp.Add(p);
        List<KeyValuePair<Vector3, int>> list;
        bool findLoop = false;
        KeyValuePair<Vector3, int> find = new KeyValuePair<Vector3, int>();
        while (BlurTryGetValue(splitGraph, p, out list))
        {
            if (list.Count < 2) break;
            foreach (KeyValuePair<Vector3, int> adj in list)
            {
                if (lastIndex >= 0 && lastIndex == adj.Value) continue;
                if (temp.Count > 0 && BlurEqual(temp[0], adj.Key))
                {
                    findLoop = true;
                    find = adj;
                    break;
                }
                else
                {
                    temp.Add(adj.Key);
                    lastIndex = adj.Value;
                    List<Vector3> pl = splitPoints[adj.Value];
                    if (BlurEqual(pl[0], p))
                    {
                        for (int i = 1; i < pl.Count; i++)
                        {
                            points.Add(pl[i]);
                        }
                    }
                    else
                    {
                        for (int i = pl.Count - 2; i >= 0; i--)
                        {
                            points.Add(pl[i]);
                        }
                    }
                    p = adj.Key;
                    break;
                }
            }
            if (findLoop) break;
        }
        if (findLoop)
        {
            List<Vector3> pl = splitPoints[find.Value];
            if(BlurEqual(pl[0], p))
            {
                for(int i = 1; i < pl.Count; i++)
                {
                    points.Add(pl[i]);
                }
            }
            else
            {
                for (int i = pl.Count - 2; i >= 0; i--)
                {
                    points.Add(pl[i]);
                }
            }
            res.Add(points);
            points = new List<Vector3>();
            RemoveFromGraph(splitGraph, temp);
            FindLoop(splitGraph, splitPoints, res);
        }
    }

    private static void RemoveFromGraph(Dictionary<Vector3, List<KeyValuePair<Vector3, int>>> splitGraph, List<Vector3> points)
    {
        List<KeyValuePair<Vector3, int>> list;
        int count = points.Count;
        for(int i = 0; i < count; i++)
        {
            if(BlurTryGetValue(splitGraph, points[i], out list))
            {
                if(list.Count <= 2)
                {
                    BlurRemove(splitGraph, points[i]);
                }
                else
                {
                    BlurRemove(list, points[(i + count - 1) % count]);
                    BlurRemove(list, points[(i + 1) % count]);
                }
            }
        }
    }

    private static void Delete1Degree(Dictionary<Vector3, List<KeyValuePair<Vector3, int>>> splitGraph)
    {
        bool hasLine = true;
        while (splitGraph.Count != 0 && hasLine)
        {
            hasLine = false;
            KeyValuePair<Vector3, List<KeyValuePair<Vector3, int>>> tempPair = new KeyValuePair<Vector3, List<KeyValuePair<Vector3, int>>>();
            foreach (KeyValuePair<Vector3, List<KeyValuePair<Vector3, int>>> pair in splitGraph)
            {
                if (pair.Value.Count == 1)
                {
                    hasLine = true;
                    tempPair = pair;
                    break;
                }
            }
            if (hasLine)
            {
                Vector3 lastp = tempPair.Key;
                splitGraph.Remove(lastp);
                Vector3 p = tempPair.Value[0].Key;
                List<KeyValuePair<Vector3, int>> list = new List<KeyValuePair<Vector3, int>>();
                if (BlurTryGetValue(splitGraph, p, out list))
                {
                    if (list.Count == 1)
                    {
                        BlurRemove(splitGraph, p);
                    }
                    else
                    {
                        BlurRemove(list, tempPair.Key);
                    }
                }
            }
        }
    }

    private static List<List<Vector3>> GroupSplits(Dictionary<Vector3, List<Vector3>> pointGraph)
    {
        List<List<Vector3>> res = new List<List<Vector3>>();
        bool hasLine = true;
        while (pointGraph.Count != 0 && hasLine)
        {
            hasLine = false;
            List<Vector3> line = new List<Vector3>();
            KeyValuePair<Vector3, List<Vector3>> tempPair = new KeyValuePair<Vector3, List<Vector3>>();
            foreach (KeyValuePair<Vector3, List<Vector3>> pair in pointGraph)
            {
                if (pair.Value.Count == 1)
                {
                    hasLine = true;
                    tempPair = pair;
                    break;
                }
            }
            if (hasLine)
            {
                Vector3 lastp = tempPair.Key;
                line.Add(lastp);
                pointGraph.Remove(lastp);
                Vector3 p = tempPair.Value[0];
                List<Vector3> list;
                while (BlurTryGetValue(pointGraph, p, out list))
                {
                    if (list.Count == 1)
                    {
                        line.Add(p);
                        BlurRemove(pointGraph, p);
                        break;
                    }
                    else if (list.Count == 2)
                    {
                        line.Add(p);
                        BlurRemove(list, lastp);
                        lastp = p;
                        p = list[0];
                        BlurRemove(pointGraph, lastp);
                    }
                    else if (list.Count > 2)
                    {
                        BlurRemove(list, lastp);
                        line.Add(p);
                        break;
                    }
                    else
                    {
                        Debug.Log("pointGraph list count = 0");
                    }
                }
                res.Add(line);
            }
        }
        if (pointGraph.Count == 0) return res;
        bool connectLoop = false;
        foreach (KeyValuePair<Vector3, List<Vector3>> pair in pointGraph)
        {
            if (pair.Value.Count > 2)
            {
                connectLoop = true;
                break;
            }
        }
        if (connectLoop)
        {
            Debug.Log("Unhandled connectloop");
            return res;
        }
        bool hasLoop = true;
        while (pointGraph.Count != 0 && hasLoop)
        {
            hasLoop = false;
            List<Vector3> loop = new List<Vector3>();
            KeyValuePair<Vector3, List<Vector3>> tempPair = new KeyValuePair<Vector3, List<Vector3>>();
            foreach (KeyValuePair<Vector3, List<Vector3>> pair in pointGraph)
            {
                if (pair.Value.Count == 2)
                {
                    hasLoop = true;
                    tempPair = pair;
                    break;
                }
                else
                {
                    Debug.Log("unexpected list count");
                    break;
                }
            }
            if (hasLoop)
            {
                Vector3 lastp = tempPair.Key;
                loop.Add(lastp);
                pointGraph.Remove(lastp);
                Vector3 p = tempPair.Value[0];
                List<Vector3> list;
                while (BlurTryGetValue(pointGraph, p, out list))
                {
                    if (list.Count == 2)
                    {
                        loop.Add(p);
                        BlurRemove(list, lastp);
                        lastp = p;
                        p = list[0];
                        BlurRemove(pointGraph, lastp);
                    }
                    else
                    {
                        Debug.Log("unexpected list count " + list.Count);
                        break;
                    }
                }
                loop.Add(p);
                res.Add(loop);
            }
        }
        return res;
    }

    private static void SplitTriangleMesh(Vector3 p1, Vector3 p2, Vector3 p3,
        ref List<Vector3> vertices1, ref List<int> triangles1,
        ref List<Vector3> vertices2, ref List<int> triangles2,
        ref Dictionary<Vector3, List<Vector3>> pointGraph, Vector3 normal, Vector3 planePoint)
    {
        Vector3 sec12 = IntersectionLineWithFace(p2 - p1, p1, normal, planePoint);
        Vector3 sec13 = IntersectionLineWithFace(p3 - p1, p1, normal, planePoint);
        List<Vector3> v;
        if (BlurTryGetValue(pointGraph, sec12, out v))
        {
            v.Add(sec13);
        }
        else
        {
            v = new List<Vector3> { sec13 };
            pointGraph.Add(sec12, v);
        }
        if (BlurTryGetValue(pointGraph, sec13, out v))
        {
            v.Add(sec12);
        }
        else
        {
            v = new List<Vector3> { sec12 };
            pointGraph.Add(sec13, v);
        }
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

    public static bool BlurContains(List<Vector3> list, Vector3 key)
    {
        foreach (Vector3 v in list)
        {
            if (BlurEqual(v, key))
            {
                return true;
            }
        }
        return false;
    }

    public static bool BlurEqual(Vector3 v1, Vector3 v2)
    {
        return Vector3.Distance(v1, v2) < MDefinitions.VECTOR3_PRECISION;
    }

    public static void BlurRemove(List<Vector3> list, Vector3 key)
    {
        float min = float.MaxValue;
        float temp;
        Vector3 k = new Vector3();
        foreach (Vector3 v in list)
        {
            if ((temp = Vector3.Distance(v, key)) < min)
            {
                min = temp;
                k = v;
            }
        }
        if (min < MDefinitions.VECTOR3_PRECISION)
        {
            list.Remove(k);
        }
    }

    public static void BlurRemove(List<KeyValuePair<Vector3, int>> list, Vector3 key)
    {
        float min = float.MaxValue;
        float temp;
        KeyValuePair<Vector3, int> k = new KeyValuePair<Vector3, int>();
        foreach (KeyValuePair<Vector3, int> p in list)
        {
            if ((temp = Vector3.Distance(p.Key, key)) < min)
            {
                min = temp;
                k = p;
            }
        }
        if (min < MDefinitions.VECTOR3_PRECISION)
        {
            list.Remove(k);
        }
    }

    public static void BlurRemove(Dictionary<Vector3, List<Vector3>> dictionary, Vector3 key)
    {
        float min = float.MaxValue;
        float temp;
        Vector3 k = new Vector3();
        foreach (Vector3 v in dictionary.Keys)
        {
            if ((temp = Vector3.Distance(v, key)) < min)
            {
                min = temp;
                k = v;
            }
        }
        if (min < MDefinitions.VECTOR3_PRECISION)
        {
            dictionary.Remove(k);
        }
    }

    public static void BlurRemove(Dictionary<Vector3, List<KeyValuePair<Vector3, int>>> dictionary, Vector3 key)
    {
        float min = float.MaxValue;
        float temp;
        Vector3 k = new Vector3();
        foreach (Vector3 v in dictionary.Keys)
        {
            if ((temp = Vector3.Distance(v, key)) < min)
            {
                min = temp;
                k = v;
            }
        }
        if (min < MDefinitions.VECTOR3_PRECISION)
        {
            dictionary.Remove(k);
        }
    }

    public static bool BlurTryGetValue(Dictionary<Vector3, List<Vector3>> dictionary, Vector3 key, out List<Vector3> value)
    {
        float min = float.MaxValue;
        float temp;
        List<Vector3> list = null;
        foreach (KeyValuePair<Vector3, List<Vector3>> pair in dictionary)
        {
            if((temp = Vector3.Distance(pair.Key, key)) < min)
            {
                min = temp;
                list = pair.Value;
            }
        }
        if(min < MDefinitions.VECTOR3_PRECISION)
        {
            value = list;
            return true;
        }
        value = null;
        return false;
    }

    public static bool BlurTryGetValue(Dictionary<Vector3, List<KeyValuePair<Vector3, int>>> dictionary, Vector3 key, out List<KeyValuePair<Vector3, int>> value)
    {
        float min = float.MaxValue;
        float temp;
        List<KeyValuePair<Vector3, int>> list = null;
        foreach (KeyValuePair<Vector3, List<KeyValuePair<Vector3, int>>> pair in dictionary)
        {
            if ((temp = Vector3.Distance(pair.Key, key)) < min)
            {
                min = temp;
                list = pair.Value;
            }
        }
        if (min < MDefinitions.VECTOR3_PRECISION)
        {
            value = list;
            return true;
        }
        value = null;
        return false;
    }

    public static void AddValToDictionary(Dictionary<Vector3, List<KeyValuePair<Vector3, int>>> dictionary, Vector3 key, Vector3 val, int index)
    {
        List<KeyValuePair<Vector3, int>> list;
        if (MHelperFunctions.BlurTryGetValue(dictionary, key, out list))
        {
            list.Add(new KeyValuePair<Vector3, int>(val, index));
        }
        else
        {
            dictionary.Add(key, new List<KeyValuePair<Vector3, int>> { new KeyValuePair<Vector3, int>(val, index) });
        }
    }
}
