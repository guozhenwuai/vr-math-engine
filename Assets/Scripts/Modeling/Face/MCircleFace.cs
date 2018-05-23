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
        boundingBox = new AABB();
        // TODO: 计算圆面的包围盒
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
    public float GetSurface()
    {
        return Mathf.PI * Mathf.Pow(circle.radius, 2);
    }

    private void InitMesh()
    {
        // TODO: 圆形Mesh的绘制
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
