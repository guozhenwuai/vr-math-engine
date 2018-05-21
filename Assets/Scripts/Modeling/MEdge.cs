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
    public bool IsValid()
    {
        Debug.Log("MEdge: Virtual IsValid");
        return false;
    }

    virtual
    public float GetLength()
    {
        Debug.Log("MEdge: Virtual GetLength");
        return 0;
    }

}
