using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCircleFace : MFace
{
    public MCurveEdge circle
    {
        get;
        set;
    }

    public MCircleFace(MCurveEdge edge)
    {
        this.circle = edge;
        faceType = MFaceType.CIRCLE;
        entityType = MEntityType.FACE;
        entityStatus = MEntityStatus.DEFAULT;
        InitMesh();
    }

    override
    public float CalcDistance(Vector3 point)
    {
        Vector3 p = MHelperFunctions.PointProjectionInFace(point, circle.normal, circle.center.position);
        float r = Vector3.Distance(p, circle.center.position);
        float h = MHelperFunctions.DistanceP2F(point, circle.normal, circle.center.position);
        if (r <= circle.radius) return h;
        else
        {
            r = Mathf.Abs(r - circle.radius);
            return Mathf.Sqrt(r * r + h * h);
        }
    }

    override
    public Vector3 SpecialPointFind(Vector3 point)
    {
        return MHelperFunctions.PointProjectionInFace(point, circle.normal, circle.center.position);
    }

    override
    public Vector3 GetProjection(Vector3 target, Vector3 assistant)
    {
        return MHelperFunctions.PointProjectionInFace(target, circle.normal, circle.center.position);
    }

    override
    public Vector3 GetVerticalPoint(Vector3 startPoint, Vector3 curPoint)
    {
        return MHelperFunctions.PointProjectionInLine(curPoint, circle.normal, startPoint);
    }

    override
    public float GetSurface()
    {
        return Mathf.PI * Mathf.Pow(circle.radius, 2);
    }

    private void InitMesh()
    {
        mesh = MPrefab.GetCircleFaceMesh(circle.center.position, circle.normal, circle.radius);
        mesh.RecalculateBounds();
        boundingBox = new AABB(mesh.bounds);
    }

    override
    public bool IsValid()
    {
        if (!circle.IsValid()) return false;
        return true;
    }

    override
    public bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (this.GetType() != obj.GetType())
            return false;

        return Equals(obj as MCircleFace);
    }

    private bool Equals(MCircleFace obj)
    {
        return this.circle.Equals(obj.circle);
    }

    override
    public int GetHashCode()
    {
        return this.circle.GetHashCode();
    }
}
