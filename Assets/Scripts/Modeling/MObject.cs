using System.Collections;
using System.Collections.Generic;
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
    public MObject(string path)
    {
        // TODO: 针对导入模型的初始化，生成新的GameObject，挂载MeshFilter，初始化MMesh和包围盒
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
        transform = gameObject.transform;
        transform.position = MDefinitions.DEFAULT_POSITION;
        scale = MDefinitions.DEFAULT_SCALE;
        InitRefEdge();
        InitTextMesh();
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
        mesh.Highlight();
    }

    public void ResetStatus()
    {
        mesh.ResetStatus();
    }

    public void Select()
    {
        mesh.Select();
    }
    
    public void Render()
    {
        mesh.Render(localToWorldMatrix);
    }

    public void Destroy()
    {
        Object.Destroy(gameObject);
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
        if(refEdge != null)return edge.GetLength() / refEdgeLength;
        return edge.GetLength();
    }

    public float GetFaceSurface(MFace face)
    {
        if (refEdge != null)return face.GetSurface() / refEdgeLength / refEdgeLength;
        return face.GetSurface();
    }

	public void CreateLinearEdge(MPoint start, MPoint end){
		mesh.CreateLinearEdge (start, end);
	}

    public MRelation getEntityRelation(MEntity e1, MEntity e2)
    {
        // TODO: 根据MEntity的不同类型来生成MRelation类，对于线和面只用考虑直线和多边形面。
        // 求距离时将线段视为直线，将多边形面视为无边界的平面
        // 注意MRelation中的distance是在世界坐标系下的距离，需要根据基准边做变换，具体参考GetEdgeLength。
        return null;
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

    private void HitPoint(MPoint point)
    {
        Debug.Log("hit point " + localToWorldMatrix.MultiplyPoint(point.position));
    }

    private void HitEdge(MEdge edge)
    {
        switch (edge.edgeType)
        {
            case MEdge.MEdgeType.LINEAR:
                Debug.Log("hit edge " + localToWorldMatrix.MultiplyPoint(((MLinearEdge)edge).start.position) 
                    + " " + localToWorldMatrix.MultiplyPoint(((MLinearEdge)edge).end.position));
                break;
            case MEdge.MEdgeType.CURVE:
                Debug.Log("hit edge " + localToWorldMatrix.MultiplyPoint(((MCurveEdge)edge).center.position)
                    + " " + localToWorldMatrix.MultiplyPoint(((MCurveEdge)edge).normal) + " " + ((MCurveEdge)edge).radius);
                break;
            default:
                Debug.Log("hit unknown edge of type " + edge.edgeType);
                break;
        }
    }

    private void HitFace(MFace face)
    {
        switch (face.faceType)
        {
            case MFace.MFaceType.POLYGON:
                Debug.Log("hit face " + localToWorldMatrix.MultiplyPoint(((MPolygonFace)face).edgeList[0].start.position));
                break;
            case MFace.MFaceType.CIRCLE:
            case MFace.MFaceType.CONE:
            case MFace.MFaceType.CYLINDER:
            case MFace.MFaceType.SPHERE:
            default:
                Debug.Log("hit unknown face of type " + face.faceType);
                break;
        }
    }
}
