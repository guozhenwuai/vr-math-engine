using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MEdge : MEntity
{
    public enum MEdgeType {LINEAR, CURVE}

    public MEdgeType edgeType
    {
        get;
        protected set;
    }

    public List<MFace> faces = new List<MFace>();

    override
    public float CalcDistance(Vector3 point)
    {
        Debug.Log("MEdge: Virtual CalcDistance");
        return 0;
    }

    override
    public Vector3 SpecialPointFind(Vector3 point)
    {
        Debug.Log("MEdge: Virtual SpecialPointFind");
        return point;
    }

    override
    public bool IsValid()
    {
        Debug.Log("MEdge: Virtual IsValid");
        return false;
    }

    override
    public void Render(Matrix4x4 matrix)
    {
        Material mat = null;
        switch (entityStatus)
        {
            case MEntityStatus.DEFAULT:
                mat = MMaterial.GetDefaultEdgeMat();
                break;
            case MEntityStatus.ACTIVE:
                mat = MMaterial.GetActiveEdgeMat();
                break;
            case MEntityStatus.SELECT:
                mat = MMaterial.GetSelectEdgeMat();
                break;
            case MEntityStatus.SPECIAL:
                mat = MMaterial.GetSpecialEdgeMat();
                break;
            case MEntityStatus.TRANSPARENT:
                mat = null;
                break;
            default:
                Debug.Log("MEdge: unkown entity status: " + entityStatus);
                break;
        }
        if (mat != null && mesh != null) Graphics.DrawMesh(mesh, matrix, mat, 0);
    }

    virtual
    public float GetLength()
    {
        Debug.Log("MEdge: Virtual GetLength");
        return 0;
    }

}
