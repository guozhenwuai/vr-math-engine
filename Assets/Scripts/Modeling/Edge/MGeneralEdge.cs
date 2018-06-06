using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGeneralEdge : MEdge
{
    public List<Vector3> points { get; private set; }

    private float length;

    public MGeneralEdge(List<Vector3> points)
    {
        this.points = points;
        edgeType = MEdgeType.GENERAL;
        entityType = MEntityType.EDGE;
        entityStatus = MEntityStatus.DEFAULT;
        if (IsValid())
        {
            InitLength();
            InitMesh();
        }
    }

    override
    public float CalcDistance(Vector3 point)
    {
        float min = float.MaxValue;
        int count = points.Count - 1;
        for (int i = 0; i < count; i++)
        {
            min = Mathf.Min(min, MHelperFunctions.DistanceP2S(point, points[i], points[i + 1]));
        }
        return min;
    }

    override
    public Vector3 SpecialPointFind(Vector3 point)
    {
        float min = float.MaxValue;
        int index = 0;
        float temp;
        int count = points.Count - 1;
        for (int i = 0; i < count; i++)
        {
            if((temp = MHelperFunctions.DistanceP2S(point, points[i], points[i + 1])) < min)
            {
                min = temp;
                index = i;
            }
        }
        return MHelperFunctions.PointProjectionInLine(point, points[index + 1] - points[index], points[index]);
    }
    
    override
    public float GetLength()
    {
        return length;
    }

    private void InitLength()
    {
        length = 0;
        int count = points.Count - 1;
        for(int i = 0; i < count; i++)
        {
            length += Vector3.Distance(points[i], points[i + 1]);
        }
    }

    private void InitMesh()
    {
        int count = points.Count - 1;
        CombineInstance[] combines = new CombineInstance[count];
        Matrix4x4 transform = Matrix4x4.Scale(Vector3.one);
        Vector3 lastNormal = (points[1] - points[0]).normalized;
        for (int i = 0; i < count; i++)
        {
            Vector3 curNormal = (points[i + 1] - points[i]).normalized;
            combines[i].mesh = MPrefab.GetLineMesh(points[i],lastNormal, points[i + 1],curNormal, MDefinitions.LINE_RADIUS);
            combines[i].transform = transform;
            lastNormal = curNormal;
        }
        mesh = new Mesh();
        mesh.CombineMeshes(combines);
    }

    override
    public bool IsValid()
    {
        if (points.Count < 2) return false;
        return true;
    }
}
