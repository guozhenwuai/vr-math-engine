using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformState : IState
{
    private SceneManager sceneManager;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;
    private VRTK.ControllerInteractionEventHandler rightTriggerReleased;
    private VRTK.ControllerInteractionEventHandler leftTriggerPressed;
    private VRTK.ControllerInteractionEventHandler leftTriggerReleased;

    private bool leftPressed;
    private bool rightPressed;
    private bool transforming;
    private bool scaling;

    private MObject curObj;

    private float initDis;
    private float initScale;

    public TransformState(SceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
        rightTriggerReleased = new VRTK.ControllerInteractionEventHandler(RightTriggerReleased);
        leftTriggerPressed = new VRTK.ControllerInteractionEventHandler(LeftTriggerPressed);
        leftTriggerReleased = new VRTK.ControllerInteractionEventHandler(LeftTriggerReleased);
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.TRANSFORM;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        sceneManager.rightEvents.TriggerReleased += rightTriggerReleased;
        sceneManager.leftEvents.TriggerPressed += leftTriggerPressed;
        sceneManager.leftEvents.TriggerReleased += leftTriggerReleased;
        leftPressed = false;
        rightPressed = false;
        transforming = false;
        scaling = false;
        curObj = null;
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        sceneManager.rightEvents.TriggerReleased -= rightTriggerReleased;
        sceneManager.leftEvents.TriggerPressed -= leftTriggerPressed;
        sceneManager.leftEvents.TriggerReleased -= leftTriggerReleased;
        if (transforming && curObj!= null && curObj.transform.parent != null)
        {
            curObj.transform.parent = null;
        }
        if(curObj != null)
        {
            curObj.ResetStatus();
            curObj = null;
        }
    }

    public void OnUpdate()
    {
        if(!transforming && !scaling)sceneManager.UpdateObjectHighlight();
        if(!scaling && curObj != null && leftPressed && rightPressed)
        {
            scaling = true;
            initDis = Vector3.Distance(sceneManager.leftControllerPosition, sceneManager.rightControllerPosition);
            initScale = curObj.scale;
        }
        else if (scaling)
        {
            float dis = Vector3.Distance(sceneManager.leftControllerPosition, sceneManager.rightControllerPosition);
            curObj.scale = dis / initDis * initScale;
        }
        sceneManager.StartRender();
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if(sceneManager.activeEntity.obj != null && !leftPressed) // 移动&旋转
        {
            SelectObject(sceneManager.activeEntity.obj);
            curObj.transform.parent = sceneManager.rightController.transform;
            transforming = true;
        } else
        {
            rightPressed = true;
        }
    }

    private void RightTriggerReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (transforming)
        {
            curObj.transform.parent = null;
            transforming = false;
        }
        else if (scaling)
        {
            scaling = false;
        }
        rightPressed = false;
    }

    private void LeftTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        leftPressed = true;
    }

    private void LeftTriggerReleased(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (scaling)
        {
            scaling = false;
        }
        leftPressed = false;
    }

    private void SelectObject(MObject obj)
    {
        if(curObj != obj)
        {
           if(curObj != null)
            {
                curObj.ResetStatus();
            } 
           if(obj != null)
            {
                obj.Select();
            }
            curObj = obj;
        }
    }
}
