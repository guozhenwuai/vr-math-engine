using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePointState : IState
{
    private SceneManager sceneManager;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    private MPoint activePoint;

    private MObject curObj;

    public CreatePointState(SceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.CREATE_POINT;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        sceneManager.rightEvents.GripPressed += RightGripPressed;
        activePoint = new MPoint(Vector3.zero);
        activePoint.entityStatus = MEntity.MEntityStatus.ACTIVE;
        curObj = null;
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        activePoint = null;
        SelectObject(null);
    }

    public void OnUpdate()
    {
        if(curObj != null)
        {
            sceneManager.UpdateEntityHighlight(MObject.MInteractMode.ALL, curObj);
            UpdateActivePointPosition();
        }
        else
        {
            sceneManager.UpdateObjectHighlight();
        }
        sceneManager.StartRender();
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (sceneManager.pointerOnMenu) return;
        if (curObj == null)
        {
            if(sceneManager.activeEntity.obj != null)
            {
                SelectObject(sceneManager.activeEntity.obj);
            }
        } else
        {
            curObj.CreatePoint(activePoint.position);
        }
    }

    private void RightGripPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        SelectObject(null);
    }

    private void UpdateActivePointPosition()
    {
        Vector3 pos = sceneManager.rightControllerPosition;
        MEntity entity = sceneManager.activeEntity.entity;
        if (entity != null)
        {
            pos = entity.SpecialPointFind(curObj.worldToLocalMatrix.MultiplyPoint(pos));
        }
        else
        {
            pos = curObj.worldToLocalMatrix.MultiplyPoint(pos);
        }
        activePoint.SetPosition(pos);
        activePoint.Render(curObj.localToWorldMatrix);
    }

    private void SelectObject(MObject obj)
    {
        if (curObj != obj)
        {
            if (curObj != null)
            {
                curObj.ResetStatus();
                curObj.InactiveTextMesh();
            }
            if (obj != null)
            {
                obj.ResetStatus();
                obj.ActiveTextMesh();
                obj.SetMeshText("按右手手柄侧键，\n重新选择操作模型");
            }
            curObj = obj;
        }
    }
}