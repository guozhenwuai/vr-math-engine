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

    public bool SetEndPoint(Vector3 end) //谨慎使用！！！目前用于创建活动线段, endpoint不会渲染，所以不更新endpoint的mesh
    {
        this.end.position = end;
        if (!IsValid())
        {
            return false;
        }
        InitMesh();
        return true;
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
    public float GetLength()
    {
        return Vector3.Distance(start.position, end.position);
    }

    private void InitMesh()
    {
        Vector3 startPosition = start.position;
        Vector3 endPosition = end.position;
        float radius = MDefinitions.LINE_RADIUS;
        bool trade = false;
        float Xgap = Mathf.Abs(startPosition.x - endPosition.x);
        float Ygap = Mathf.Abs(startPosition.y - endPosition.y);
        float Zgap = Mathf.Abs(startPosition.z - endPosition.z);

        Vector3[] _Vertices = new Vector3[24];
        //X轴向线条
        if (Xgap >= Ygap && Xgap >= Zgap)
        {
            Vector3 middle;
            if (startPosition.x < endPosition.x)
            {
                middle = startPosition;
                startPosition = endPosition;
                endPosition = middle;
                trade = true;
            }

            _Vertices = new Vector3[24]
            {
                new Vector3(startPosition.x, startPosition.y, startPosition.z + radius),
                new Vector3(startPosition.x, startPosition.y + Mathf.Sin(3.14f/180*30) * radius, startPosition.z + Mathf.Cos(3.14f/180*30) * radius),
                new Vector3(startPosition.x, startPosition.y + Mathf.Sin(3.14f/180*60) * radius, startPosition.z + Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(startPosition.x, startPosition.y + radius, startPosition.z),
                new Vector3(startPosition.x, startPosition.y + Mathf.Sin(3.14f/180*60) * radius, startPosition.z - Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(startPosition.x, startPosition.y + Mathf.Sin(3.14f/180*30) * radius, startPosition.z - Mathf.Cos(3.14f/180*30) * radius),
                new Vector3(startPosition.x, startPosition.y, startPosition.z - radius),
                new Vector3(startPosition.x, startPosition.y - Mathf.Sin(3.14f/180*30) * radius, startPosition.z - Mathf.Cos(3.14f/180*30) * radius),
                new Vector3(startPosition.x, startPosition.y - Mathf.Sin(3.14f/180*60) * radius, startPosition.z - Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(startPosition.x, startPosition.y - radius, startPosition.z),
                new Vector3(startPosition.x, startPosition.y - Mathf.Sin(3.14f/180*60) * radius, startPosition.z + Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(startPosition.x, startPosition.y - Mathf.Sin(3.14f/180*30) * radius, startPosition.z + Mathf.Cos(3.14f/180*30) * radius),

                new Vector3(endPosition.x, endPosition.y, endPosition.z + radius),
                new Vector3(endPosition.x, endPosition.y + Mathf.Sin(3.14f/180*30) * radius, endPosition.z + Mathf.Cos(3.14f/180*30) * radius),
                new Vector3(endPosition.x, endPosition.y + Mathf.Sin(3.14f/180*60) * radius, endPosition.z + Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(endPosition.x, endPosition.y + radius, endPosition.z),
                new Vector3(endPosition.x, endPosition.y + Mathf.Sin(3.14f/180*60) * radius, endPosition.z - Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(endPosition.x, endPosition.y + Mathf.Sin(3.14f/180*30) * radius, endPosition.z - Mathf.Cos(3.14f/180*30) * radius),
                new Vector3(endPosition.x, endPosition.y, endPosition.z - radius),
                new Vector3(endPosition.x, endPosition.y - Mathf.Sin(3.14f/180*30) * radius, endPosition.z - Mathf.Cos(3.14f/180*30) * radius),
                new Vector3(endPosition.x, endPosition.y - Mathf.Sin(3.14f/180*60) * radius, endPosition.z - Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(endPosition.x, endPosition.y - radius, endPosition.z),
                new Vector3(endPosition.x, endPosition.y - Mathf.Sin(3.14f/180*60) * radius, endPosition.z + Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(endPosition.x, endPosition.y - Mathf.Sin(3.14f/180*30) * radius, endPosition.z + Mathf.Cos(3.14f/180*30) * radius)
            };
        }
        //Y轴向线条
        else if (Ygap >= Xgap && Ygap >= Zgap)
        {
            Vector3 middle;
            if (startPosition.y < endPosition.y)
            {
                middle = startPosition;
                startPosition = endPosition;
                endPosition = middle;
                trade = true;
            }

            _Vertices = new Vector3[24]
            {
                new Vector3(startPosition.x, startPosition.y, startPosition.z + radius),
                new Vector3(startPosition.x - Mathf.Sin(3.14f/180*30) * radius, startPosition.y, startPosition.z + Mathf.Cos(3.14f/180*30) * radius),
                new Vector3(startPosition.x - Mathf.Sin(3.14f/180*60) * radius, startPosition.y, startPosition.z + Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(startPosition.x - radius, startPosition.y, startPosition.z),
                new Vector3(startPosition.x - Mathf.Sin(3.14f/180*60) * radius, startPosition.y, startPosition.z - Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(startPosition.x - Mathf.Sin(3.14f/180*30) * radius, startPosition.y, startPosition.z - Mathf.Cos(3.14f/180*30) * radius),
                new Vector3(startPosition.x, startPosition.y, startPosition.z - radius),
                new Vector3(startPosition.x + Mathf.Sin(3.14f/180*30) * radius, startPosition.y, startPosition.z - Mathf.Cos(3.14f/180*30) * radius),
                new Vector3(startPosition.x + Mathf.Sin(3.14f/180*60) * radius, startPosition.y, startPosition.z - Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(startPosition.x + radius, startPosition.y, startPosition.z),
                new Vector3(startPosition.x + Mathf.Sin(3.14f/180*60) * radius, startPosition.y, startPosition.z + Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(startPosition.x + Mathf.Sin(3.14f/180*30) * radius, startPosition.y, startPosition.z + Mathf.Cos(3.14f/180*30) * radius),

                new Vector3(endPosition.x, endPosition.y, endPosition.z + radius),
                new Vector3(endPosition.x - Mathf.Sin(3.14f/180*30) * radius, endPosition.y, endPosition.z + Mathf.Cos(3.14f/180*30) * radius),
                new Vector3(endPosition.x - Mathf.Sin(3.14f/180*60) * radius, endPosition.y, endPosition.z + Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(endPosition.x - radius, endPosition.y, endPosition.z),
                new Vector3(endPosition.x - Mathf.Sin(3.14f/180*60) * radius, endPosition.y, endPosition.z - Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(endPosition.x - Mathf.Sin(3.14f/180*30) * radius, endPosition.y, endPosition.z - Mathf.Cos(3.14f/180*30) * radius),
                new Vector3(endPosition.x, endPosition.y, endPosition.z - radius),
                new Vector3(endPosition.x + Mathf.Sin(3.14f/180*30) * radius, endPosition.y, endPosition.z - Mathf.Cos(3.14f/180*30) * radius),
                new Vector3(endPosition.x + Mathf.Sin(3.14f/180*60) * radius, endPosition.y, endPosition.z - Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(endPosition.x + radius, endPosition.y, endPosition.z),
                new Vector3(endPosition.x + Mathf.Sin(3.14f/180*60) * radius, endPosition.y, endPosition.z + Mathf.Cos(3.14f/180*60) * radius),
                new Vector3(endPosition.x + Mathf.Sin(3.14f/180*30) * radius, endPosition.y, endPosition.z + Mathf.Cos(3.14f/180*30) * radius),
            };
        }
        //Z轴向线条
        else if (Zgap >= Xgap && Zgap >= Ygap)
        {
            Vector3 middle;
            if (startPosition.z < endPosition.z)
            {
                middle = startPosition;
                startPosition = endPosition;
                endPosition = middle;
                trade = true;
            }

            _Vertices = new Vector3[]
            {
                new Vector3(startPosition.x - radius, startPosition.y, startPosition.z),
                new Vector3(startPosition.x - Mathf.Cos(3.14f/180*30) * radius, startPosition.y + Mathf.Sin(3.14f/180*30) * radius, startPosition.z),
                new Vector3(startPosition.x - Mathf.Cos(3.14f/180*60) * radius, startPosition.y + Mathf.Sin(3.14f/180*60) * radius, startPosition.z),
                new Vector3(startPosition.x, startPosition.y + radius, startPosition.z),
                new Vector3(startPosition.x + Mathf.Cos(3.14f/180*60) * radius, startPosition.y + Mathf.Sin(3.14f/180*60) * radius, startPosition.z),
                new Vector3(startPosition.x + Mathf.Cos(3.14f/180*30) * radius, startPosition.y + Mathf.Sin(3.14f/180*30) * radius, startPosition.z),
                new Vector3(startPosition.x + radius, startPosition.y, startPosition.z),
                new Vector3(startPosition.x + Mathf.Cos(3.14f/180*30) * radius, startPosition.y - Mathf.Sin(3.14f/180*30) * radius, startPosition.z),
                new Vector3(startPosition.x + Mathf.Cos(3.14f/180*60) * radius, startPosition.y - Mathf.Sin(3.14f/180*60) * radius, startPosition.z),
                new Vector3(startPosition.x, startPosition.y - radius, startPosition.z),
                new Vector3(startPosition.x - Mathf.Cos(3.14f/180*60) * radius, startPosition.y - Mathf.Sin(3.14f/180*60) * radius, startPosition.z),
                new Vector3(startPosition.x - Mathf.Cos(3.14f/180*30) * radius, startPosition.y - Mathf.Sin(3.14f/180*30) * radius, startPosition.z),

                new Vector3(endPosition.x - radius, endPosition.y, endPosition.z),
                new Vector3(endPosition.x - Mathf.Cos(3.14f/180*30) * radius, endPosition.y + Mathf.Sin(3.14f/180*30) * radius, endPosition.z),
                new Vector3(endPosition.x - Mathf.Cos(3.14f/180*60) * radius, endPosition.y + Mathf.Sin(3.14f/180*60) * radius, endPosition.z),
                new Vector3(endPosition.x, endPosition.y + radius, endPosition.z),
                new Vector3(endPosition.x + Mathf.Cos(3.14f/180*60) * radius, endPosition.y + Mathf.Sin(3.14f/180*60) * radius, endPosition.z),
                new Vector3(endPosition.x + Mathf.Cos(3.14f/180*30) * radius, endPosition.y + Mathf.Sin(3.14f/180*30) * radius, endPosition.z),
                new Vector3(endPosition.x + radius, endPosition.y, endPosition.z),
                new Vector3(endPosition.x + Mathf.Cos(3.14f/180*30) * radius, endPosition.y - Mathf.Sin(3.14f/180*30) * radius, endPosition.z),
                new Vector3(endPosition.x + Mathf.Cos(3.14f/180*60) * radius, endPosition.y - Mathf.Sin(3.14f/180*60) * radius, endPosition.z),
                new Vector3(endPosition.x, endPosition.y - radius, endPosition.z),
                new Vector3(endPosition.x - Mathf.Cos(3.14f/180*60) * radius, endPosition.y - Mathf.Sin(3.14f/180*60) * radius, endPosition.z),
                new Vector3(endPosition.x - Mathf.Cos(3.14f/180*30) * radius, endPosition.y - Mathf.Sin(3.14f/180*30) * radius, endPosition.z),
            };
        }
        
        //生成所有面
        int[] _Triangles = new int[72]
        {
            //顶点0,1,12按顺序生成一个面，面朝向顺时针方向
            0,1,12,
            1,13,12,
            1,2,13,
            2,14,13,
            2,3,14,
            3,15,14,
            3,4,15,
            4,16,15,
            4,5,16,
            5,17,16,
            5,6,17,
            6,18,17,
            6,7,18,
            7,19,18,
            7,8,19,
            8,20,19,
            8,9,20,
            9,21,20,
            9,10,21,
            10,22,21,
            10,11,22,
            11,23,22,
            11,0,23,
            0,12,23
        };

        mesh = new Mesh();
        mesh.vertices = _Vertices;
        mesh.triangles = _Triangles;
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
