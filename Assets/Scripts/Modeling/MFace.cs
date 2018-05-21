using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MFace: MEntity
{
    public enum MFaceType { POLYGON, CIRCLE, CYLINDER, SPHERE, CONE}

    public MFaceType faceType
    {
        get;
        protected set;
    }

    public AABB boundingBox
    {
        get;
        protected set;
    }

    override
    public float CalcDistance(Vector3 point)
    {
        Debug.Log("MFace: Virtual CalcDistance");
        return 0;
    }
    virtual
    public float GetSurface()
    {
        Debug.Log("MFace: Virtual GetSurface");
        return 0;
    }

    override
    public bool IsValid()
    {
        Debug.Log("MFace: Virtual IsValid");
        return false;
    }
}
