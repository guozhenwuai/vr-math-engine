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
        InitMesh();
    }

    override
    public void UpdateMesh()
    {
        InitMesh();
        foreach(MFace face in faces)
        {
            face.UpdateMesh();
        }
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
        return MHelperFunctions.DistanceP2S(point, start.position, end.position);
    }

    override
    public Vector3 SpecialPointFind(Vector3 point)
    {
        Vector3 p = MHelperFunctions.PointProjectionInLine(point, direction, start.position);
        float ratio = Vector3.Distance(p, start.position) / Vector3.Distance(end.position, start.position);
        if(Mathf.Abs(ratio - 0.5f) <= MDefinitions.AUTO_REVISE_FACTOR) // 线段中点
        {
            return (end.position - start.position) * 0.5f + start.position;
        } else if(Mathf.Abs(ratio - 1.0f / 3) <= MDefinitions.AUTO_REVISE_FACTOR) // 1/3点
        {
            return (end.position - start.position) / 3 + start.position;
        } else if (Mathf.Abs(ratio - 2.0f / 3) <= MDefinitions.AUTO_REVISE_FACTOR) // 2/3点
        {
            return (end.position - start.position) * 2/ 3 + start.position;
        }
        else if (Mathf.Abs(ratio - 0.25f) <= MDefinitions.AUTO_REVISE_FACTOR) // 1/4点
        {
            return (end.position - start.position) * 0.25f + start.position;
        }
        else if (Mathf.Abs(ratio - 0.75f) <= MDefinitions.AUTO_REVISE_FACTOR) // 3/4点
        {
            return (end.position - start.position) * 0.75f + start.position;
        } else
        {
            return p;
        }
    }

    override
    public Vector3 GetProjection(Vector3 target, Vector3 assistant)
    {
        return MHelperFunctions.PointProjectionInLine(target, direction, start.position);
    }

    override
    public float GetLength()
    {
        return Vector3.Distance(start.position, end.position);
    }

    private void InitMesh()
    {
        mesh = MPrefab.GetLineMesh(start.position, end.position, MDefinitions.LINE_RADIUS);
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
