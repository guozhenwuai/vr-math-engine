using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectPointState : IState
{
    private SceneManager sceneManager;

    private MPoint selectPoint;

    private MObject curObject;

    private MLinearEdge activeEdge;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    bool connecting;

    public ConnectPointState(SceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.CONNECT_POINT;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        connecting = false;
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        ClearSelectPoint();
        activeEdge = null;
        curObject = null;
    }

    public void OnUpdate()
    {
        sceneManager.UpdateEntityHighlight(MObject.MInteractMode.POINT_ONLY);
        sceneManager.StartRender();
        if (connecting)
        {
            UpdateActiveEdge();
        }
    }

    private void UpdateActiveEdge()
    {
        Vector3 v = curObject.worldToLocalMatrix.MultiplyPoint(sceneManager.rightControllerPosition);
        if (activeEdge == null)
        {
            activeEdge = new MLinearEdge(selectPoint, new MPoint(v));
            activeEdge.entityStatus = MEntity.MEntityStatus.ACTIVE;
        }
        else
        {
            activeEdge.SetEndPoint(v);
        }
        if(activeEdge != null && activeEdge.IsValid())
        {
            activeEdge.Render(curObject.localToWorldMatrix);
        }
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if(sceneManager.activeEntity.entity == null || sceneManager.activeEntity.entity.entityType != MEntity.MEntityType.POINT)
        {
            connecting = false;
            activeEdge = null;
            curObject = null;
            ClearSelectPoint();
        }
        else if (connecting)
        {
            if(curObject != sceneManager.activeEntity.obj)
            {
                activeEdge = null;
                curObject = sceneManager.activeEntity.obj;
                SelectPoint((MPoint)sceneManager.activeEntity.entity);
            } else if((MPoint)sceneManager.activeEntity.entity == selectPoint)
            {
                connecting = false;
                activeEdge = null;
                curObject = null;
                ClearSelectPoint();
            } else
            {
                MPoint p = (MPoint)sceneManager.activeEntity.entity;
                curObject.CreateLinearEdge(selectPoint, p);
                connecting = false;
                activeEdge = null;
                curObject = null;
                ClearSelectPoint();
            }
        }
        else
        {
            connecting = true;
            activeEdge = null;
            curObject = sceneManager.activeEntity.obj;
            SelectPoint((MPoint)sceneManager.activeEntity.entity);
        }
    }

    private void ClearSelectPoint()
    {
        if (selectPoint != null)
        {
            selectPoint.entityStatus = MEntity.MEntityStatus.DEFAULT;
            selectPoint = null;
        }
    }

    private void SelectPoint(MPoint point)
    {
        ClearSelectPoint();
        if(point != null)
        {
            point.entityStatus = MEntity.MEntityStatus.SELECT;
        }
        selectPoint = point;
    }
}