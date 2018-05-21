using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLinearEdge : MEdge
{
    public MPoint start
    {
        get;
        set;
    }
    public MPoint end
    {
        get;
        set;
    }

    public Vector3 direction
    {
        get { return end.position - start.position; }
    }

    public MLinearEdge(MPoint start, MPoint end)
    {
        this.start = start;
        this.end = end;
        edgeType = MEdgeType.LINEAR;
        entityType = MEntityType.EDGE;
        entityStatus = MEntityStatus.DEFAULT;
    }

    public bool Parallel(MLinearEdge edge)
    {
        return MHelperFunctions.Parallel(direction, edge.direction);
    }

    public bool Perpendicular(MLinearEdge edge)
    {
        return MHelperFunctions.Perpendicular(direction, edge.direction);
    }

    override
    public float CalcDistance(Vector3 point)
    {
        float d1 = Vector3.Dot(end.position - start.position, point - start.position);
        if (MHelperFunctions.FloatZero(d1) <= 0)
        {
            return Vector3.Distance(point, start.position);
        }
        float d2 = Vector3.Dot(end.position - start.position, end.position - start.position);
        if(d1 >= d2)
        {
            return Vector3.Distance(point, end.position);
        }
        float r = d1 / d2;
        return Vector3.Distance(point,r * end.position + (1 - r) * start.position);
    }

    override
    public float GetLength()
    {
        return Vector3.Distance(start.position, end.position);
    }

    override
    public bool IsValid()
    {
        if (start.Equals(end)) return false;
        return true;
    }

    override
    public bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (this.GetType() != obj.GetType())
            return false;

        return Equals(obj as MLinearEdge);
    }

    private bool Equals(MLinearEdge obj)
    {
        return (this.start.Equals(obj.start) && this.end.Equals(obj.end)) || (this.start.Equals(obj.end) && this.end.Equals(obj.start));
    }

    override
    public int GetHashCode()
    {
        return this.start.GetHashCode() ^ this.end.GetHashCode();
    }
}
