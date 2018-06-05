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
        InitMesh();
    }

    public void SetPosition(Vector3 position)
    {
        this.position = position;
        UpdateMesh();
        foreach(MEdge edge in edges)
        {
            edge.UpdateMesh();
        }
        foreach(MFace face in faces)
        {
            face.UpdateMesh();
        }
    }

    override
    public void UpdateMesh()
    {
        InitMesh();
    }

    override
    public float CalcDistance(Vector3 point)
    {
        return Vector3.Distance(position, point);
    }

    override
    public Vector3 SpecialPointFind(Vector3 point)
    {
        Vector3 v = point - position;
        if(v.magnitude <= MDefinitions.POINT_PRECISION)
        {
            return position + v.normalized * MDefinitions.POINT_PRECISION;
        }
        else
        {
            return point;
        }
    }

    override
    public Vector3 GetProjection(Vector3 target, Vector3 assistant)
    {
        return position;
    }

    override
    public void Render(Matrix4x4 matrix)
    {
        Material mat = null;
        switch (entityStatus)
        {
            case MEntityStatus.DEFAULT:
                mat = MMaterial.GetDefaultPointMat();
                break;
            case MEntityStatus.ACTIVE:
                mat = MMaterial.GetActivePointMat();
                break;
            case MEntityStatus.SELECT:
                mat = MMaterial.GetSelectPointMat();
                break;
            case MEntityStatus.SPECIAL:
                mat = MMaterial.GetSpecialPointMat();
                break;
            case MEntityStatus.TRANSPARENT:
                mat = null;
                break;
            default:
                Debug.Log("MPoint: unkown entity status: " + entityStatus);
                break;
        }
        if (mat != null && mesh != null) Graphics.DrawMesh(mesh, matrix, mat, 0);
    }

    private void InitMesh()
    {
        float radius = MDefinitions.POINT_PRECISION;
        mesh = MPrefab.GetCubeMesh();
        Vector3[] vertices = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            vertices[i] = mesh.vertices[i] * radius * 2 + position;
        }
        mesh.vertices = vertices;
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
