using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class MObject
{
    public enum MPrefabType { CUBE, SPHERE, CYLINDER, PRISM, PYRAMID, CONE};

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
        using(StreamReader sr = new StreamReader(MDefinitions.PATH + "/" + filename))
        {
            string line = sr.ReadLine();
            while (line!=null)
            {
                string[] lineComponent = line.Split(' ');
                string type = lineComponent[0];
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
                        int count = (lineComponent.Length - 2) / 3;
                        List<Vector3> points = new List<Vector3>();
                        for(int i = 0; i < count; i++)
                        {
                            points.Add(new Vector3(
                                (float)Convert.ToDouble(lineComponent[3 * i + 1]),
                                (float)Convert.ToDouble(lineComponent[3 * i + 2]),
                                (float)Convert.ToDouble(lineComponent[3 * i + 3])));
                        }
                        mesh.CreateGeneralEdge(points);
                        break;
                    case "polyf":
                        List<MLinearEdge> edgeList = new List<MLinearEdge>();
                        for (int i = 1; i < lineComponent.Length-1; i++)
                        {
                            int index = Convert.ToInt32(lineComponent[i]);
                            edgeList.Add((MLinearEdge)mesh.edgeList[index]);
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
                }
                line = sr.ReadLine();
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
            default:
                Debug.Log("Unknown prefab type: " + type);
                return;
        }
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
                    sb.Append("ge ");
                    foreach(Vector3 v in ge.points)
                    {
                        sb.Append(string.Format("{0} {1} {2} ", v.x, v.y, v.z));
                    }
                    sb.Append("\n");
                    break;
            }
        }
        foreach(MFace face in faceList)
        {
            switch (face.faceType)
            {
                case MFace.MFaceType.POLYGON:
                    sb.Append("polyf ");
                    foreach(MLinearEdge edge in ((MPolygonFace)face).edgeList)
                    {
                        int index = edgeList.IndexOf(edge);
                        sb.Append(string.Format("{0} ", index));
                    }
                    sb.Append("\n");
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
            }
        }
        if (!Directory.Exists(MDefinitions.PATH))
        {
            Directory.CreateDirectory(MDefinitions.PATH);
        }
        using (StreamWriter sw = new StreamWriter(MDefinitions.PATH+"/" + filename))
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
		mesh.CreateLinearEdge (start, end);
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
