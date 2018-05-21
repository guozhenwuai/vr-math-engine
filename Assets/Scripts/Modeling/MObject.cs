using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MObject
{
    public enum MPrefabType { CUBE, SPHERE, CYLINDER, PRISM, PYRAMID, CONE, SQUARE};

    public enum MInteractMode { ALL, POINT_ONLY, EDGE_ONLY, FACE_ONLY};

    public Transform transform;

    private GameObject gameObject;

    private MMesh mesh;

    private Matrix4x4 worldToLocalMatrix
    {
        get { return transform.worldToLocalMatrix; }
    }

    private Matrix4x4 localToWorldMatrix
    {
        get { return transform.localToWorldMatrix; }
    }

    private float scale
    {
        get { return transform.lossyScale.x; }
    }

    // 针对导入模型的初始化
    public MObject(Mesh mesh)
    {
        // TODO: 针对导入模型的初始化，生成新的GameObject，挂载MeshFilter，初始化MMesh和包围盒
    }

    // 针对预制件的初始化
    public MObject(MPrefabType type)
    {
        gameObject = new GameObject();
        switch (type)
        {
            case MPrefabType.CUBE:
                gameObject.AddComponent<MeshFilter>().mesh = MCube.GetMesh();
                mesh = MCube.GetMMesh();
                break;
            default:
                Debug.Log("Unknown prefab type: " + type);
                return;
        }
        transform = gameObject.transform;
        gameObject.AddComponent<MeshRenderer>();
    }

    public bool HitObject(Vector3 pos)
    {
        Vector3 p = worldToLocalMatrix.MultiplyPoint(pos);
        Debug.Log("world pos: " + pos + ", object pos: " + p);
        if (mesh.boundingBox.Contains(p))
        {
            Debug.Log("hit object");
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
                dis = mesh.GetClosetPoint(out point, p) * scale;
                if(dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    HitPoint(point);
                    e = point;
                    res = dis;
                    break;
                }
                dis = mesh.GetClosetEdge(out edge, p) * scale;
                if(dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    HitEdge(edge);
                    e = edge;
                    res = dis;
                    break;
                }
                dis = mesh.GetClosetFace(out face, p, true, MDefinitions.ACTIVE_DISTANCE / scale) * scale;
                if(dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    HitFace(face);
                    e = face;
                    res = dis;
                    break;
                }
                break;
            case MInteractMode.POINT_ONLY:
                dis = mesh.GetClosetPoint(out point, p) * scale;
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    HitPoint(point);
                    e = point;
                    res = dis;
                }
                break;
            case MInteractMode.EDGE_ONLY:
                dis = mesh.GetClosetEdge(out edge, p) * scale;
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    HitEdge(edge);
                    e = edge;
                    res = dis;
                }
                break;
            case MInteractMode.FACE_ONLY:
                dis = mesh.GetClosetFace(out face, p, true, MDefinitions.ACTIVE_DISTANCE / scale) * scale;
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    HitFace(face);
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
    
    public void Destroy()
    {
        Object.Destroy(gameObject);
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
