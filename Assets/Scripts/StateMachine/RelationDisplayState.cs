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

    public void OnEnter(StateMachine machine, IState prevState)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        textMesh.SetActive(true);
        textMesh.GetComponentInChildren<TextMesh>(true).text = "";
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        ClearSelectedEntity();
        textMesh.SetActive(false);
    }

    public void OnUpdate()
    {
        sceneManager.UpdateEntityHighlight(MObject.MInteractMode.ALL);
        if (sceneManager.camera != null) UpdateTextTransform();
        sceneManager.StartRender();
    }

    private void UpdateTextTransform()
    {
        textMesh.transform.rotation = Quaternion.LookRotation((sceneManager.camera.transform.position - textMesh.transform.position) * -1, Vector3.up);
    }

    private void UpdateText()
    {
        string text = "";
        if(selectedEntity.Count == 2)
        {
            MRelation relation = selectedEntity[0].obj.getEntityRelation(selectedEntity[0].entity, selectedEntity[1].entity);
            if (relation != null)
            {
                switch (relation.relationType)
                {
                    case MRelation.EntityRelationType.POINT_POINT:
                        text += "两点距离： " + relation.distance + "\n";
                        break;
                    case MRelation.EntityRelationType.POINT_EDGE:
                        if (MHelperFunctions.FloatEqual(relation.distance, 0))
                        {
                            text += "点在直线上\n";
                        }
                        else
                        {
                            text += "点到直线距离： " + relation.distance + "\n";
                        }
                        break;
                    case MRelation.EntityRelationType.POINT_FACE:
                        if (MHelperFunctions.FloatEqual(relation.distance, 0))
                        {
                            text += "点在平面上\n";
                        }
                        else
                        {
                            text += "点到平面距离： " + relation.distance + "\n";
                        }
                        break;
                    case MRelation.EntityRelationType.EDGE_EDGE:
                        if (MHelperFunctions.FloatEqual(relation.distance, 0) || MHelperFunctions.FloatEqual(relation.angle, 0))
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
                        if (!MHelperFunctions.FloatEqual(relation.distance, 0))
                        {
                            text += "两线距离： " + relation.distance + "\n";
                        }
                        break;
                    case MRelation.EntityRelationType.EDGE_FACE:
                        if (MHelperFunctions.FloatEqual(relation.distance, 0))
                        {
                            text += "线在面上\n";
                        }
                        else if (MHelperFunctions.FloatEqual(relation.distance, -1))
                        {
                            text += "线面相交\n";

                        }
                        if (!MHelperFunctions.FloatEqual(relation.distance, 0))
                        {
                            if (MHelperFunctions.FloatEqual(relation.angle, 0))
                            {
                                text += "线面平行\n";
                                text += "线面距离：" + relation.distance + "\n";
                            }
                            else if (MHelperFunctions.FloatEqual(relation.angle, 90))
                            {
                                text += "线面垂直\n";
                            }
                            else
                            {
                                text += "线面夹角：" + relation.angle + "\n";
                            }
                        }
                        break;
                    case MRelation.EntityRelationType.FACE_FACE:
                        if (MHelperFunctions.FloatEqual(relation.angle, 0))
                        {
                            text += "面面平行\n";
                            text += "面面距离：" + relation.distance + "\n";
                        }
                        else if (MHelperFunctions.FloatEqual(relation.distance, 90))
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
        MObject obj = entityPair.obj;
        if (entity.entityStatus == MEntity.MEntityStatus.SELECT)
        {
            entity.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
        selectedEntity.Remove(entityPair);
    }

    private void AddEntity(MEntityPair entityPair)
    {
        MEntity entity = entityPair.entity;
        MObject obj = entityPair.obj;
        if (entityPair.entity.entityStatus == MEntity.MEntityStatus.DEFAULT)
        {
            entityPair.entity.entityStatus = MEntity.MEntityStatus.SELECT;
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
}