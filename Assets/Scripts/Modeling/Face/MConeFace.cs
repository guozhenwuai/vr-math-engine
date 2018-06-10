using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MConeFace : MFace
{
    public MPoint top
    {
        get;
        set;
    }
    public MCurveEdge bottom
    {
        get;
        set;
    }

    private float generatrix;

    public MConeFace(MPoint top, MCurveEdge bottom)
    {
        this.top = top;
        this.bottom = bottom;
        faceType = MFaceType.CONE;
        entityType = MEntityType.FACE;
        entityStatus = MEntityStatus.DEFAULT;
        if (IsValid())
        {
            InitMesh();
            float h = Vector3.Distance(top.position, bottom.center.position);
            generatrix = Mathf.Sqrt(bottom.radius * bottom.radius + h * h);
        }
    }

    override
    public float CalcDistance(Vector3 point)
    {
        float d1 = Vector3.Dot(bottom.center.position - top.position, point - top.position);
        if (MHelperFunctions.FloatZero(d1) <= 0)
        {
            return Vector3.Distance(point, top.position);
        }
        float d2 = Vector3.Dot(bottom.center.position - top.position, bottom.center.position - top.position);
        if (d1 >= d2)
        {
            return bottom.CalcDistance(point);
        }
        Vector3 v = MHelperFunctions.PointProjectionInLine(point, bottom.center.position - top.position, top.position);
        float dis = Vector3.Distance(point, v);
        if (dis < MDefinitions.FLOAT_PRECISION)
        {
            return Vector3.Distance(v, top.position) * bottom.radius / generatrix;
        }
        float h = Vector3.Distance(top.position, bottom.center.position);
        return Mathf.Abs(dis - Vector3.Distance(v, top.position) * bottom.radius / h) * h / generatrix;
    }

    override
    public float GetSurface()
    {
        
        return Mathf.PI * bottom.radius * generatrix;
    }

    override
    public Vector3 SpecialPointFind(Vector3 point)
    {
        Vector3 v = MHelperFunctions.PointProjectionInLine(point, bottom.center.position - top.position, top.position);
        float h = Vector3.Distance(top.position, bottom.center.position);
        return (point - v).normalized * Vector3.Distance(top.position, v) / h * bottom.radius + v;
    }

    override
    public Vector3 GetProjection(Vector3 target, Vector3 assistant)
    {
        Vector3 v = MHelperFunctions.PointProjectionInLine(target, bottom.center.position - top.position, top.position);
        Vector3 p;
        if (Vector3.Distance(target, v) < MDefinitions.FLOAT_PRECISION)
        {
            p = MHelperFunctions.PointProjectionInFace(assistant, bottom.normal, bottom.center.position);
        }
        else
        {
            p = MHelperFunctions.PointProjectionInFace(target, bottom.normal, bottom.center.position);
        }
        p = (p - bottom.center.position).normalized * bottom.radius + bottom.center.position;
        return MHelperFunctions.PointProjectionInLine(target, p - top.position, top.position);
    }

    private void InitMesh()
    {
        mesh = MPrefab.GetConeFaceMesh(bottom.center.position, top.position, bottom.normal, bottom.radius);
        mesh.RecalculateBounds();
        boundingBox = new AABB(mesh.bounds);
    }

    override
    public bool IsValid()
    {
        return bottom.IsValid() && MHelperFunctions.Parallel(top.position - bottom.center.position, bottom.normal) && (!top.Equals(bottom.center));
    }

    override
    public bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (this.GetType() != obj.GetType())
            return false;

        return Equals(obj as MConeFace);
    }

    private bool Equals(MConeFace obj)
    {
        return top.Equals(obj.top) && bottom.Equals(obj.bottom);
    }

    override
    public int GetHashCode()
    {
        return top.GetHashCode() ^ bottom.GetHashCode();
    }
}
