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
        boundingBox = new AABB();
        // TODO: 计算柱面的包围盒
    }

    override
    public float CalcDistance(Vector3 point)
    {
        // TODO: 计算到柱面的距离
        return 0;
    }

    override
    public float GetSurface()
    {
        return 2 * Mathf.PI * top.radius * Vector3.Distance(top.center.position, bottom.center.position);
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
