using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPolygonFace : MFace
{
    public List<MLinearEdge> edgeList
    {
        get;
        private set;
    }

    List<MPoint> sortedPoints;

    public Vector3 normal { get; private set; }

    bool buildSuccess = true;

    MeshCollider collider;

    public MPolygonFace(List<MLinearEdge> edgeList)
    {
        faceType = MFaceType.POLYGON;
        entityType = MEntityType.FACE;
        entityStatus = MEntityStatus.DEFAULT;
        if (edgeList.Count < 3)
        {
            buildSuccess = false;
            return;
        }
        this.edgeList = edgeList;
        GenerateSortedPoint();
        if(buildSuccess)CalcNormal();
        if (buildSuccess) InitMesh();
    }

    override
    public float CalcDistance(Vector3 point)
    {
        Vector3 projectionPoint = MHelperFunctions.PointProjectionInFace(point, normal, sortedPoints[0].position);
        Vector3 rotateAxis;
        float rotateAngle;
        MHelperFunctions.CalcRotateAxisAndAngle(out rotateAxis, out rotateAngle, normal, new Vector3(0, 0, 1));
        List<Vector3> rotatePoints = new List<Vector3>();
        foreach (MPoint p in sortedPoints)
        {
            rotatePoints.Add(MHelperFunctions.CalcRotate(p.position, rotateAxis, rotateAngle));
        }
        projectionPoint = MHelperFunctions.CalcRotate(projectionPoint, rotateAxis, rotateAngle);
        if (MHelperFunctions.InPolygon(projectionPoint, rotatePoints))
        {
            return MHelperFunctions.DistanceP2F(point, normal, sortedPoints[0].position);
        }
        else
        {
            float min = float.MaxValue;
            foreach (MLinearEdge edge in edgeList)
            {
                min = Mathf.Min(min, edge.CalcDistance(point));
            }
            return min;
        }
    }

    override
    public Vector3 SpecialPointFind(Vector3 point)
    {
        return MHelperFunctions.PointProjectionInFace(point, normal, sortedPoints[0].position);
    }

    override
    public Vector3 GetProjection(Vector3 target, Vector3 assistant)
    {
        return MHelperFunctions.PointProjectionInFace(target, normal, sortedPoints[0].position);
    }

    override
    public Vector3 GetVerticalPoint(Vector3 startPoint, Vector3 curPoint)
    {
        return MHelperFunctions.PointProjectionInLine(curPoint, normal, startPoint);
    }

    override
    public float GetSurface()
    {
        float surface = 0;
        int count = sortedPoints.Count;
        Vector3 v = sortedPoints[0].position;
        for(int i = 1; i < count - 1; i++)
        {
            surface += MHelperFunctions.TriangleSurface(v, sortedPoints[i].position, sortedPoints[i + 1].position);
        }
        return surface;
    }

    override
    public void UpdateMesh()
    {
        InitMesh();
    }

    private void InitMesh()
    {
        int count = sortedPoints.Count;
        Vector3[] vertices = new Vector3[count];
        Vector3[] normals = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            vertices[i] = sortedPoints[i].position;
            normals[i] = normal;
        }
        int[] triangles = new int[3 * (count - 2)];
        
        for (int i = 0; i < count - 2; i++)
        {
            triangles[3 * i] = 0;
            triangles[3 * i + 1] = i + 1;
            triangles[3 * i + 2] = i + 2;
        }
        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.RecalculateBounds();
        boundingBox = new AABB(mesh.bounds);
    }

    override
    public bool IsValid()
    {
        if (!buildSuccess) return false;
        foreach(MLinearEdge edge in edgeList)
        {
            if (!edge.IsValid()) return false;
            if (!MHelperFunctions.Perpendicular(edge.direction, normal)) return false;
        }
        return true;
    }

    override
    public bool Equals(object obj)
    {
        if (obj == null)
            return false;

        if (this.GetType() != obj.GetType())
            return false;

        return Equals(obj as MPolygonFace);
    }

    private bool Equals(MPolygonFace obj)
    {
        return IdenticalLoop(sortedPoints, obj.sortedPoints);
    }

    override
    public int GetHashCode()
    {
        return this.sortedPoints.GetHashCode();
    }

    private void CalcNormal()
    {
        MLinearEdge e1 = edgeList[0];
        MLinearEdge e2 = e1;
        int i;
        int count = edgeList.Count;
        for (i = 1; i < count; i++)
        {
            e2 = edgeList[i];
            if (!e1.Parallel(e2)) break;
        }
        if (i != count)
        {
            normal = Vector3.Normalize(Vector3.Cross(e1.direction, e2.direction));
        }
        else
        {
            Debug.Log("MPolygonFace: CalcNormal: wrong edgeList");
            buildSuccess = false;
        }
    }

    private void GenerateSortedPoint()
    {
        sortedPoints = new List<MPoint>();
        List<MLinearEdge> edges = new List<MLinearEdge>(edgeList);
        MLinearEdge edge = edges[0];
        MPoint p = edge.start;
        edges.Remove(edge);
        sortedPoints.Add(p);
        p = edge.end;
        bool find = false;
        int count = edgeList.Count;
        for(int i = 1; i < count; i++)
        {
            find = false;
            foreach(MLinearEdge e in edges)
            {
                if(e.start == p)
                {
                    edge = e;
                    sortedPoints.Add(p);
                    p = e.end;
                    find = true;
                    break;
                } else if(e.end == p)
                {
                    edge = e;
                    sortedPoints.Add(p);
                    p = e.start;
                    find = true;
                    break;
                }
            }
            if (!find)
            {
                Debug.Log("MPolygonFace: GenerateSortedPoint: wrong edge list");
                buildSuccess = false;
                break;
            } else
            {
                edges.Remove(edge);
            }
        }
    }
    
    private bool IdenticalLoop(List<MPoint> p1, List<MPoint> p2)
    {
        int count;
        if ((count = p1.Count) != p2.Count || count < 3) return false;
        int j = p2.IndexOf(p1[0]);
        if (j == -1) return false;
        int sig = 1;
        if (p1[1].Equals(p2[(j + 1) % count])) sig = 1;
        else if (p1[1].Equals(p2[(j + count - 1) % count])) sig = count - 1;
        else return false;
        for (int i = 0; i < count; i++, j = (j + sig) % count)
        {
            if (!p1[i].Equals(p2[j])) return false;
        }
        return true;
    }
}