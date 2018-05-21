using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MEntity
{
    public enum MEntityType { POINT, EDGE, FACE}

    public enum MEntityStatus { DEFAULT, ACTIVE, SELECT}

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

    abstract
    public float CalcDistance(Vector3 point);

    abstract
    public bool IsValid();
}
