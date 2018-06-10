using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGeneralFlatFace : MFace
{
    public List<Vector3> points
    {
        get;
        set;
    }

    public Vector3 normal { get; private set; }

    bool buildSuccess = true;

    float surface;

    public MGeneralFlatFace(List<Vector3> points)
    {
        faceType = MFaceType.GENERAL_FLAT;
        entityType = MEntityType.FACE;
        entityStatus = MEntityStatus.DEFAULT;
        if (points.Count < 3)
        {
            buildSuccess = false;
            return;
        }
        this.points = points;
        if (buildSuccess) CalcNormal();
        if (buildSuccess) InitMesh();
        if (buildSuccess) CalcSurface();
    }

    override
    public float CalcDistance(Vector3 point)
    {
        Vector3 projectionPoint = MHelperFunctions.PointProjectionInFace(point, normal, points[0]);
        Vector3 rotateAxis;
        float rotateAngle;
        MHelperFunctions.CalcRotateAxisAndAngle(out rotateAxis, out rotateAngle, normal, new Vector3(0, 0, 1));
        List<Vector3> rotatePoints = new List<Vector3>();
        foreach (Vector3 p in points)
        {
            rotatePoints.Add(MHelperFunctions.CalcRotate(p, rotateAxis, rotateAngle));
        }
        projectionPoint = MHelperFunctions.CalcRotate(projectionPoint, rotateAxis, rotateAngle);
        if (MHelperFunctions.InPolygon(projectionPoint, rotatePoints))
        {
            return MHelperFunctions.DistanceP2F(point, normal, points[0]);
        }
        else
        {
            float min = float.MaxValue;
            int count = points.Count;
            for(int i = 0; i < count; i++)
            {
                min = Mathf.Min(min, MHelperFunctions.DistanceP2S(point, points[i], points[(i + 1) % count]));
            }
            return min;
        }
    }

    override
    public Vector3 SpecialPointFind(Vector3 point)
    {
        return MHelperFunctions.PointProjectionInFace(point, normal, points[0]);
    }

    override
    public Vector3 GetProjection(Vector3 target, Vector3 assistant)
    {
        return MHelperFunctions.PointProjectionInFace(target, normal, points[0]);
    }

    override
    public Vector3 GetVerticalPoint(Vector3 startPoint, Vector3 curPoint)
    {
        return MHelperFunctions.PointProjectionInLine(curPoint, normal, startPoint);
    }

    override
    public float GetSurface()
    {
        return surface;
    }

    private void InitMesh()
    {
        int count = points.Count;
        Vector3[] vertices = new Vector3[count];
        Vector3[] normals = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            vertices[i] = points[i];
            normals[i] = normal;
        }
        int[] triangles = new int[3 * (count - 2)];

        for (int i = 0; i < count - 2; i++)
        {
            triangles[3 * i] = 0;
            triangles[3 * i + 1] = i + 1;
            triangles[3 * i + 2] = i + 2;
        }
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.RecalculateBounds();
        boundingBox = new AABB(mesh.bounds);
    }

    override
    public bool IsValid()
    {
        return buildSuccess;
    }

    override
    public bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (this.GetType() != obj.GetType())
            return false;

        return Equals(obj as MGeneralFlatFace);
    }

    private bool Equals(MGeneralFlatFace obj)
    {
        return IdenticalLoop(points, obj.points);
    }

    override
    public int GetHashCode()
    {
        return this.points.GetHashCode();
    }

    private void CalcNormal()
    {
        Vector3 v1 = points[1] - points[0];
        Vector3 v2 = v1;
        int count = points.Count;
        int i;
        for (i = 1; i < count; i++)
        {
            v2 = points[(i + 1) % count] - points[i];
            if (!MHelperFunctions.Parallel(v1, v2)) break;
        }
        if (i != count)
        {
            normal = Vector3.Normalize(Vector3.Cross(v1, v2));
        }
        else
        {
            Debug.Log("MPolygonFace: CalcNormal: wrong edgeList");
            buildSuccess = false;
        }
        if (buildSuccess)
        {
            Vector3 p = points[0];
            for(i = 1; i < count; i++)
            {
                Vector3 v = MHelperFunctions.PointProjectionInFace(points[i], normal, points[0]);
                if(Vector3.Distance(points[i], v) < MDefinitions.VECTOR3_PRECISION)
                {
                    points[i] = v;
                }
                else
                {
                    buildSuccess = false;
                    return;
                }
            }
        }
    }

    private void CalcSurface()
    {
        surface = 0;
        int count = points.Count;
        Vector3 v = points[0];
        for (int i = 1; i < count - 1; i++)
        {
            surface += MHelperFunctions.TriangleSurface(v, points[i], points[i + 1]);
        }
    }

    private bool IdenticalLoop(List<Vector3> p1, List<Vector3> p2)
    {
        int count;
        if ((count = p1.Count) != p2.Count || count < 3) return false;
        int j = p2.IndexOf(p1[0]);
        if (j == -1) return false;
        int sig = 1;
        if (p1[1].Equals(p2[(j + 1) % count])) sig = 1;
        else if (p1[1].Equals(p2[(j + count - 1) % count])) sig = count - 1;
        else return false;
        for (int i = 0; i < count; i++, j = (j + sig) % count)
        {
            if (!p1[i].Equals(p2[j])) return false;
        }
        return true;
    }
}
