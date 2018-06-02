using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MCurveEdge : MEdge
{
    public MPoint center
    {
        get;
        set;
    }
    public Vector3 normal
    {
        get;
        private set;
    }
    public float radius
    {
        get;
        private set;
    }

    public MCurveEdge(MPoint center, Vector3 normal, float radius)
    {
        this.center = center;
        this.normal = Vector3.Normalize(normal);
        this.radius = radius;
        edgeType = MEdgeType.CURVE;
        entityType = MEntityType.EDGE;
        entityStatus = MEntityStatus.DEFAULT;
        InitMesh();
    }

    override
    public float CalcDistance(Vector3 point)
    {
        float h = MHelperFunctions.DistanceP2F(point, normal, center.position);
        Vector3 p = MHelperFunctions.PointProjectionInFace(point, normal, center.position);
        float r = Vector3.Distance(p, center.position);
        r = Mathf.Abs(r - radius);
        return Mathf.Sqrt(r * r + h * h);
    }

    override
    public Vector3 SpecialPointFind(Vector3 point)
    {
        Vector3 p = MHelperFunctions.PointProjectionInFace(point, normal, center.position);
        return center.position + (p - center.position).normalized * radius;
    }

    override
    public float GetLength()
    {
        return 2 * Mathf.PI * radius;
    }

    private void InitMesh()
    {
        // TODO: 曲线Mesh的绘制
    }

    override
    public bool IsValid()
    {
        if (radius <= MDefinitions.POINT_PRECISION) return false;
        return true;
    }

    override
    public bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (this.GetType() != obj.GetType())
            return false;

        return Equals(obj as MCurveEdge);
    }

    private bool Equals(MCurveEdge obj)
    {
        return this.center.Equals(obj.center) && MHelperFunctions.FloatEqual(this.radius, obj.radius) && MHelperFunctions.Parallel(this.normal, obj.normal);
    }

    override
    public int GetHashCode()
    {
        return this.center.GetHashCode() ^ this.radius.GetHashCode() ^ this.normal.GetHashCode();
    }
}
