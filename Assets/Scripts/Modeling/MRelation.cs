using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRelation
{
    public enum EntityRelationType { POINT_POINT, POINT_EDGE, POINT_FACE, EDGE_EDGE, EDGE_FACE, FACE_FACE };

    public EntityRelationType relationType { get; set; }

    public MEntityPair lowerEntity { get; set; }

    public MEntityPair higherEntity { get; set; }

    public float distance { get; set; }

    public float angle { get; set; }

    public bool shareObject { get; set; }

    public MRelation(EntityRelationType type, MEntityPair lower, MEntityPair higher, float dis, float angle, bool shareObject)
    {
        relationType = type;
        lowerEntity = lower;
        higherEntity = higher;
        distance = dis;
        this.angle = angle;
        this.shareObject = shareObject;
    }
}