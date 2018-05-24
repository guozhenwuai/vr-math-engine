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

    override
    public void Render(Matrix4x4 matrix)
    {
        Material mat = null;
        switch (entityStatus)
        {
            case MEntityStatus.DEFAULT:
                mat = MMaterial.GetDefaultFaceMat();
                break;
            case MEntityStatus.ACTIVE:
                mat = MMaterial.GetActiveFaceMat();
                break;
            case MEntityStatus.SELECT:
                mat = MMaterial.GetSelectFaceMat();
                break;
            case MEntityStatus.SPECIAL:
                mat = MMaterial.GetSpecialFaceMat();
                break;
            case MEntityStatus.TRANSPARENT:
                mat = null;
                break;
            default:
                Debug.Log("MFace: unkown entity status: " + entityStatus);
                break;
        }
        if (mat != null && mesh != null) Graphics.DrawMesh(mesh, matrix, mat, 0);
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
