using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPoint : MEntity
{
    public Vector3 position
    {
        get;
        private set;
    }

    public List<MEdge> edges = new List<MEdge>();

    public List<MFace> faces = new List<MFace>();
    
    public MPoint(Vector3 position)
    {
        this.position = position;
        entityType = MEntityType.POINT;
        entityStatus = MEntityStatus.DEFAULT;
    }

    override
    public float CalcDistance(Vector3 point)
    {
        return Vector3.Distance(position, point);
    }

    override
    public bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (this.GetType() != obj.GetType())
            return false;

        return Equals(obj as MPoint) ;
    }

    private bool Equals(MPoint obj)
    {
        return Vector3.Distance(position, obj.position) <= MDefinitions.POINT_PRECISION;
    }

    override
    public int GetHashCode()
    {
        return this.position.GetHashCode();
    }

    override
    public bool IsValid()
    {
        return true;
    }
}
