using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class MObject
{
    public enum MPrefabType { CUBE, SPHERE, CYLINDER, PRISM, PYRAMID, CONE, SQUARE};

    public enum MInteractMode { ALL, POINT_ONLY, EDGE_ONLY, FACE_ONLY};

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
    public MObject(GameObject template, string path)
    {
        gameObject = GameObject.Instantiate(template);
        mesh = new MMesh();
        using(StreamReader sr = new StreamReader(path + "/obj"))
        {
            string line = sr.ReadLine();
            while (line!=null)
            {
                string[] lineComponent = line.Split(' ');
                char type = Convert.ToChar(lineComponent[0]);
                switch (type)
                {
                    case 'p':
                        float x = (float)Convert.ToDouble(lineComponent[1]);
                        float y = (float)Convert.ToDouble(lineComponent[2]);
                        float z = (float)Convert.ToDouble(lineComponent[3]);
                        Vector3 position = new Vector3(x, y, z);
                        MPoint p = mesh.CreatePoint(position);
                        break;
                    case 'e':
                        char edgeType = Convert.ToChar(lineComponent[1]);
                        switch (edgeType)
                        {
                            case 'l':
                                int start = Convert.ToInt32(lineComponent[2]);
                                int end = Convert.ToInt32(lineComponent[3]);
                                List<MPoint> pointList = mesh.pointList;
                                MLinearEdge edge = mesh.CreateLinearEdge(pointList[start], pointList[end]);
                                break;
                            case 'c':
                                break;
                        }
                        break;
                    case 'f':
                        char faceType = Convert.ToChar(lineComponent[1]);
                        switch (faceType)
                        {
                            case 'p':
                                List<MLinearEdge> edgeList = new List<MLinearEdge>();
                                Debug.Log(lineComponent.Length);
                                for (int i = 2; i < lineComponent.Length-1; i++)
                                {
                                    int index = Convert.ToInt32(lineComponent[i]);
                                    edgeList.Add((MLinearEdge)mesh.edgeList[index]);
                                }
                                MPolygonFace face = mesh.CreatePolygonFace(edgeList);
                                break;
                        }
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
            default:
                Debug.Log("Unknown prefab type: " + type);
                return;
        }
        InitObject();
    }

    public bool ExportObject(string path)
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
                    sb.Append(string.Format("e l {0} {1}\n", start, end));
                    break;
                case MEdge.MEdgeType.CURVE:
                    break;
            }
        }
        foreach(MFace face in faceList)
        {
            switch (face.faceType)
            {
                case MFace.MFaceType.POLYGON:
                    sb.Append("f p ");
                    foreach(MLinearEdge edge in ((MPolygonFace)face).edgeList)
                    {
                        int index = edgeList.IndexOf(edge);
                        sb.Append(string.Format("{0} ", index));
                    }
                    sb.Append("\n");
                    break;
                case MFace.MFaceType.CIRCLE:
                    break;
                case MFace.MFaceType.CONE:
                    break;
                case MFace.MFaceType.CYLINDER:
                    break;
                case MFace.MFaceType.SPHERE:
                    break;
            }
        }
        using (StreamWriter sw = new StreamWriter(path+"/obj"))
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
                    //HitPoint(point);
                    e = point;
                    res = dis;
                    break;
                }
                dis = mesh.GetClosetEdge(out edge, p);
                if(dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    //HitEdge(edge);
                    e = edge;
                    res = dis;
                    break;
                }
                dis = mesh.GetClosetFace(out face, p, true, MDefinitions.ACTIVE_DISTANCE);
                if(dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    //HitFace(face);
                    e = face;
                    res = dis;
                    break;
                }
                break;
            case MInteractMode.POINT_ONLY:
                dis = mesh.GetClosetPoint(out point, p);
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    //HitPoint(point);
                    e = point;
                    res = dis;
                }
                break;
            case MInteractMode.EDGE_ONLY:
                dis = mesh.GetClosetEdge(out edge, p);
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    //HitEdge(edge);
                    e = edge;
                    res = dis;
                }
                break;
            case MInteractMode.FACE_ONLY:
                dis = mesh.GetClosetFace(out face, p, true, MDefinitions.ACTIVE_DISTANCE / scale);
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    //HitFace(face);
                    e = face;
                    res = dis;
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
