using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMesh
{
    private List<MPoint> pointList;

    private List<MEdge> edgeList;

    private List<MFace> faceList;

    public AABB boundingBox { get; private set; }

    public MMesh()
    {
        boundingBox = new AABB();
        pointList = new List<MPoint>();
        edgeList = new List<MEdge>();
        faceList = new List<MFace>();
    }

    public void Render(Matrix4x4 matrix)
    {
        foreach(MPoint point in pointList)
        {
            point.Render(matrix);
        }
        foreach(MEdge edge in edgeList)
        {
            edge.Render(matrix);
        }
        foreach(MFace face in faceList)
        {
            face.Render(matrix);
        }
    }

    public MPoint CreatePoint(Vector3 position)
    {
        MPoint point = new MPoint(position);
        int i;
        if ((i = AddPointToMesh(point)) != -1)
        {
            return pointList[i];
        } else
        {
            return point;
        }
    }

    public MLinearEdge CreateLinearEdge(MPoint start, MPoint end)
    {
        MLinearEdge edge = new MLinearEdge(start, end);
        if (!edge.IsValid()) return null;
        int i;
        if ((i = AddEdgeToMesh(edge)) != -1)
        {
            return edgeList[i] as MLinearEdge;
        } else
        {
            return edge;
        }
    }

    public MCurveEdge CreateCurveEdge(MPoint center, float radius, Vector3 normal)
    {
        // TODO: 创建圆边
        return null;
    }

    public MPolygonFace CreatePolygonFace(List<MLinearEdge> edges)
    {
        MPolygonFace face = new MPolygonFace(edges);
        if (!face.IsValid()) return null;
        int i;
        if((i = AddFaceToMesh(face)) != -1)
        {
            return faceList[i] as MPolygonFace;
        } else
        {
            return face;
        }
    }

    public MCircleFace CreateCircleFace(MCurveEdge edge)
    {
        // TODO： 创建圆形
        return null;
    }

    public MConeFace CreateConeFace(MPoint top, MCurveEdge bottom)
    {
        // TODO: 创建锥面
        return null;
    }

    public MCylinderFace CreateCylinderFace(MCurveEdge top, MCurveEdge bottom)
    {
        // TODO: 创建柱面
        return null;
    }

    public MSphereFace CreateSphereFace(MPoint center, float radius)
    {
        // TODO: 创建球面
        return null;
    }

    public float GetClosetPoint(out MPoint point, Vector3 pos)
    {
        float min = float.MaxValue;
        float temp;
        MPoint tp = null;
        foreach(MPoint p in pointList)
        {
            if((temp = p.CalcDistance(pos)) < min)
            {
                min = temp;
                tp = p;
            }
        }
        point = tp;
        return min;
    }

    public float GetClosetEdge(out MEdge edge, Vector3 pos)
    {
        float min = float.MaxValue;
        float temp;
        MEdge te = null;
        foreach(MEdge e in edgeList)
        {
            if((temp = e.CalcDistance(pos)) < min)
            {
                min = temp;
                te = e;
            }
        }
        edge = te;
        return min;
    }

    public float GetClosetFace(out MFace face, Vector3 pos, bool useBoundingBox, float precision)
    {
        float min = float.MaxValue;
        float temp;
        MFace tf = null;
        foreach(MFace f in faceList)
        {
            if (useBoundingBox && !f.boundingBox.Contains(pos, precision)) continue;
            if((temp = f.CalcDistance(pos)) < min)
            {
                min = temp;
                tf = f;
            }
        }
        face = tf;
        return min;
    }

    private int AddPointToMesh(MPoint point)
    {
        int i;
        if ((i = pointList.IndexOf(point)) == -1)
        {
            pointList.Add(point);
        }
        return i;
    }

    private int AddEdgeToMesh(MEdge edge)
    {
        int i, j;
        if ((i = edgeList.IndexOf(edge)) == -1)
        {
            edgeList.Add(edge);
            switch (edge.edgeType)
            {
                case MEdge.MEdgeType.LINEAR:
                    MLinearEdge le = edge as MLinearEdge;
                    j = AddPointToMesh(le.start);
                    if(j != -1)
                    {
                        le.start = pointList[j];
                    }
                    le.start.edges.Add(edge);
                    j = AddPointToMesh(le.end);
                    if(j != -1)
                    {
                        le.end = pointList[j];
                    }
                    le.end.edges.Add(edge);
                    break;
                case MEdge.MEdgeType.CURVE:
                    MCurveEdge ce = edge as MCurveEdge;
                    j = AddPointToMesh(ce.center);
                    if(j != -1)
                    {
                        ce.center = pointList[j];
                    }
                    ce.center.edges.Add(edge);
                    break;
                default:
                    Debug.Log("MMesh: AddEdgeToList: unhandled edge type " + edge.edgeType);
                    break;
            }
        }
        return i;
    }

    private int AddFaceToMesh(MFace face)
    {
        int i, j;
        if((i = faceList.IndexOf(face)) == -1)
        {
            faceList.Add(face);
            switch (face.faceType)
            {
                case MFace.MFaceType.POLYGON:
                    MPolygonFace pf = face as MPolygonFace;
                    int count = pf.edgeList.Count;
                    for(int k = 0; k < count; k++)
                    {
                        j = AddEdgeToMesh(pf.edgeList[k]);
                        if(j != -1)
                        {
                            pf.edgeList[k] = edgeList[j] as MLinearEdge;
                        }
                        pf.edgeList[k].faces.Add(face);
                    }
                    break;
                case MFace.MFaceType.CIRCLE:
                    MCircleFace cirf = face as MCircleFace;
                    j = AddEdgeToMesh(cirf.circle);
                    if(j != -1)
                    {
                        cirf.circle = edgeList[j] as MCurveEdge;
                    }
                    cirf.circle.faces.Add(face);
                    break;
                case MFace.MFaceType.CONE:
                    MConeFace conf = face as MConeFace;
                    j = AddEdgeToMesh(conf.bottom);
                    if(j != -1)
                    {
                        conf.bottom = edgeList[j] as MCurveEdge;
                    }
                    conf.bottom.faces.Add(face);
                    j = AddPointToMesh(conf.top);
                    if(j != -1)
                    {
                        conf.top = pointList[j];
                    }
                    conf.top.faces.Add(face);
                    break;
                case MFace.MFaceType.CYLINDER:
                    MCylinderFace cylf = face as MCylinderFace;
                    j = AddEdgeToMesh(cylf.top);
                    if(j != -1)
                    {
                        cylf.top = edgeList[j] as MCurveEdge;
                    }
                    cylf.top.faces.Add(face);
                    j = AddEdgeToMesh(cylf.bottom);
                    if(j != -1)
                    {
                        cylf.bottom = edgeList[j] as MCurveEdge;
                    }
                    cylf.bottom.faces.Add(face);
                    break;
                case MFace.MFaceType.SPHERE:
                    MSphereFace sf = face as MSphereFace;
                    j = AddPointToMesh(sf.center);
                    if(j != -1)
                    {
                        sf.center = pointList[j];
                    }
                    sf.center.faces.Add(face);
                    break;
                default:
                    Debug.Log("MMesh: AddFaceToList: unhandled edge type " + face.faceType);
                    break;
            }
            boundingBox.AdjustToContain(face.boundingBox);
        }
        return i;
    }

}
