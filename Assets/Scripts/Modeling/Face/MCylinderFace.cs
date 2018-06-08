using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCylinderFace : MFace
{
    public MCurveEdge top
    {
        get;
        set;
    }

    public MCurveEdge bottom
    {
        get;
        set;
    }

    public MCylinderFace(MCurveEdge top, MCurveEdge bottom)
    {
        this.top = top;
        this.bottom = bottom;
        faceType = MFaceType.CYLINDER;
        entityType = MEntityType.FACE;
        entityStatus = MEntityStatus.DEFAULT;
        if(IsValid())InitMesh();
    }

    override
    public float CalcDistance(Vector3 point)
    {
        float d1 = Vector3.Dot(bottom.center.position - top.center.position, point - top.center.position);
        if(MHelperFunctions.FloatZero(d1) <= 0)
        {
            return top.CalcDistance(point);
        }
        float d2 = Vector3.Dot(bottom.center.position - top.center.position, bottom.center.position - top.center.position);
        if(d1 >= d2)
        {
            return bottom.CalcDistance(point);
        }
        float r = MHelperFunctions.DistanceP2L(point, bottom.center.position - top.center.position, top.center.position);
        return Mathf.Abs(r - top.radius);
    }

    override
    public Vector3 SpecialPointFind(Vector3 point)
    {
        Vector3 v = MHelperFunctions.PointProjectionInLine(point, bottom.center.position - top.center.position, top.center.position);
        return (point - v).normalized * top.radius + v;
    }

    override
    public Vector3 GetProjection(Vector3 target, Vector3 assistant)
    {
        Vector3 v = MHelperFunctions.PointProjectionInLine(target, bottom.center.position - top.center.position, top.center.position);
        if(Vector3.Distance(target, v) < MDefinitions.FLOAT_PRECISION)
        {
            Vector3 p = MHelperFunctions.PointProjectionInFace(assistant, top.normal, v);
            return (p - v).normalized * top.radius + v;
        }
        return (target - v).normalized * top.radius + v;
    }

    override
    public float GetSurface()
    {
        return 2 * Mathf.PI * top.radius * Vector3.Distance(top.center.position, bottom.center.position);
    }

    private void InitMesh()
    {
        mesh = MPrefab.GetCylinderFaceMesh(top.center.position, bottom.center.position, top.normal, top.radius);
        mesh.RecalculateBounds();
        boundingBox = new AABB(mesh.bounds);
    }

    override
    public bool IsValid()
    {
        return top.IsValid() && bottom.IsValid() 
            && !top.center.Equals(bottom.center) 
            && MHelperFunctions.FloatEqual(top.radius, bottom.radius)
            && MHelperFunctions.Parallel(top.normal, bottom.normal)
            && MHelperFunctions.Parallel(top.center.position - bottom.center.position, top.normal);
    }

    override
    public bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (this.GetType() != obj.GetType())
            return false;

        return Equals(obj as MCylinderFace);
    }

    private bool Equals(MCylinderFace obj)
    {
        return ((top.Equals(obj.top) && bottom.Equals(obj.bottom)) || (top.Equals(obj.bottom) && bottom.Equals(obj.top)));
    }

    override
    public int GetHashCode()
    {
        return top.GetHashCode() ^ bottom.GetHashCode();
    }

}
