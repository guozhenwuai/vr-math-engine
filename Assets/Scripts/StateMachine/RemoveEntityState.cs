using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveEntityState : IState
{
    private SceneManager sceneManager;

    private MEntity selectEntity;

    private MObject curObj;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    private VRTK.ControllerInteractionEventHandler rightGripPressed;

    public RemoveEntityState(SceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
        rightGripPressed = new VRTK.ControllerInteractionEventHandler(RightGripPressed);
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.REMOVE_ENTITY;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        sceneManager.rightEvents.GripPressed += rightGripPressed;
        selectEntity = null;
        curObj = null;
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        sceneManager.rightEvents.GripPressed -= rightGripPressed;
        SelectEntity(null);
        SelectObject(null);
    }

    public void OnUpdate()
    {
        sceneManager.UpdateEntityHighlight(MObject.MInteractMode.ALL);
        if (curObj != null && sceneManager.camera != null)
        {
            curObj.RotateTextMesh(sceneManager.camera.transform.position);
        }
        sceneManager.StartRender();
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if(sceneManager.activeEntity.entity != null)
        {
            SelectEntity(sceneManager.activeEntity.entity);
            SelectObject(sceneManager.activeEntity.obj);
        }
    }

    private void RightGripPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if(selectEntity != null)
        {
            curObj.RemoveEntity(selectEntity);
        }
        selectEntity = null;
        SelectObject(null);
    }

    private void SelectObject(MObject obj)
    {
        if (curObj != obj)
        {
            if (curObj != null)
            {
                curObj.InactiveTextMesh();
            }
            if (obj != null)
            {
                obj.ActiveTextMesh();
                obj.SetMeshText("按右手手柄侧键，\n删除当前选中图元");
            }
            curObj = obj;
        }
    }

    private void SelectEntity(MEntity entity)
    {
        
        if(entity != selectEntity)
        {
            if (selectEntity != null)
            {
                selectEntity.entityStatus = MEntity.MEntityStatus.DEFAULT;
            }
            if (entity != null)
            {
                entity.entityStatus = MEntity.MEntityStatus.SELECT;
            }
            selectEntity = entity;
        }
    }
}