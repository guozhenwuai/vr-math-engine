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

    override
    public float CalcDistance(Vector3 point)
    {
        return Vector3.Distance(position, point);
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
        Vector3[] vertices = new Vector3[8];
        vertices[0] = position + new Vector3(-radius, -radius, -radius);
        vertices[1] = position + new Vector3(-radius, -radius, radius);
        vertices[2] = position + new Vector3(radius, -radius, radius);
        vertices[3] = position + new Vector3(radius, -radius, -radius);
        vertices[4] = position + new Vector3(-radius, radius, -radius);
        vertices[5] = position + new Vector3(-radius, radius, radius);
        vertices[6] = position + new Vector3(radius, radius, radius);
        vertices[7] = position + new Vector3(radius, radius, -radius);
        int[] triangles = new int[36]
        {
            0,1,2, 0,2,3,
            5,4,0, 1,5,0,
            7,3,0, 4,7,0,
            6,2,3, 7,6,3,
            6,5,1, 2,6,1,
            5,6,7, 5,7,4
        };
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
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
