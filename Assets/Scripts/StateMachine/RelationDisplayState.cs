using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelationDisplayState : IState
{
    private SceneManager sceneManager;

    private GameObject textMesh;

    private Text[] textList;

    private List<MEntityPair> selectedEntity;

    private List<string> texts;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    public RelationDisplayState(SceneManager sceneManager, GameObject textMesh)
    {
        this.sceneManager = sceneManager;
        this.textMesh = textMesh;
        textList = textMesh.GetComponentsInChildren<Text>();
        selectedEntity = new List<MEntityPair>();
        texts = new List<string>();
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.RELATION_DISPLAY;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        textMesh.SetActive(true);
        texts.Clear();
        UpdateTextMesh();
		foreach (MObject obj in sceneManager.objects)
		{
			MLinearEdge refEdge = obj.refEdge;
			if (refEdge != null)
			{
				refEdge.entityStatus = MEntity.MEntityStatus.SPECIAL;
			}
		}
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        ClearSelectedEntity();
        textMesh.SetActive(false);
		foreach (MObject obj in sceneManager.objects)
		{
			MLinearEdge refEdge = obj.refEdge;
			if (refEdge != null)
			{
				refEdge.entityStatus = MEntity.MEntityStatus.DEFAULT;
			}
		}
    }

    public void OnUpdate()
    {
        sceneManager.UpdateEntityHighlight(MObject.MInteractMode.ALL);
        if (sceneManager.camera != null) UpdateTextTransform();
        sceneManager.StartRender();
    }

    private void UpdateTextMesh()
    {
        int i = 0;
        for(;i < texts.Count && i < textList.Length; i++)
        {
            textList[i].gameObject.SetActive(true);
            textList[i].text = texts[i];
        }
        for(; i < textList.Length; i++)
        {
            textList[i].gameObject.SetActive(false);
        }
    }

    private void UpdateTextTransform()
    {
        textMesh.transform.rotation = Quaternion.LookRotation((sceneManager.camera.transform.position - textMesh.transform.position) * -1, Vector3.up) * Quaternion.Euler(0, 180, 0);
    }

    private void UpdateText()
    {
        string text = "";
        texts.Clear();
        if(selectedEntity.Count == 2)
        {
            MRelation relation = GetEntityRelation(selectedEntity[0], selectedEntity[1]);
            if (relation != null)
            {
                string ls, hs;
                switch (relation.relationType)
                {
                    case MRelation.EntityRelationType.POINT_POINT:
                        if (relation.shareObject)
                        {
                            MObject obj = relation.lowerEntity.obj;
                            ls = obj.GetPointIdentifier((MPoint)(relation.lowerEntity.entity));
                            hs = obj.GetPointIdentifier((MPoint)(relation.higherEntity.entity));
                            if(ls != null && hs != null)
                            {
                                text = ls + hs + " = " + relation.distance;
                                texts.Add(text);
                            }
                            else
                            {
                                text = "两点距离：" + relation.distance;
                                texts.Add(text);
                            }
                            
                        }
                        break;
                    case MRelation.EntityRelationType.POINT_EDGE:
                        if (relation.shareObject)
                        {
                            MObject obj = relation.lowerEntity.obj;
                            ls = obj.GetPointIdentifier((MPoint)(relation.lowerEntity.entity));
                            hs = GetEdgeString(relation.higherEntity.entity as MLinearEdge, obj);
                            if (MHelperFunctions.FloatEqual(relation.distance, 0))
                            {
                                if(ls != null && hs != null)
                                {
                                    text = ls + " ∈ " + hs;
                                    texts.Add(text);
                                }
                                else
                                {
                                    text = "点在直线上";
                                    texts.Add(text);
                                }
                            }
                            else
                            {
                                if (ls != null && hs != null)
                                {
                                    text = "Dis(" + ls + ", " + hs + ") = " + relation.distance;
                                    texts.Add(text);
                                }
                                else
                                {
                                    text = "点到直线距离： " + relation.distance;
                                    texts.Add(text);
                                }
                                    
                            }
                        }
                        break;
                    case MRelation.EntityRelationType.POINT_FACE:
                        ls = null;
                        hs = null;
                        if (relation.shareObject)
                        {
                            MObject obj = relation.lowerEntity.obj;
                            ls = obj.GetPointIdentifier((MPoint)(relation.lowerEntity.entity));
                            hs = GetFaceString(relation.higherEntity.entity as MFace, obj);
                            if (MHelperFunctions.FloatEqual(relation.distance, 0))
                            {
                                if(ls != null && hs != null)
                                {
                                    text = ls + " ∈ " + hs;
                                    texts.Add(text);
                                }
                                else
                                {
                                    text = "点在平面上";
                                    texts.Add(text);
                                }
                            }
                            else
                            {
                                if(ls != null && hs != null)
                                {
                                    text = "Dis(" + ls + ", " + hs + ") = " + relation.distance;
                                    texts.Add(text);
                                }
                                else
                                {
                                    text = "点到平面距离： " + relation.distance;
                                    texts.Add(text);
                                }
                            }
                        }
                        break;
                    case MRelation.EntityRelationType.EDGE_EDGE:
                        hs = null;
                        ls = null;
                        if (relation.shareObject)
                        {
                            MObject obj = relation.lowerEntity.obj;
                            ls = GetEdgeString(relation.lowerEntity.entity as MLinearEdge, obj);
                            hs = GetEdgeString(relation.higherEntity.entity as MLinearEdge, obj);
                        }
                        if ((relation.shareObject && MHelperFunctions.FloatEqual(relation.distance, 0)) || MHelperFunctions.FloatEqual(relation.angle, 0))
                        {
                            if(ls != null && hs != null)
                            {
                                text = ls + "、" + hs + "共面";
                                texts.Add(text);
                            }
                            else
                            {
                                text = "两线共面";
                                texts.Add(text);
                            }
                        }
                        if (MHelperFunctions.FloatEqual(relation.angle, 0))
                        {
                            if(relation.shareObject && ls != null && hs != null)
                            {
                                text = ls + " // " + hs;
                                texts.Add(text);
                            }
                            else
                            {
                                text = "两线平行";
                                texts.Add(text);
                            }
                        }
                        else if (MHelperFunctions.FloatEqual(relation.angle, 90))
                        {
                            if (relation.shareObject && ls != null && hs != null)
                            {
                                text = ls + " ⊥ " + hs;
                                texts.Add(text);
                            }
                            else
                            {
                                text = "两线垂直";
                                texts.Add(text);
                            }
                        }
                        else
                        {
                            if (relation.shareObject && ls != null && hs != null)
                            {
                                text = ls + "、" + hs + "夹角： " + relation.angle;
                                texts.Add(text);
                            }
                            else
                            {
                                text = "两线夹角：" + relation.angle;
                                texts.Add(text);
                            }
                        }
                        if (relation.shareObject && !MHelperFunctions.FloatEqual(relation.distance, 0))
                        {
                            if (ls != null && hs != null)
                            {
                                text = "Dis(" + ls + ", " + hs + ") = " + relation.distance;
                                texts.Add(text);
                            }
                            else
                            {
                                text = "两线距离： " + relation.distance;
                                texts.Add(text);
                            }
                        }
                        break;
                    case MRelation.EntityRelationType.EDGE_FACE:
                        hs = null;
                        ls = null;
                        if (relation.shareObject)
                        {
                            MObject obj = relation.lowerEntity.obj;
                            ls = GetEdgeString(relation.lowerEntity.entity as MLinearEdge, obj);
                            hs = GetFaceString(relation.higherEntity.entity as MFace, obj);
                        }
                        if (relation.shareObject && MHelperFunctions.FloatEqual(relation.distance, 0))
                        {
                            if (ls != null && hs != null)
                            {
                                text = ls + " ⊂ " + hs;
                                texts.Add(text);
                            }
                            else
                            {
                                text = "线在面上";
                                texts.Add(text);
                            }
                        }
                        else if (relation.shareObject && MHelperFunctions.FloatEqual(relation.distance, -1))
                        {
                            if (ls != null && hs != null)
                            {
                                text = ls + "、" + hs + "相交";
                                texts.Add(text);
                            }
                            else
                            {
                                text = "线面相交";
                                texts.Add(text);
                            }
                        }
                        if (MHelperFunctions.FloatEqual(relation.angle, 0))
                        {
                            if(relation.shareObject && ls != null && hs != null)
                            {
                                text = ls + " // " + hs;
                                texts.Add(text);
                            }
                            else
                            {
                                text = "线面平行";
                                texts.Add(text);
                            }
                            if (relation.shareObject)
                            {
                                if (ls != null && hs != null)
                                {
                                    text = "Dis(" + ls + ", " + hs + ") = " + relation.distance;
                                    texts.Add(text);
                                }
                                else
                                {
                                    text = "线面距离：" + relation.distance;
                                    texts.Add(text);
                                }
                            }
                        }
                        else if (MHelperFunctions.FloatEqual(relation.angle, 90))
                        {
                            if (relation.shareObject && ls != null && hs != null)
                            {
                                text = ls + " ⊥ " + hs;
                                texts.Add(text);
                            }
                            else
                            {
                                text = "线面垂直";
                                texts.Add(text);
                            }
                        }
                        else
                        {
                            if (relation.shareObject && ls != null && hs != null)
                            {
                                text = ls + "、" + hs + "夹角： " + relation.angle;
                                texts.Add(text);
                            }
                            else
                            {
                                text = "线面夹角：" + relation.angle;
                                texts.Add(text);
                            }
                        }
                        break;
                    case MRelation.EntityRelationType.FACE_FACE:
                        hs = null;
                        ls = null;
                        if (relation.shareObject)
                        {
                            MObject obj = relation.lowerEntity.obj;
                            ls = GetFaceString(relation.lowerEntity.entity as MFace, obj);
                            hs = GetFaceString(relation.higherEntity.entity as MFace, obj);
                        }
                        if (MHelperFunctions.FloatEqual(relation.angle, 0))
                        {
                            if (relation.shareObject && ls != null && hs != null)
                            {
                                text = ls + " // " + hs;
                                texts.Add(text);
                            }
                            else
                            {
                                text = "面面平行";
                                texts.Add(text);
                            }
                            if (relation.shareObject)
                            {
                                if (ls != null && hs != null)
                                {
                                    text = "Dis(" + ls + ", " + hs + ") = " + relation.distance;
                                    texts.Add(text);
                                }
                                else
                                {
                                    text = "面面距离：" + relation.distance;
                                    texts.Add(text);
                                }
                            }
                        }
                        else if (MHelperFunctions.FloatEqual(relation.angle, 90))
                        {
                            if (relation.shareObject && ls != null && hs != null)
                            {
                                text = ls + " ⊥ " + hs;
                                texts.Add(text);
                            }
                            else
                            {
                                text = "面面垂直";
                                texts.Add(text);
                            }
                        }
                        else
                        {
                            if (relation.shareObject && ls != null && hs != null)
                            {
                                text = ls + "、" + hs + "夹角： " + relation.angle;
                                texts.Add(text);
                            }
                            else
                            {
                                text = "面面夹角：" + relation.angle;
                                texts.Add(text);
                            }
                        }
                        break;
                }
            }
        }
        UpdateTextMesh();
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (sceneManager.activeEntity.entity != null)
        {
            SelectEntity(sceneManager.activeEntity);
            UpdateText();
        }
    }

    private void SelectEntity(MEntityPair entityPair)
    {
        if (selectedEntity.Contains(entityPair))
        {
            RemoveEntity(entityPair);
        }
        else
        {
            if(selectedEntity.Count == 2)
            {
                RemoveEntity(selectedEntity[0]);
            }
            AddEntity(entityPair);
        }
    }

    private void RemoveEntity(MEntityPair entityPair)
    {
        MEntity entity = entityPair.entity;
        if (entity.entityStatus == MEntity.MEntityStatus.SELECT)
        {
            entity.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
        selectedEntity.Remove(entityPair);
    }

    private void AddEntity(MEntityPair entityPair)
    {
        MEntity entity = entityPair.entity;
        if (entity.entityStatus != MEntity.MEntityStatus.SPECIAL)
        {
            entity.entityStatus = MEntity.MEntityStatus.SELECT;
        }
        selectedEntity.Add(entityPair);
    }

    private void ClearSelectedEntity()
    {
        foreach (MEntityPair pair in selectedEntity)
        {
            pair.entity.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
        selectedEntity.Clear();
    }

    private MRelation GetEntityRelation(MEntityPair e1, MEntityPair e2)
    {
        MEntityPair lowerEntity, higherEntity;
        CompareEntity(e1, e2, out lowerEntity, out higherEntity);
        bool shareObject = (e1.obj == e2.obj);
        MObject obj = null;
        if (shareObject) obj = e1.obj;
        float distance = -1;
        float angle = 0;
        MRelation.EntityRelationType type;
        if(higherEntity.entity.entityType == MEntity.MEntityType.POINT) //点点关系
        {
            type = MRelation.EntityRelationType.POINT_POINT;
            if (shareObject)
            {
                Vector3 p1 = ((MPoint)lowerEntity.entity).position;
                Vector3 p2 = ((MPoint)higherEntity.entity).position;
                distance = obj.RefEdgeRelativeLength(Vector3.Distance(p1, p2));
            }
        }
        else if(lowerEntity.entity.entityType == MEntity.MEntityType.POINT && higherEntity.entity.entityType == MEntity.MEntityType.EDGE) //点线关系
        {
            type = MRelation.EntityRelationType.POINT_EDGE;
            if (shareObject)
            {
                MEdge edge = (MEdge)higherEntity.entity;
                if (edge.edgeType != MEdge.MEdgeType.LINEAR) return null;
                distance = obj.RefEdgeRelativeLength(MHelperFunctions.DistanceP2L(
                    ((MPoint)lowerEntity.entity).position, 
                    ((MLinearEdge)edge).direction, 
                    ((MLinearEdge)edge).end.position));
            }
        }
        else if(higherEntity.entity.entityType == MEntity.MEntityType.EDGE) //线线关系
        {
            type = MRelation.EntityRelationType.EDGE_EDGE;
            MEdge edge1 = (MEdge)lowerEntity.entity;
            MEdge edge2 = (MEdge)higherEntity.entity;
            if (edge1.edgeType != MEdge.MEdgeType.LINEAR || edge2.edgeType != MEdge.MEdgeType.LINEAR) return null;
            MLinearEdge le1 = edge1 as MLinearEdge;
            MLinearEdge le2 = edge2 as MLinearEdge;
            if (shareObject)
            {
                distance = obj.RefEdgeRelativeLength(MHelperFunctions.DistanceL2L(le1.start.position, le1.direction, le2.start.position, le2.direction));
                angle = MHelperFunctions.CalcAngle(le1.direction, le2.direction);
            }
            else
            {
                angle = MHelperFunctions.CalcAngle(
                    lowerEntity.obj.localToWorldMatrix.MultiplyVector(le1.direction), 
                    higherEntity.obj.localToWorldMatrix.MultiplyVector(le2.direction));
            }
        }
        else if(lowerEntity.entity.entityType == MEntity.MEntityType.POINT) //点面关系
        {
            type = MRelation.EntityRelationType.POINT_FACE;
            MFace face = (MFace)higherEntity.entity;
            Vector3 normal;
            Vector3 facePoint;
            if (shareObject)
            {
                if (face.faceType == MFace.MFaceType.POLYGON)
                {
                    normal = ((MPolygonFace)face).normal;
                    facePoint = ((MPolygonFace)face).edgeList[0].start.position;
                }
                else if (face.faceType == MFace.MFaceType.CIRCLE)
                {
                    normal = ((MCircleFace)face).circle.normal;
                    facePoint = ((MCircleFace)face).circle.center.position;
                }
                else
                {
                    return null;
                }
                distance = obj.RefEdgeRelativeLength(MHelperFunctions.DistanceP2F(((MPoint)lowerEntity.entity).position, normal, facePoint));
            }
        }
        else if(lowerEntity.entity.entityType == MEntity.MEntityType.EDGE) //线面关系
        {
            type = MRelation.EntityRelationType.EDGE_FACE;
            MEdge edge = (MEdge)lowerEntity.entity;
            if (edge.edgeType != MEdge.MEdgeType.LINEAR) return null;
            MFace face = (MFace)higherEntity.entity;
            Vector3 normal;
            Vector3 facePoint;
            if (face.faceType == MFace.MFaceType.POLYGON)
            {
                normal = ((MPolygonFace)face).normal;
                facePoint = ((MPolygonFace)face).edgeList[0].start.position;
            }
            else if (face.faceType == MFace.MFaceType.CIRCLE)
            {
                normal = ((MCircleFace)face).circle.normal;
                facePoint = ((MCircleFace)face).circle.center.position;
            }
            else
            {
                return null;
            }
            if (shareObject)
            {
                angle = 90 - MHelperFunctions.CalcAngle(((MLinearEdge)edge).direction, normal);
                if(MHelperFunctions.FloatEqual(angle, 0))
                {
					distance = obj.RefEdgeRelativeLength(MHelperFunctions.DistanceP2F(((MLinearEdge)edge).start.position, normal, facePoint));
                }
            }
            else
            {
                angle = 90 - MHelperFunctions.CalcAngle(
                    lowerEntity.obj.localToWorldMatrix.MultiplyVector(((MLinearEdge)edge).direction), 
                    higherEntity.obj.localToWorldMatrix.MultiplyVector(normal));
            }
        }
        else //面面关系
        {
            type = MRelation.EntityRelationType.FACE_FACE;
            MFace f1 = (MFace)lowerEntity.entity;
            MFace f2 = (MFace)higherEntity.entity;
            Vector3 normal1, normal2;
            Vector3 facePoint1, facePoint2;
            if (f1.faceType == MFace.MFaceType.POLYGON)
            {
                normal1 = ((MPolygonFace)f1).normal;
                facePoint1 = ((MPolygonFace)f1).edgeList[0].start.position;
            }
            else if (f1.faceType == MFace.MFaceType.CIRCLE)
            {
                normal1 = ((MCircleFace)f1).circle.normal;
                facePoint1 = ((MCircleFace)f1).circle.center.position;
            }
            else
            {
                return null;
            }
            if (f2.faceType == MFace.MFaceType.POLYGON)
            {
                normal2 = ((MPolygonFace)f2).normal;
                facePoint2 = ((MPolygonFace)f2).edgeList[0].start.position;
            }
            else if (f2.faceType == MFace.MFaceType.CIRCLE)
            {
                normal2 = ((MCircleFace)f2).circle.normal;
                facePoint2 = ((MCircleFace)f2).circle.center.position;
            }
            else
            {
                return null;
            }
            if (shareObject)
            {
                angle = MHelperFunctions.CalcAngle(normal1, normal2);
                if (MHelperFunctions.FloatEqual(angle, 0))
                {
					distance = obj.RefEdgeRelativeLength(MHelperFunctions.DistanceP2F(facePoint1, normal2, facePoint2));
                }
            }
            else
            {
                angle = MHelperFunctions.CalcAngle(
                    lowerEntity.obj.localToWorldMatrix.MultiplyVector(normal1),
                    higherEntity.obj.localToWorldMatrix.MultiplyVector(normal2));
            }
        }
        return new MRelation(type, lowerEntity, higherEntity, distance, angle, shareObject);
    }

    private void CompareEntity(MEntityPair e1, MEntityPair e2, out MEntityPair lower, out MEntityPair higher)
    {
        if((e1.entity.entityType == e2.entity.entityType) 
            || (e1.entity.entityType == MEntity.MEntityType.POINT) 
            || (e1.entity.entityType == MEntity.MEntityType.EDGE && e2.entity.entityType == MEntity.MEntityType.FACE))
        {
            lower = e1;
            higher = e2;
            return;
        }
        else
        {
            lower = e2;
            higher = e1;
            return;
        }
    }

    private string GetFaceString(MFace face, MObject obj)
    {
        string hs = "";
        string s;
        switch (face.faceType)
        {
            case MFace.MFaceType.POLYGON:
                hs += "平面";
                MPolygonFace polyf = face as MPolygonFace;
                foreach (MPoint p in polyf.sortedPoints)
                {
                    s = obj.GetPointIdentifier(p);
                    if (s == null)
                    {
                        hs = null;
                        break;
                    }
                    hs += s;
                }
                break;
            case MFace.MFaceType.CIRCLE:
                hs += "圆";
                MCircleFace cirf = face as MCircleFace;
                s = obj.GetPointIdentifier(cirf.circle.center);
                if (s == null)
                {
                    hs = null;
                    break;
                }
                hs += s;
                break;
            default:
                hs = null;
                break;
        }
        return hs;
    }

    private string GetEdgeString(MLinearEdge edge, MObject obj)
    {
        string str = null;
        string s1 = obj.GetPointIdentifier(edge.start);
        string s2 = obj.GetPointIdentifier(edge.end);
        if (s1 != null && s2 != null) str = s1 + s2;
        return str;
    }
}