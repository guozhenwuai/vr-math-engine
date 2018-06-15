﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMesh
{
    public List<MPoint> pointList { get; private set; }

    public List<MEdge> edgeList { get; private set; }

    public List<MFace> faceList { get; private set; }

    public AABB boundingBox { get; private set; }

    private List<string> identifierLetters;

    private Dictionary<MPoint, GameObject> pointIdentifiers;

    private List<GameObject> freeGameObjects;

    public MMesh()
    {
        boundingBox = new AABB();
        pointList = new List<MPoint>();
        edgeList = new List<MEdge>();
        faceList = new List<MFace>();
        pointIdentifiers = new Dictionary<MPoint, GameObject>();
        identifierLetters = new List<string> {
            "A", "B", "C", "D", "E", "F", "G",
            "H", "I", "J", "K", "L", "M", "N",
            "O", "P", "Q", "R", "S", "T",
            "U", "V", "W", "X", "Y", "Z" };
        freeGameObjects = new List<GameObject>();
        for(int i = 0; i < identifierLetters.Count; i++)
        {
            freeGameObjects.Add(MPrefab.GetTextMesh());
        }
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

    public void RenderFace(Matrix4x4 matrix, Material mat)
    {
        foreach(MFace face in faceList)
        {
            face.Render(matrix, mat);
        }
    }

    public void Highlight()
    {
        foreach(MPoint entity in pointList)
        {
            entity.entityStatus = MEntity.MEntityStatus.ACTIVE;
        }
        foreach(MEdge entity in edgeList)
        {
            entity.entityStatus = MEntity.MEntityStatus.ACTIVE;
        }
        foreach(MFace entity in faceList)
        {
            entity.entityStatus = MEntity.MEntityStatus.ACTIVE;
        }
    }

    public void ResetStatus()
    {
        foreach (MPoint entity in pointList)
        {
            entity.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
        foreach (MEdge entity in edgeList)
        {
            entity.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
        foreach (MFace entity in faceList)
        {
            entity.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
    }

    public void Select()
    {
        foreach (MPoint entity in pointList)
        {
            entity.entityStatus = MEntity.MEntityStatus.SELECT;
        }
        foreach (MEdge entity in edgeList)
        {
            entity.entityStatus = MEntity.MEntityStatus.SELECT;
        }
        foreach (MFace entity in faceList)
        {
            entity.entityStatus = MEntity.MEntityStatus.SELECT;
        }
    }

    public bool RemoveEntity(MEntity entity)
    {
        switch (entity.entityType)
        {
            case MEntity.MEntityType.POINT:
                return RemovePoint((MPoint)entity);
            case MEntity.MEntityType.EDGE:
                return RemoveEdge((MEdge)entity);
            case MEntity.MEntityType.FACE:
                return RemoveFace((MFace)entity);
        }
        return false;
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
        MCurveEdge edge = new MCurveEdge(center, normal, radius);
        if (!edge.IsValid()) return null;
        int i;
        if((i = AddEdgeToMesh(edge)) != -1)
        {
            return edgeList[i] as MCurveEdge;
        }
        else
        {
            return edge;
        }
    }

    public MGeneralEdge CreateGeneralEdge(List<Vector3> points)
    {
        MGeneralEdge edge = new MGeneralEdge(points);
        if (!edge.IsValid()) return null;
        int i;
        if((i = AddEdgeToMesh(edge)) != -1)
        {
            return edgeList[i] as MGeneralEdge;
        }
        else
        {
            return edge;
        }
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
        MCircleFace face = new MCircleFace(edge);
        if (!face.IsValid()) return null;
        int i;
        if ((i = AddFaceToMesh(face)) != -1)
        {
            return faceList[i] as MCircleFace;
        }
        else
        {
            return face;
        }
    }

    public MConeFace CreateConeFace(MPoint top, MCurveEdge bottom)
    {
        MConeFace face = new MConeFace(top, bottom);
        if (!face.IsValid()) return null;
        int i;
        if ((i = AddFaceToMesh(face)) != -1)
        {
            return faceList[i] as MConeFace;
        }
        else
        {
            return face;
        }
    }

    public MCylinderFace CreateCylinderFace(MCurveEdge top, MCurveEdge bottom)
    {
        MCylinderFace face = new MCylinderFace(top, bottom);
        if (!face.IsValid()) return null;
        int i;
        if ((i = AddFaceToMesh(face)) != -1)
        {
            return faceList[i] as MCylinderFace;
        }
        else
        {
            return face;
        }
    }

    public MSphereFace CreateSphereFace(MPoint center, float radius)
    {
        MSphereFace face = new MSphereFace(center, radius);
        if (!face.IsValid()) return null;
        int i;
        if((i = AddFaceToMesh(face)) != -1)
        {
            return faceList[i] as MSphereFace;
        } else
        {
            return face;
        }
    }

    public MGeneralFlatFace CreateGeneralFlatFace(List<Vector3> points)
    {
        MGeneralFlatFace face = new MGeneralFlatFace(points);
        if (!face.IsValid()) return null;
        int i;
        if ((i = AddFaceToMesh(face)) != -1)
        {
            return faceList[i] as MGeneralFlatFace;
        }
        else
        {
            return face;
        }
    }

    public MGeneralCurveFace CreateGeneralCurveFace(Mesh mesh)
    {
        MGeneralCurveFace face = new MGeneralCurveFace(mesh);
        if (!face.IsValid()) return null;
        int i;
        if ((i = AddFaceToMesh(face)) != -1)
        {
            return faceList[i] as MGeneralCurveFace;
        }
        else
        {
            return face;
        }
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

    public MLinearEdge GetLinearEdge()
    {
        foreach(MEdge edge in edgeList)
        {
            if (edge.edgeType == MEdge.MEdgeType.LINEAR) return edge as MLinearEdge;
        }
        return null;
    }

    public bool IsEmpty()
    {
        return pointList.Count == 0 && edgeList.Count == 0 && faceList.Count == 0;
    }

    public void SetTransformParent(Transform transform)
    {
        foreach(KeyValuePair<MPoint, GameObject> pair in pointIdentifiers)
        {
            pair.Value.transform.parent = transform;
            pair.Value.transform.position = transform.localToWorldMatrix.MultiplyPoint(pair.Key.position);
        }
        foreach(GameObject go in freeGameObjects)
        {
            go.transform.parent = transform;
        }
    }

    public string GetPointIdentifier(MPoint point)
    {
        GameObject go;
        if(pointIdentifiers.TryGetValue(point, out go))
        {
            return go.GetComponent<TextMesh>().text;
        }
        else
        {
            return null;
        }
    }

    public void Destroy()
    {
        foreach(GameObject go in pointIdentifiers.Values)
        {
            Object.Destroy(go);
        }
        foreach(GameObject go in freeGameObjects)
        {
            Object.Destroy(go);
        }
    }

    private int AddPointToMesh(MPoint point)
    {
        int i;
        if ((i = pointList.IndexOf(point)) == -1)
        {
            pointList.Add(point);
            TryAddIdentifier(point);
            boundingBox.AdjustToContain(point.position);
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
                    foreach(Vector3 v in ce.mesh.vertices)
                    {
                        boundingBox.AdjustToContain(v);
                    }
                    j = AddPointToMesh(ce.center);
                    if(j != -1)
                    {
                        ce.center = pointList[j];
                    }
                    ce.center.edges.Add(edge);
                    break;
                case MEdge.MEdgeType.GENERAL:
                    MGeneralEdge ge = edge as MGeneralEdge;
                    foreach(Vector3 v in ge.points)
                    {
                        boundingBox.AdjustToContain(v);
                    }
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
                case MFace.MFaceType.GENERAL_FLAT:
                    break;
                case MFace.MFaceType.GENERAL_CURVE:
                    break;
                default:
                    Debug.Log("MMesh: AddFaceToList: unhandled edge type " + face.faceType);
                    break;
            }
            boundingBox.AdjustToContain(face.boundingBox);
        }
        return i;
    }

    private bool RemoveFace(MFace face)
    {
        return faceList.Remove(face);
    }

    private bool RemoveEdge(MEdge edge)
    {
        if (!edgeList.Remove(edge)) return false;
        foreach(MFace face in edge.faces)
        {
            RemoveFace(face);
        }
        return true;
    }

    private bool RemovePoint(MPoint point)
    {
        if (!pointList.Remove(point)) return false;
        RemoveIdentifier(point);
        foreach(MEdge edge in point.edges)
        {
            RemoveEdge(edge);
        }
        foreach(MFace face in point.faces)
        {
            RemoveFace(face);
        }
        return true;
    }

    private void RemoveIdentifier(MPoint point)
    {
        GameObject go;
        if (pointIdentifiers.TryGetValue(point, out go))
        {
            pointIdentifiers.Remove(point);
            identifierLetters.Add(go.GetComponent<TextMesh>().text);
            go.SetActive(false);
            freeGameObjects.Add(go);
            if(freeGameObjects.Count == 1)
            {
                UpdateIdentifiers();
            }
        }
    }

    private bool TryAddIdentifier(MPoint point)
    {
        if (freeGameObjects.Count == 0) return false;
        string identifier = identifierLetters[0];
        identifierLetters.RemoveAt(0);
        GameObject go = freeGameObjects[0];
        if (go.transform.parent != null)
        {
            go.transform.position = go.transform.parent.transform.localToWorldMatrix.MultiplyPoint(point.position);
        }
        go.SetActive(true);
        freeGameObjects.RemoveAt(0);
        go.GetComponent<TextMesh>().text = identifier;
        pointIdentifiers.Add(point, go);
        return true;
    }

    private void UpdateIdentifiers()
    {
        foreach(MPoint p in pointList)
        {
            if (freeGameObjects.Count == 0) break;
            if (!pointIdentifiers.ContainsKey(p))
            {
                TryAddIdentifier(p);
            }
        }
    }
}
