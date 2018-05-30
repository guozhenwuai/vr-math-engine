using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MEntityPair
{
    public MEntity entity { get; set; }

    public MObject obj { get; set; }

    public MEntityPair(MEntity entity, MObject obj)
    {
        this.entity = entity;
        this.obj = obj;
    }
}