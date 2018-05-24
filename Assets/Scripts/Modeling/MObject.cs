﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MObject
{
    public enum MPrefabType { CUBE, SPHERE, CYLINDER, PRISM, PYRAMID, CONE, SQUARE};

    public enum MInteractMode { ALL, POINT_ONLY, EDGE_ONLY, FACE_ONLY};

    private Transform transform;

    private GameObject gameObject;

    private MMesh mesh;

    private MLinearEdge refEdge = null;

    private float refEdgeLength;

    private Matrix4x4 worldToLocalMatrix
    {
        get { return transform.worldToLocalMatrix; }
    }

    private Matrix4x4 localToWorldMatrix
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
    public MObject(MPrefabType type)
    {
        gameObject = new GameObject();
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
                dis = mesh.GetClosetPoint(out point, p) * scale;
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    //HitPoint(point);
                    e = point;
                    res = dis;
                }
                break;
            case MInteractMode.EDGE_ONLY:
                dis = mesh.GetClosetEdge(out edge, p) * scale;
                if (dis < MDefinitions.ACTIVE_DISTANCE)
                {
                    //HitEdge(edge);
                    e = edge;
                    res = dis;
                }
                break;
            case MInteractMode.FACE_ONLY:
                dis = mesh.GetClosetFace(out face, p, true, MDefinitions.ACTIVE_DISTANCE / scale) * scale;
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
        if(refEdge != null)
        {
            refEdge.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
        refEdge = edge;
        if (refEdge != null) {
            refEdgeLength = refEdge.GetLength();
            refEdge.entityStatus = MEntity.MEntityStatus.SPECIAL;
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

    private void InitRefEdge()
    {
        refEdge = mesh.GetLinearEdge();
        if (refEdge != null) {
            refEdgeLength = refEdge.GetLength();
            refEdge.entityStatus = MEntity.MEntityStatus.SPECIAL;
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
