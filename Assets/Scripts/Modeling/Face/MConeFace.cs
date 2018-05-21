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

    public MConeFace(MPoint top, MCurveEdge bottom)
    {
        this.top = top;
        this.bottom = bottom;
        faceType = MFaceType.CONE;
        entityType = MEntityType.FACE;
        entityStatus = MEntityStatus.DEFAULT;
        boundingBox = new AABB();
        // TODO: 计算锥面的包围盒
    }

    override
    public float CalcDistance(Vector3 point)
    {
        // TODO: 计算到锥面的距离
        return 0;
    }

    override
    public float GetSurface()
    {
        // TODO: 计算锥面的表面积
        return 0;
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
