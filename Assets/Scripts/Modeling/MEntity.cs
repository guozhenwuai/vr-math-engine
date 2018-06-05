using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MEntity
{
    public enum MEntityType { POINT, EDGE, FACE}

    public enum MEntityStatus { DEFAULT, ACTIVE, SELECT, SPECIAL, TRANSPARENT}

    public MEntityType entityType
    {
        get;
        protected set;
    }

    public MEntityStatus entityStatus
    {
        get;
        set;
    }

    public Mesh mesh
    {
        get;
        protected set;
    }

    abstract
    public float CalcDistance(Vector3 point);

    abstract
    public bool IsValid();

    abstract
    public void Render(Matrix4x4 matrix);

    abstract
    public void UpdateMesh();

    abstract
    public Vector3 SpecialPointFind(Vector3 point);

    abstract
    public Vector3 GetProjection(Vector3 target, Vector3 assistant);
    
}
