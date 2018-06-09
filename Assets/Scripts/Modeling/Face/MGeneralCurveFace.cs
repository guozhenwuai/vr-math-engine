using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGeneralCurveFace : MFace
{
    float surface;

    AABB[] boundingboxes;

    public MGeneralCurveFace(Mesh mesh)
    {
        faceType = MFaceType.GENERAL_CURVE;
        entityType = MEntityType.FACE;
        entityStatus = MEntityStatus.DEFAULT;
        this.mesh = mesh;
        mesh.RecalculateBounds();
        boundingBox = new AABB(mesh.bounds);
        CalcSurface();
        InitBoundingBoxes();
    }

    override
    public float CalcDistance(Vector3 point)
    {
        float min = float.MaxValue;
        int count = mesh.triangles.Length / 3;
        for(int i = 0; i < count; i++)
        {
            if (!boundingboxes[i].Contains(point, MDefinitions.POINT_PRECISION)) continue;
            Vector3 p1 = mesh.vertices[mesh.triangles[3 * i]];
            Vector3 p2 = mesh.vertices[mesh.triangles[3 * i + 1]];
            Vector3 p3 = mesh.vertices[mesh.triangles[3 * i + 2]];
            min = Mathf.Min(min, MHelperFunctions.DistanceP2T(point, p1, p2, p3));
        }
        return min;
    }

    override
    public Vector3 SpecialPointFind(Vector3 point)
    {
        int index = 0;
        float min = float.MaxValue;
        float temp;
        Vector3 p1, p2, p3;
        int count = mesh.triangles.Length / 3;
        for (int i = 0; i < count; i++)
        {
            if (!boundingboxes[i].Contains(point, MDefinitions.POINT_PRECISION)) continue;
            p1 = mesh.vertices[mesh.triangles[3 * i]];
            p2 = mesh.vertices[mesh.triangles[3 * i + 1]];
            p3 = mesh.vertices[mesh.triangles[3 * i + 2]];
            if((temp = MHelperFunctions.DistanceP2T(point, p1, p2, p3)) < min)
            {
                min = temp;
                index = i;
            }
        }
        p1 = mesh.vertices[mesh.triangles[3 * index]];
        p2 = mesh.vertices[mesh.triangles[3 * index + 1]];
        p3 = mesh.vertices[mesh.triangles[3 * index + 2]];
        Vector3 normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;
        return MHelperFunctions.PointProjectionInFace(point, normal, p1);
    }

    override
    public float GetSurface()
    {
        return surface;
    }

    override
    public bool IsValid()
    {
        return mesh != null && mesh.triangles.Length != 0;
    }

    override
    public bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (this.GetType() != obj.GetType())
            return false;

        return Equals(obj as MGeneralCurveFace);
    }

    private bool Equals(MGeneralCurveFace obj)
    {
        return mesh.Equals(obj.mesh);
    }

    override
    public int GetHashCode()
    {
        return mesh.GetHashCode();
    }

    private void CalcSurface()
    {
        surface = 0;
        int count = mesh.triangles.Length / 3;
        for (int i = 0; i < count; i++)
        {
            Vector3 p1 = mesh.vertices[mesh.triangles[3 * i]];
            Vector3 p2 = mesh.vertices[mesh.triangles[3 * i + 1]];
            Vector3 p3 = mesh.vertices[mesh.triangles[3 * i + 2]];
            surface += MHelperFunctions.TriangleSurface(p1, p2, p3);
        }
    }

    private void InitBoundingBoxes()
    {
        Vector3 p1, p2, p3;
        int count = mesh.triangles.Length / 3;
        boundingboxes = new AABB[count];
        for (int i = 0; i < count; i++)
        {
            p1 = mesh.vertices[mesh.triangles[3 * i]];
            p2 = mesh.vertices[mesh.triangles[3 * i + 1]];
            p3 = mesh.vertices[mesh.triangles[3 * i + 2]];
            boundingboxes[i] = new AABB(MHelperFunctions.Min(p1, p2, p3), MHelperFunctions.Max(p1, p2, p3));
        }
    }
}