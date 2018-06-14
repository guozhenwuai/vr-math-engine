using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class MObject
{
    public enum MPrefabType { CUBE, SPHERE, CYLINDER, PRISM, PYRAMID, CONE, REGULAR_TRIANGLE, CIRCLE};

    public enum MInteractMode { ALL, POINT_ONLY, EDGE_ONLY, FACE_ONLY, POINT_EXPT, EDGE_EXPT, FACE_EXPT};

    public enum MObjectState { DEFAULT, ACTIVE, SELECT};

    public MObjectState objectState;

    public Transform transform;

    public GameObject gameObject;

    private MMesh mesh;

    public MLinearEdge refEdge = null;

    private float refEdgeLength;

    private Transform textPlane;

    private TextMesh textMesh;

    public Matrix4x4 worldToLocalMatrix
    {
        get { return transform.worldToLocalMatrix; }
    }

    public Matrix4x4 localToWorldMatrix
    {
        get { return transform.localToWorldMatrix; }
    }

    public float scale
    {
        get { return transform.localScale.x; }
        set { transform.localScale = new Vector3(value, value, value); }
    }

    public Vector3 position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public Vector3 rotation
    {
        get { return transform.eulerAngles; }
        set { transform.eulerAngles = value; }
    }

    // 针对导入模型的初始化
    public MObject(GameObject template, string filename)
    {
        gameObject = GameObject.Instantiate(template);
        mesh = new MMesh();
        using(StreamReader sr = new StreamReader(MDefinitions.SAVE_PATH + "/" + filename))
        {
            string line = sr.ReadLine();
            string[] lineComponent;
            bool parseLine;
            while (line!=null)
            {
                lineComponent = line.Split(' ');
                string type = lineComponent[0];
                parseLine = true;
                switch (type)
                {
                    case "p":
                        float x = (float)Convert.ToDouble(lineComponent[1]);
                        float y = (float)Convert.ToDouble(lineComponent[2]);
                        float z = (float)Convert.ToDouble(lineComponent[3]);
                        Vector3 position = new Vector3(x, y, z);
                        mesh.CreatePoint(position);
                        break;
                    case "le":
                        int start = Convert.ToInt32(lineComponent[1]);
                        int end = Convert.ToInt32(lineComponent[2]);
                        List<MPoint> pointList = mesh.pointList;
                        mesh.CreateLinearEdge(pointList[start], pointList[end]);
                        break;
                    case "ce":
                        int center = Convert.ToInt32(lineComponent[1]);
                        float radius = (float)Convert.ToDouble(lineComponent[2]);
                        Vector3 normal = new Vector3(
                            (float)Convert.ToDouble(lineComponent[3]), 
                            (float)Convert.ToDouble(lineComponent[4]), 
                            (float)Convert.ToDouble(lineComponent[5]));
                        mesh.CreateCurveEdge(mesh.pointList[center], radius, normal);
                        break;
                    case "ge":
                        List<Vector3> points = new List<Vector3>();
                        line = sr.ReadLine();
                        while (line != null)
                        {
                            lineComponent = line.Split(' ');
                            if(lineComponent[0] != "v")
                            {
                                parseLine = false;
                                break;
                            }
                            else
                            {
                                points.Add(new Vector3(
                                (float)Convert.ToDouble(lineComponent[1]),
                                (float)Convert.ToDouble(lineComponent[2]),
                                (float)Convert.ToDouble(lineComponent[3])));
                            }
                            line = sr.ReadLine();
                        }
                        mesh.CreateGeneralEdge(points);
                        break;
                    case "polyf":
                        List<MLinearEdge> edgeList = new List<MLinearEdge>();
                        line = sr.ReadLine();
                        while (line != null)
                        {
                            lineComponent = line.Split(' ');
                            if (lineComponent[0] != "e")
                            {
                                parseLine = false;
                                break;
                            }
                            else
                            {
                                int index = Convert.ToInt32(lineComponent[1]);
                                edgeList.Add((MLinearEdge)mesh.edgeList[index]);
                            }
                            line = sr.ReadLine();
                        }
                        mesh.CreatePolygonFace(edgeList);
                        break;
                    case "circf":
                        int circle = Convert.ToInt32(lineComponent[1]);
                        mesh.CreateCircleFace((MCurveEdge)mesh.edgeList[circle]);
                        break;
                    case "conf":
                        int top = Convert.ToInt32(lineComponent[1]);
                        int bottom = Convert.ToInt32(lineComponent[2]);
                        mesh.CreateConeFace(mesh.pointList[top], (MCurveEdge)mesh.edgeList[bottom]);
                        break;
                    case "cylf":
                        top = Convert.ToInt32(lineComponent[1]);
                        bottom = Convert.ToInt32(lineComponent[2]);
                        mesh.CreateCylinderFace((MCurveEdge)mesh.edgeList[top], (MCurveEdge)mesh.edgeList[bottom]);
                        break;
                    case "sf":
                        center = Convert.ToInt32(lineComponent[1]);
                        radius = (float)Convert.ToDouble(lineComponent[2]);
                        mesh.CreateSphereFace(mesh.pointList[center], radius);
                        break;
                    case "gff":
                        points = new List<Vector3>();
                        line = sr.ReadLine();
                        while (line != null)
                        {
                            lineComponent = line.Split(' ');
                            if (lineComponent[0] != "v")
                            {
                                parseLine = false;
                                break;
                            }
                            else
                            {
                                points.Add(new Vector3(
                                (float)Convert.ToDouble(lineComponent[1]),
                                (float)Convert.ToDouble(lineComponent[2]),
                                (float)Convert.ToDouble(lineComponent[3])));
                            }
                            line = sr.ReadLine();
                        }
                        mesh.CreateGeneralFlatFace(points);
                        break;
                    case "gcf":
                        List<Vector3> vertices = new List<Vector3>();
                        line = sr.ReadLine();
                        while (line != null)
                        {
                            lineComponent = line.Split(' ');
                            if (lineComponent[0] != "v")
                            {
                                parseLine = false;
                                break;
                            }
                            else
                            {
                                vertices.Add(new Vector3(
                                (float)Convert.ToDouble(lineComponent[1]),
                                (float)Convert.ToDouble(lineComponent[2]),
                                (float)Convert.ToDouble(lineComponent[3])));
                            }
                            line = sr.ReadLine();
                        }
                        List<int> triangles = new List<int>();
                        if (!parseLine) line = sr.ReadLine();
                        while (line != null)
                        {
                            lineComponent = line.Split(' ');
                            if (lineComponent[0] != "f")
                            {
                                parseLine = false;
                                break;
                            }
                            else
                            {
                                triangles.Add(Convert.ToInt32(lineComponent[1]));
                                triangles.Add(Convert.ToInt32(lineComponent[2]));
                                triangles.Add(Convert.ToInt32(lineComponent[3]));
                            }
                            line = sr.ReadLine();
                        }
                        Mesh m = new Mesh();
                        m.vertices = vertices.ToArray();
                        m.triangles = triangles.ToArray();
                        m.RecalculateNormals();
                        mesh.CreateGeneralCurveFace(m);
                        break;
                }
                if(parseLine)line = sr.ReadLine();
            }
            sr.Close();
        }
        InitObject();
    }

    // 针对预制件的初始化
    public MObject(GameObject template, MPrefabType type)
    {
        gameObject = GameObject.Instantiate(template);
        switch (type)
        {
            case MPrefabType.CUBE:
                mesh = MCube.GetMMesh();
                break;
            case MPrefabType.SPHERE:
                mesh = MSphere.GetMMesh();
                break;
            case MPrefabType.CYLINDER:
                mesh = MCylinder.GetMMesh();
                break;
            case MPrefabType.CONE:
                mesh = MCone.GetMMesh();
                break;
            case MPrefabType.PRISM:
                mesh = MPrism.GetMMesh();
                break;
            case MPrefabType.PYRAMID:
                mesh = MPyramid.GetMMesh();
                break;
            case MPrefabType.REGULAR_TRIANGLE:
                mesh = MRegularTriangle.GetMMesh();
                break;
            case MPrefabType.CIRCLE:
                mesh = MCircle.GetMMesh();
                break;
            default:
                Debug.Log("Unknown prefab type: " + type);
                return;
        }
        InitObject();
    }

    // 针对分割后模型的初始化
    public MObject(GameObject template, MMesh mesh)
    {
        gameObject = GameObject.Instantiate(template);
        this.mesh = mesh;
        InitObject();
    }

    // 空的模型
    public MObject(GameObject template)
    {
        gameObject = GameObject.Instantiate(template);
        mesh = new MMesh();
        InitObject();
    }

    public bool ExportObject(string filename)
    {
        StringBuilder sb = new StringBuilder();
        List<MPoint> pointList = mesh.pointList;
        List<MEdge> edgeList = mesh.edgeList;
        List<MFace> faceList = mesh.faceList;
        foreach(MPoint point in pointList)
        {
            sb.Append(string.Format("p {0} {1} {2}\n", point.position.x, point.position.y, point.position.z));
        }
        foreach(MEdge edge in edgeList)
        {
            switch (edge.edgeType)
            {
                case MEdge.MEdgeType.LINEAR:
                    int start = pointList.IndexOf(((MLinearEdge)edge).start);
                    int end = pointList.IndexOf(((MLinearEdge)edge).end);
                    sb.Append(string.Format("le {0} {1}\n", start, end));
                    break;
                case MEdge.MEdgeType.CURVE:
                    MCurveEdge ce = edge as MCurveEdge;
                    int center = pointList.IndexOf(ce.center);
                    sb.Append(string.Format("ce {0} {1} {2} {3} {4}\n", center, ce.radius, ce.normal.x, ce.normal.y, ce.normal.z));
                    break;
                case MEdge.MEdgeType.GENERAL:
                    MGeneralEdge ge = edge as MGeneralEdge;
                    sb.Append("ge\n");
                    foreach(Vector3 v in ge.points)
                    {
                        sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
                    }
                    break;
            }
        }
        foreach(MFace face in faceList)
        {
            switch (face.faceType)
            {
                case MFace.MFaceType.POLYGON:
                    sb.Append("polyf\n");
                    foreach(MLinearEdge edge in ((MPolygonFace)face).edgeList)
                    {
                        int index = edgeList.IndexOf(edge);
                        sb.Append(string.Format("e {0}\n", index));
                    }
                    break;
                case MFace.MFaceType.CIRCLE:
                    MCircleFace circf = face as MCircleFace;
                    int circle = edgeList.IndexOf(circf.circle);
                    sb.Append(string.Format("circf {0}\n", circle));
                    break;
                case MFace.MFaceType.CONE:
                    MConeFace conf = face as MConeFace;
                    int top = pointList.IndexOf(conf.top);
                    int bottom = edgeList.IndexOf(conf.bottom);
                    sb.Append(string.Format("conf {0} {1}\n", top, bottom));
                    break;
                case MFace.MFaceType.CYLINDER:
                    MCylinderFace cylindf = face as MCylinderFace;
                    top = edgeList.IndexOf(cylindf.top);
                    bottom = edgeList.IndexOf(cylindf.bottom);
                    sb.Append(string.Format("cylf {0} {1}\n", top, bottom));
                    break;
                case MFace.MFaceType.SPHERE:
                    MSphereFace sf = face as MSphereFace;
                    int center = pointList.IndexOf(sf.center);
                    sb.Append(string.Format("sf {0} {1}\n", center, sf.radius));
                    break;
                case MFace.MFaceType.GENERAL_FLAT:
                    MGeneralFlatFace gff = face as MGeneralFlatFace;
                    sb.Append("gff\n");
                    foreach (Vector3 v in gff.points)
                    {
                        sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
                    }
                    break;
                case MFace.MFaceType.GENERAL_CURVE:
                    MGeneralCurveFace gcf = face as MGeneralCurveFace;
                    sb.Append("gcf\n");
                    foreach(Vector3 v in gcf.mesh.vertices)
                    {
                        sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
                    }
                    int count = gcf.mesh.triangles.Length / 3;
                    for(int i = 0; i < count; i++)
                    {
                        sb.Append(string.Format("f {0} {1} {2}\n", gcf.mesh.triangles[3 * i], gcf.mesh.triangles[3 * i + 1], gcf.mesh.triangles[3 * i + 2]));
                    }
                    break;
            }
        }
        if (!Directory.Exists(MDefinitions.SAVE_PATH))
        {
            Directory.CreateDirectory(MDefinitions.SAVE_PATH);
        }
        using (StreamWriter sw = new StreamWriter(MDefinitions.SAVE_PATH+"/" + filename))
        {
            sw.Write(sb.ToString());
            sw.Flush();
            sw.Close();
        }
        return true;
    }

    public bool HitObject(Vector3 pos)
    {
        Vector3 p = worldToLocalMatrix.MultiplyPoint(pos);
        if (mesh.boundingBox.Contains(p, MDefinitions.ACTIVE_DISTANCE))
        {
            return true;
        }
        return false;
    }

    public float Response(out MEntity entity, Vector3 pos, MInteractMode mode)
    {
        Vector3 p = worldToLocalMatrix.MultiplyPoint(pos);
        float dis = float.MaxValue;
        float res = float.MaxValue;
        MPoint point;
        MEdge edge;
        MFace face;
        MEntity e = null;
        switch (mode)
        {
            case MInteractMode.ALL:
                dis = mesh.GetClosetPoint(out point, p);
                if(dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    e = point;
                    res = dis;
                    break;
                }
                dis = mesh.GetClosetEdge(out edge, p);
                if(dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    e = edge;
                    res = dis;
                    break;
                }
                dis = mesh.GetClosetFace(out face, p, true, MDefinitions.ACTIVE_DISTANCE / scale);
                if(dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    e = face;
                    res = dis;
                    break;
                }
                break;
            case MInteractMode.POINT_ONLY:
                dis = mesh.GetClosetPoint(out point, p);
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    e = point;
                    res = dis;
                }
                break;
            case MInteractMode.EDGE_ONLY:
                dis = mesh.GetClosetEdge(out edge, p);
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    e = edge;
                    res = dis;
                }
                break;
            case MInteractMode.FACE_ONLY:
                dis = mesh.GetClosetFace(out face, p, true, MDefinitions.ACTIVE_DISTANCE / scale);
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    e = face;
                    res = dis;
                }
                break;
            case MInteractMode.POINT_EXPT:
                dis = mesh.GetClosetEdge(out edge, p);
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    e = edge;
                    res = dis;
                    break;
                }
                dis = mesh.GetClosetFace(out face, p, true, MDefinitions.ACTIVE_DISTANCE / scale);
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    e = face;
                    res = dis;
                    break;
                }
                break;
            case MInteractMode.EDGE_EXPT:
                dis = mesh.GetClosetPoint(out point, p);
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    e = point;
                    res = dis;
                    break;
                }
                dis = mesh.GetClosetFace(out face, p, true, MDefinitions.ACTIVE_DISTANCE / scale);
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    e = face;
                    res = dis;
                    break;
                }
                break;
            case MInteractMode.FACE_EXPT:
                dis = mesh.GetClosetPoint(out point, p);
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    e = point;
                    res = dis;
                    break;
                }
                dis = mesh.GetClosetEdge(out edge, p);
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    e = edge;
                    res = dis;
                    break;
                }
                break;
            default:
                Debug.Log("MObject: Response: unknown interact mode");
                break;
        }
        entity = e;
        return res;
    }

    public List<MObject> PlaneSplit(Vector3 normal, Vector3 planePoint)
    {
        normal = worldToLocalMatrix.MultiplyVector(normal).normalized;
        planePoint = worldToLocalMatrix.MultiplyPoint(planePoint);
        MMesh mesh1 = new MMesh();
        MMesh mesh2 = new MMesh();
        foreach(MPoint p in mesh.pointList)
        {
            int r = MHelperFunctions.FloatZero(Vector3.Dot(p.position - planePoint, normal));
            if(r == 0)
            {
                mesh1.CreatePoint(p.position);
                mesh2.CreatePoint(p.position);
            }
            else if(r > 0)
            {
                mesh1.CreatePoint(p.position);
            }
            else
            {
                mesh2.CreatePoint(p.position);
            }
        }
        Dictionary<MEdge, List<MEdge>> edgeMap = new Dictionary<MEdge, List<MEdge>>();
        List<MFace> facesNeedSplit = new List<MFace>();
        foreach(MEdge edge in mesh.edgeList)
        {
            switch (edge.edgeType)
            {
                case MEdge.MEdgeType.LINEAR:
                    {
                        MLinearEdge le = edge as MLinearEdge;
                        Vector3 v1 = le.start.position - MHelperFunctions.PointProjectionInFace(le.start.position, normal, planePoint);
                        Vector3 v2 = le.end.position - MHelperFunctions.PointProjectionInFace(le.end.position, normal, planePoint);
                        int r1 = MHelperFunctions.FloatZero(Vector3.Dot(v1, normal));
                        int r2 = MHelperFunctions.FloatZero(Vector3.Dot(v2, normal));
                        if (r1 == 0 && r2 == 0)
                        {
                            mesh1.CreateLinearEdge(new MPoint(le.start.position), new MPoint(le.end.position));
                            mesh2.CreateLinearEdge(new MPoint(le.start.position), new MPoint(le.end.position));
                        }
                        else if (r1 >= 0 && r2 >= 0)
                        {
                            mesh1.CreateLinearEdge(new MPoint(le.start.position), new MPoint(le.end.position));
                        }
                        else if (r1 <= 0 && r2 <= 0)
                        {
                            mesh2.CreateLinearEdge(new MPoint(le.start.position), new MPoint(le.end.position));
                        }
                        else
                        {
                            foreach (MFace face in le.faces)
                            {
                                if(!facesNeedSplit.Contains(face))facesNeedSplit.Add(face);
                            }
                            List<MEdge> edgeList = new List<MEdge>();
                            Vector3 v = (le.end.position - le.start.position).normalized * v1.magnitude / (v1.magnitude + v2.magnitude) + le.start.position;
                            if (r1 > 0)
                            {
                                edgeList.Add(mesh1.CreateLinearEdge(new MPoint(le.start.position), new MPoint(v)));
                                edgeList.Add(mesh2.CreateLinearEdge(new MPoint(v), new MPoint(le.end.position)));
                            }
                            else
                            {
                                edgeList.Add(mesh1.CreateLinearEdge(new MPoint(le.end.position), new MPoint(v)));
                                edgeList.Add(mesh2.CreateLinearEdge(new MPoint(v), new MPoint(le.start.position)));
                            }
                            edgeMap.Add(le, edgeList);
                        }
                        break;
                    }
                case MEdge.MEdgeType.CURVE:
                    {
                        MCurveEdge ce = edge as MCurveEdge;
                        int r = MHelperFunctions.FloatZero(Vector3.Dot(ce.center.position - planePoint, normal));
                        if (MHelperFunctions.Parallel(normal, ce.normal))
                        {
                            if (r == 0)
                            {
                                mesh1.CreateCurveEdge(ce.center, ce.radius, ce.normal);
                                mesh2.CreateCurveEdge(ce.center, ce.radius, ce.normal);
                            }
                            else if (r > 0)
                            {
                                mesh1.CreateCurveEdge(ce.center, ce.radius, ce.normal);
                            }
                            else
                            {
                                mesh2.CreateCurveEdge(ce.center, ce.radius, ce.normal);
                            }
                        }
                        else
                        {
                            float sin = Vector3.Cross(normal.normalized, ce.normal.normalized).magnitude;
                            float h = MHelperFunctions.DistanceP2F(ce.center.position, normal, planePoint);
                            float thresh = h / ce.radius;
                            if (sin <= thresh) //不相交
                            {
                                if (r > 0)
                                {
                                    mesh1.CreateCurveEdge(ce.center, ce.radius, ce.normal);
                                }
                                else
                                {
                                    mesh2.CreateCurveEdge(ce.center, ce.radius, ce.normal);
                                }
                            }
                            else //相交
                            {
                                foreach (MFace face in ce.faces)
                                {
                                    facesNeedSplit.Add(face);
                                }
                                Vector3 p;
                                Vector3 centerProjection = MHelperFunctions.PointProjectionInFace(ce.center.position, normal, planePoint);
                                if (!MHelperFunctions.Perpendicular(ce.normal, normal))
                                {
                                    
                                    Vector3 intersection = MHelperFunctions.IntersectionLineWithFace(ce.normal, ce.center.position, normal, planePoint);
                                    p = (centerProjection - intersection).normalized * (h * h / Vector3.Distance(centerProjection, intersection)) + centerProjection;
                                }
                                else
                                {
                                    p = centerProjection;
                                }
                                Vector3 n = Vector3.Cross(normal, ce.normal).normalized;
                                float l = Vector3.Distance(p, ce.center.position);
                                float d = Mathf.Sqrt(ce.radius * ce.radius - l * l);
                                Vector3 secp1 = p + n * d;
                                Vector3 secp2 = p - n * d;
                                List<MEdge> edgeList = new List<MEdge>();
                                List<Vector3> points1 = MHelperFunctions.GenerateArcPoint(secp1, secp2, ce.normal, ce.center.position);
                                List<Vector3> points2 = MHelperFunctions.GenerateArcPoint(secp2, secp1, ce.normal, ce.center.position);
                                int count1 = points1.Count;
                                int count2 = points2.Count;
                                Vector3 sample = count1 >= count2 ? points1[count1/2] : points2[count2/2];
                                r = MHelperFunctions.FloatZero(Vector3.Dot(sample, normal));
                                if((r > 0 && points1.Count >= points2.Count) || (r < 0 && points1.Count < points2.Count))
                                {
                                    edgeList.Add(mesh1.CreateGeneralEdge(points1));
                                    edgeList.Add(mesh2.CreateGeneralEdge(points2));
                                }
                                else
                                {
                                    edgeList.Add(mesh1.CreateGeneralEdge(points2));
                                    edgeList.Add(mesh2.CreateGeneralEdge(points1));
                                }
                                edgeMap.Add(ce, edgeList);
                            }
                        }
                        break;
                    }
                case MEdge.MEdgeType.GENERAL:
                    {
                        MGeneralEdge ge = edge as MGeneralEdge;
                        Vector3 p;
                        int r;
                        int topHalf = 0;
                        int count = ge.points.Count;
                        List<Vector3> points = new List<Vector3>();
                        for(int i = 0; i < count; i++)
                        {
                            p = ge.points[i];
                            r = MHelperFunctions.FloatZero(Vector3.Dot(p - planePoint, normal));
                            if(topHalf != 0)
                            {
                                if(r > 0 && topHalf < 0)
                                {
                                    Vector3 intersection = MHelperFunctions.IntersectionLineWithFace(p - ge.points[i - 1], p, normal, planePoint);
                                    points.Add(intersection);
                                    mesh2.CreateGeneralEdge(points);
                                    points = new List<Vector3>();
                                    points.Add(intersection);
                                    topHalf = 1;
                                } else if(r < 0 && topHalf > 0)
                                {
                                    Vector3 intersection = MHelperFunctions.IntersectionLineWithFace(p - ge.points[i - 1], p, normal, planePoint);
                                    points.Add(intersection);
                                    mesh1.CreateGeneralEdge(points);
                                    points = new List<Vector3>();
                                    points.Add(intersection);
                                    topHalf = -1;
                                }
                            }
                            else
                            {
                                topHalf = r;
                            }
                            points.Add(p);
                        }
                        if(points.Count > 1)
                        {
                            if(topHalf > 0)
                            {
                                mesh1.CreateGeneralEdge(points);
                            }
                            else
                            {
                                mesh2.CreateGeneralEdge(points);
                            }
                        }
                        break;
                    }
            }
        }
        List<List<Vector3>> splitPoints = new List<List<Vector3>>();
        Dictionary<Vector3, List<KeyValuePair<Vector3, int>>> splitGraph = new Dictionary<Vector3, List<KeyValuePair<Vector3, int>>>();
        foreach(MFace face in mesh.faceList)
        {
            switch (face.faceType)
            {
                case MFace.MFaceType.CIRCLE:
                    {
                        MCircleFace cirf = face as MCircleFace;
                        if (facesNeedSplit.Contains(cirf))
                        {
                            List<MEdge> edges;
                            if (!edgeMap.TryGetValue(cirf.circle, out edges)) break;
                            List<Vector3> points1 = new List<Vector3>(((MGeneralEdge)edges[0]).points);
                            List<Vector3> points2 = new List<Vector3>(((MGeneralEdge)edges[1]).points);
                            List<Vector3> split = new List<Vector3>();
                            split.Add(points1[0]);
                            split.Add(points2[0]);
                            int count = splitPoints.Count;
                            splitPoints.Add(split);
                            MHelperFunctions.AddValToDictionary(splitGraph, points1[0], points2[0], count);
                            MHelperFunctions.AddValToDictionary(splitGraph, points2[0], points1[0], count);
                            mesh1.CreateLinearEdge(new MPoint(points1[0]), new MPoint(points2[0]));
                            mesh2.CreateLinearEdge(new MPoint(points1[0]), new MPoint(points2[0]));
                            points1.Add(points1[0]);
                            points2.Add(points2[0]);
                            mesh1.CreateGeneralFlatFace(points1);
                            mesh2.CreateGeneralFlatFace(points2);
                        }
                        else
                        {
                            int r = MHelperFunctions.FloatZero(Vector3.Dot(cirf.circle.center.position - planePoint, normal));
                            if(r == 0)
                            {
                                mesh1.CreateCircleFace(new MCurveEdge(new MPoint(cirf.circle.center.position), cirf.circle.normal, cirf.circle.radius));
                                mesh2.CreateCircleFace(new MCurveEdge(new MPoint(cirf.circle.center.position), cirf.circle.normal, cirf.circle.radius));
                            } else if(r > 0)
                            {
                                mesh1.CreateCircleFace(new MCurveEdge(new MPoint(cirf.circle.center.position), cirf.circle.normal, cirf.circle.radius));
                            } else
                            {
                                mesh2.CreateCircleFace(new MCurveEdge(new MPoint(cirf.circle.center.position), cirf.circle.normal, cirf.circle.radius));
                            }
                        }
                        break;
                    }
                case MFace.MFaceType.CONE:
                case MFace.MFaceType.CYLINDER:
                case MFace.MFaceType.GENERAL_CURVE:
                    {
                        List<List<Vector3>> split;
                        List<Mesh> meshs = MHelperFunctions.MeshSplit(face.mesh, normal, planePoint, out split);
                        mesh1.CreateGeneralCurveFace(meshs[0]);
                        mesh2.CreateGeneralCurveFace(meshs[1]);
                        foreach(List<Vector3> l in split)
                        {
                            if(l.Count < 2)
                            {
                                Debug.Log("unexpected list count");
                                continue;
                            }
                            if(MHelperFunctions.BlurEqual(l[0], l[l.Count - 1]))
                            {
                                mesh1.CreateGeneralFlatFace(l);
                                mesh2.CreateGeneralFlatFace(l);
                            }
                            else
                            {
                                int count = splitPoints.Count;
                                splitPoints.Add(l);
                                MHelperFunctions.AddValToDictionary(splitGraph, l[0], l[l.Count - 1], count);
                                MHelperFunctions.AddValToDictionary(splitGraph, l[l.Count - 1],l[0], count);
                            }
                            if (MHelperFunctions.PointsInLine(l))
                            {
                                mesh1.CreateLinearEdge(new MPoint(l[0]), new MPoint(l[l.Count - 1]));
                                mesh2.CreateLinearEdge(new MPoint(l[0]), new MPoint(l[l.Count - 1]));
                            }
                            else
                            {
                                mesh1.CreateGeneralEdge(new List<Vector3>(l));
                                mesh2.CreateGeneralEdge(new List<Vector3>(l));
                            }
                        }
                        break;
                    }
                case MFace.MFaceType.SPHERE:
                    {
                        MSphereFace sf = face as MSphereFace;
                        List<List<Vector3>> split;
                        List<Mesh> meshs = MHelperFunctions.MeshSplit(face.mesh, normal, planePoint, out split);
                        mesh1.CreateGeneralCurveFace(meshs[0]);
                        mesh2.CreateGeneralCurveFace(meshs[1]);
                        Vector3 projCenter = MHelperFunctions.PointProjectionInFace(sf.center.position, normal, planePoint);
                        float dis = Vector3.Distance(sf.center.position, projCenter);
                        if(dis < sf.radius)
                        {
                            float r = Mathf.Sqrt(sf.radius * sf.radius - dis * dis);
                            MCurveEdge ce = mesh1.CreateCurveEdge(new MPoint(projCenter), r, normal);
                            mesh1.CreateCircleFace(ce);
                            ce = mesh2.CreateCurveEdge(new MPoint(projCenter), r, normal);
                            mesh2.CreateCircleFace(ce);
                        }
                        break;
                    }
                case MFace.MFaceType.POLYGON:
                    {
                        MPolygonFace polyf = face as MPolygonFace;
                        if (facesNeedSplit.Contains(polyf))
                        {
                            List<MEdge> edges;
                            List<MLinearEdge> edges1 = new List<MLinearEdge>();
                            List<MLinearEdge> edges2 = new List<MLinearEdge>();
                            List<Vector3> split = new List<Vector3>();
                            foreach(MLinearEdge edge in polyf.edgeList)
                            {
                                if(edgeMap.TryGetValue(edge, out edges))
                                {
                                    MLinearEdge le = edges[0] as MLinearEdge;
                                    edges1.Add(new MLinearEdge(new MPoint(le.start.position), new MPoint(le.end.position)));
                                    split.Add(le.end.position);
                                    le = edges[1] as MLinearEdge;
                                    edges2.Add(new MLinearEdge(new MPoint(le.start.position), new MPoint(le.end.position)));
                                }
                                else
                                {
                                    int r1 = MHelperFunctions.FloatZero(Vector3.Dot(edge.start.position - planePoint, normal));
                                    int r2 = MHelperFunctions.FloatZero(Vector3.Dot(edge.end.position - planePoint, normal));
                                    if (r1 == 0 && r2 == 0)
                                    {
                                        edges1.Add(new MLinearEdge(new MPoint(edge.start.position), new MPoint(edge.end.position)));
                                        edges2.Add(new MLinearEdge(new MPoint(edge.start.position), new MPoint(edge.end.position)));
                                    } else if(r1 >= 0 && r2 >= 0)
                                    {
                                        edges1.Add(new MLinearEdge(new MPoint(edge.start.position), new MPoint(edge.end.position)));
                                    } else if(r1 <= 0 && r2 <= 0)
                                    {
                                        edges2.Add(new MLinearEdge(new MPoint(edge.start.position), new MPoint(edge.end.position)));
                                    }
                                    else
                                    {
                                        Debug.Log("r1: " + r1 + ", r2: " + r2);
                                    }
                                }
                            }
                            if(split.Count != 2)
                            {
                                // TODO: 非凸多边形 被分割成了大于2个面
                                Debug.Log("unexpected split count");
                                continue;
                            }
                            else
                            {
                                edges1.Add(new MLinearEdge(new MPoint(split[0]), new MPoint(split[1])));
                                edges2.Add(new MLinearEdge(new MPoint(split[0]), new MPoint(split[1])));
                            }
                            int count = splitPoints.Count;
                            splitPoints.Add(split);
                            MHelperFunctions.AddValToDictionary(splitGraph, split[0], split[1], count);
                            MHelperFunctions.AddValToDictionary(splitGraph, split[1], split[0], count);
                            mesh1.CreatePolygonFace(edges1);
                            mesh2.CreatePolygonFace(edges2);
                        }
                        else
                        {
                            int r = MHelperFunctions.FloatZero(Vector3.Dot(polyf.edgeList[0].start.position - planePoint, normal));
                            if (r == 0)
                            {
                                List<MLinearEdge> edges1 = new List<MLinearEdge>();
                                List<MLinearEdge> edges2 = new List<MLinearEdge>();
                                foreach(MLinearEdge e in polyf.edgeList)
                                {
                                    edges1.Add(new MLinearEdge(new MPoint(e.start.position), new MPoint(e.end.position)));
                                    edges2.Add(new MLinearEdge(new MPoint(e.start.position), new MPoint(e.end.position)));
                                }
                                mesh1.CreatePolygonFace(edges1);
                                mesh2.CreatePolygonFace(edges2);
                            }
                            else
                            {
                                List<MLinearEdge> edges = new List<MLinearEdge>();
                                foreach (MLinearEdge e in polyf.edgeList)
                                {
                                    edges.Add(new MLinearEdge(new MPoint(e.start.position), new MPoint(e.end.position)));
                                }
                                if(r > 0)
                                {
                                    mesh1.CreatePolygonFace(edges);
                                }
                                else
                                {
                                    mesh2.CreatePolygonFace(edges);
                                }
                            }
                        }
                        break;
                    }
                case MFace.MFaceType.GENERAL_FLAT:
                    {
                        MGeneralFlatFace gff = face as MGeneralFlatFace;
                        List<Vector3> points1 = new List<Vector3>();
                        List<Vector3> points2 = new List<Vector3>();
                        List<Vector3> split = new List<Vector3>();
                        Vector3 p;
                        int lastr = 0;
                        int r;
                        int count = gff.points.Count;
                        for (int i = 0; i < count; i++)
                        {
                            p = gff.points[i];
                            r = MHelperFunctions.FloatZero(Vector3.Dot(p - planePoint, normal));
                            if(lastr >= 0 && r >= 0)
                            {
                                points1.Add(p);
                            } else if(lastr <= 0 && r <= 0)
                            {
                                points2.Add(p);
                            }
                            else
                            {
                                Vector3 intersect = MHelperFunctions.IntersectionLineWithFace(p - gff.points[i - 1], p, normal, planePoint);
                                split.Add(intersect);
                                points1.Add(intersect);
                                points2.Add(intersect);
                                if(r > 0)
                                {
                                    points1.Add(p);
                                }
                                else
                                {
                                    points2.Add(p);
                                }
                            }
                            lastr = r;
                        }
                        if (split.Count != 2)
                        {
                            // TODO: 非凸多边形 被分割成了大于2个面
                            Debug.Log("unexpected split count");
                            continue;
                        }
                        else
                        {
                            mesh1.CreateLinearEdge(new MPoint(split[0]), new MPoint(split[1]));
                            mesh2.CreateLinearEdge(new MPoint(split[0]), new MPoint(split[1]));
                        }
                        int cnt = splitPoints.Count;
                        splitPoints.Add(split);
                        MHelperFunctions.AddValToDictionary(splitGraph, split[0], split[1], cnt);
                        MHelperFunctions.AddValToDictionary(splitGraph, split[1], split[0], cnt);
                        mesh1.CreateGeneralFlatFace(points1);
                        mesh2.CreateGeneralFlatFace(points2);
                        break;
                    }
            }
        }
        List<List<Vector3>> loops = MHelperFunctions.GroupLoop(splitGraph, splitPoints);
        foreach(List<Vector3> list in loops)
        {
            mesh1.CreateGeneralFlatFace(list);
            mesh2.CreateGeneralFlatFace(list);
        }
        List<MObject> objects = new List<MObject>();
        if (!mesh1.IsEmpty())
        {
            MObject obj = new MObject(gameObject, mesh1);
            obj.transform.position = transform.position;
            obj.transform.rotation = transform.rotation;
            obj.transform.localScale = transform.localScale;
            objects.Add(obj);
        }
        if (!mesh2.IsEmpty())
        {
            MObject obj = new MObject(gameObject, mesh2);
            obj.transform.position = transform.position;
            obj.transform.rotation = transform.rotation;
            obj.transform.localScale = transform.localScale;
            objects.Add(obj);
        }
        return objects;
    }

    public void Highlight()
    {
        objectState = MObjectState.ACTIVE;
        mesh.Highlight();
    }

    public void ResetStatus()
    {
        objectState = MObjectState.DEFAULT;
        mesh.ResetStatus();
    }

    public void Select()
    {
        objectState = MObjectState.SELECT;
        mesh.Select();
    }
    
    public void Render()
    {
        mesh.Render(localToWorldMatrix);
    }

    public void Destroy()
    {
        UnityEngine.Object.Destroy(gameObject);
    }

    public void RemoveEntity(MEntity entity)
    {
        mesh.RemoveEntity(entity);
    }

    public void SetRefEdge(MLinearEdge edge)
    {
        refEdge = edge;
        if (refEdge != null) {
            refEdgeLength = refEdge.GetLength();
        }
    }

    public float GetEdgeLength(MEdge edge)
    {
        return RefEdgeRelativeLength(edge.GetLength());
    }

    public float GetFaceSurface(MFace face)
    {
        return RefEdgeRelativeSurface(face.GetSurface());
    }

    public void CreatePoint(Vector3 localSpacePos)
    {
        mesh.CreatePoint(localSpacePos);
    }

	public void CreateLinearEdge(MPoint start, MPoint end){
		MLinearEdge edge = mesh.CreateLinearEdge (start, end);
        if (edge != null && refEdge == null) SetRefEdge(edge);
	}

    public void CreatePolygonFace(List<MLinearEdge> edgeList)
    {
        mesh.CreatePolygonFace(edgeList);
    }

    public void SetMeshText(string text)
    {
        textMesh.text = text;
    }

    public void RotateTextMesh(Vector3 rotateTowards)
    {
        textPlane.parent = null;
        textPlane.rotation = Quaternion.LookRotation((rotateTowards - textPlane.position) * -1, Vector3.up) * Quaternion.Euler(-90, 0, 0);
        textPlane.parent = transform;
    }

    public void ActiveTextMesh()
    {
        textPlane.gameObject.SetActive(true);
        textPlane.parent = null;
        float maxY = Mathf.Max(localToWorldMatrix.MultiplyPoint(mesh.boundingBox.min).y, localToWorldMatrix.MultiplyPoint(mesh.boundingBox.max).y);
        textPlane.position = new Vector3(transform.position.x, maxY + MDefinitions.DEFAULT_TEXT_PLANE_HEIGHT, transform.position.z);
        textPlane.rotation = Quaternion.Euler(-90, 0, 0);
        textPlane.localScale = MDefinitions.DEFAULT_TEXT_PLANE_SCALE;
        textPlane.parent = transform;
    }

    public void InactiveTextMesh()
    {
        textPlane.gameObject.SetActive(false);
    }

    public float RefEdgeRelativeLength(float length)
    {
        if (refEdge != null) return length / refEdgeLength;
        return length;
    }
    
    public float RefEdgeRelativeSurface(float surface)
    {
        if (refEdge != null) return surface / refEdgeLength / refEdgeLength;
        return surface;
    }

    private void InitObject()
    {
        objectState = MObjectState.DEFAULT;
        transform = gameObject.transform;
        transform.position = MDefinitions.DEFAULT_POSITION;
        scale = MDefinitions.DEFAULT_SCALE;
        InitRefEdge();
        InitTextMesh();
    }

    private void InitTextMesh()
    {
        textPlane = gameObject.transform.GetChild(0);
        textMesh = textPlane.GetComponentInChildren<TextMesh>(true);
    }

    private void InitRefEdge()
    {
        refEdge = mesh.GetLinearEdge();
        if (refEdge != null) {
            refEdgeLength = refEdge.GetLength();
        }
    }
}
