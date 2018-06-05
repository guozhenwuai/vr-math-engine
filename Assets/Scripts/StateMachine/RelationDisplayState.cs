using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationDisplayState : IState
{
    private SceneManager sceneManager;

    private GameObject textMesh;

    private List<MEntityPair> selectedEntity;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    public RelationDisplayState(SceneManager sceneManager, GameObject textMesh)
    {
        this.sceneManager = sceneManager;
        this.textMesh = textMesh;
        selectedEntity = new List<MEntityPair>();
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
        textMesh.GetComponentInChildren<TextMesh>(true).text = "";
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

    private void UpdateTextTransform()
    {
        textMesh.transform.rotation = Quaternion.LookRotation((sceneManager.camera.transform.position - textMesh.transform.position) * -1, Vector3.up) * Quaternion.Euler(-90, 0, 0);
    }

    private void UpdateText()
    {
        string text = "";
        if(selectedEntity.Count == 2)
        {
            MRelation relation = GetEntityRelation(selectedEntity[0], selectedEntity[1]);
            if (relation != null)
            {
                switch (relation.relationType)
                {
                    case MRelation.EntityRelationType.POINT_POINT:
                        if (relation.shareObject)
                        {
                            text += "两点距离： " + relation.distance + "\n";
                        }
                        break;
                    case MRelation.EntityRelationType.POINT_EDGE:
                        if (relation.shareObject)
                        {
                            if (MHelperFunctions.FloatEqual(relation.distance, 0))
                            {
                                text += "点在直线上\n";
                            }
                            else
                            {
                                text += "点到直线距离： " + relation.distance + "\n";
                            }
                        }
                        break;
                    case MRelation.EntityRelationType.POINT_FACE:
                        if (relation.shareObject)
                        {
                            if (MHelperFunctions.FloatEqual(relation.distance, 0))
                            {
                                text += "点在平面上\n";
                            }
                            else
                            {
                                text += "点到平面距离： " + relation.distance + "\n";
                            }
                        }
                        break;
                    case MRelation.EntityRelationType.EDGE_EDGE:
                        if ((relation.shareObject && MHelperFunctions.FloatEqual(relation.distance, 0)) || MHelperFunctions.FloatEqual(relation.angle, 0))
                        {
                            text += "两线共面\n";
                        }
                        if (MHelperFunctions.FloatEqual(relation.angle, 0))
                        {
                            text += "两线平行\n";
                        }
                        else if (MHelperFunctions.FloatEqual(relation.angle, 90))
                        {
                            text += "两线垂直\n";
                        }
                        else
                        {
                            text += "两线夹角：" + relation.angle + "\n";
                        }
                        if (relation.shareObject && !MHelperFunctions.FloatEqual(relation.distance, 0))
                        {
                            text += "两线距离： " + relation.distance + "\n";
                        }
                        break;
                    case MRelation.EntityRelationType.EDGE_FACE:
                        if (relation.shareObject && MHelperFunctions.FloatEqual(relation.distance, 0))
                        {
                            text += "线在面上\n";
                        }
                        else if (relation.shareObject && MHelperFunctions.FloatEqual(relation.distance, -1))
                        {
                            text += "线面相交\n";

                        }
                        if (MHelperFunctions.FloatEqual(relation.angle, 0))
                        {
                            text += "线面平行\n";
                            if (relation.shareObject)
                            {
                                text += "线面距离：" + relation.distance + "\n";
                            }
                        }
                        else if (MHelperFunctions.FloatEqual(relation.angle, 90))
                        {
                            text += "线面垂直\n";
                        }
                        else
                        {
                            text += "线面夹角：" + relation.angle + "\n";
                        }
                        break;
                    case MRelation.EntityRelationType.FACE_FACE:
                        if (MHelperFunctions.FloatEqual(relation.angle, 0))
                        {
                            text += "面面平行\n";
                            if (relation.shareObject)
                            {
                                text += "面面距离：" + relation.distance + "\n";
                            }
                        }
                        else if (MHelperFunctions.FloatEqual(relation.angle, 90))
                        {
                            text += "面面垂直\n";
                        }
                        else
                        {
                            text += "面面夹角： " + relation.angle + "\n";
                        }
                        break;
                }
            }
        }
        textMesh.GetComponentInChildren<TextMesh>(true).text = text;
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
        return new MRelation(type, lowerEntity.entity, higherEntity.entity, distance, angle, shareObject);
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
}