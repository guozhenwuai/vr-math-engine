using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MSphereFace : MFace
{
    public MPoint center
    {
        get;
        set;
    }
    public float radius
    {
        get;
        private set;
    }
    
    public MSphereFace(MPoint center, float radius)
    {
        this.center = center;
        this.radius = radius;
        faceType = MFaceType.SPHERE;
        entityType = MEntityType.FACE;
        entityStatus = MEntityStatus.DEFAULT;
        Vector3 v = new Vector3(radius, radius, radius);
        boundingBox = new AABB(center.position - v, center.position + v);
    }

    override
    public float CalcDistance(Vector3 point)
    {
        float dis = Vector3.Distance(point, center.position);
        return Mathf.Abs(dis = radius);
    }

    override
    public float GetSurface()
    {
        return 4 * Mathf.PI * Mathf.Pow(radius, 2);
    }

    override
    public bool IsValid()
    {
        return radius > MDefinitions.POINT_PRECISION;
    }

    override
    public bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (this.GetType() != obj.GetType())
            return false;

        return Equals(obj as MSphereFace);
    }

    private bool Equals(MSphereFace obj)
    {
        return MHelperFunctions.FloatEqual(radius, obj.radius) && center.Equals(obj.center);
    }

    override
    public int GetHashCode()
    {
        return center.GetHashCode() ^ radius.GetHashCode();
    }
}
