using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRelation
{
    public enum EntityRelationType { POINT_POINT, POINT_EDGE, POINT_FACE, EDGE_EDGE, EDGE_FACE, FACE_FACE };

    public EntityRelationType relationType { get; set; }

    // 下面两个成员存储进行关系判断的两个图元，lowerEntity为两者中级别较低的，如判断线面关系时lowerEntity应为线，higherEntity应为面
    public MEntity lowerEntity { get; set; }

    public MEntity higherEntity { get; set; }

    // 存储两个图元的距离，对于属于情况（点属于线&面、线属于面）或线线共面情况(平行除外)距离应为0， 对于不平行的面面关系或者相交的线面距离设为-1
    public float distance { get; set; }

    // 存储两个图元的角度，范围应该在[0, 90]，仅针对线线、线面、面面关系。角度为0说明平行，为90说明垂直，线面角度是90 - 线和面的法向量的角度
    public float angle { get; set; }

    public MRelation(EntityRelationType type, MEntity lower, MEntity higher, float dis, float angle)
    {
        relationType = type;
        lowerEntity = lower;
        higherEntity = higher;
        distance = dis;
        this.angle = angle;
    }
}