using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCuttingState : IState
{
    private SceneManager sceneManager;

    private MObject curObj;

    private enum STATUS { SELECT, CUT};

    private STATUS status;

    private GameObject quad;

    private Material faceMat;

    private Material edgeEffectMat;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    private VRTK.ControllerInteractionEventHandler rightGripPressed;

    public ObjectCuttingState(SceneManager sceneManager, GameObject quad)
    {
        this.sceneManager = sceneManager;
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
        rightGripPressed = new VRTK.ControllerInteractionEventHandler(RightGripPressed);
        this.quad = quad;
        this.faceMat = MMaterial.GetShadingFaceMat();
        this.edgeEffectMat = MMaterial.GetShadingEdgeMat();
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.OBJECT_CUTTING;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        sceneManager.rightEvents.GripPressed += rightGripPressed;
        status = STATUS.SELECT;
        curObj = null;
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        sceneManager.rightEvents.GripPressed -= rightGripPressed;
        SelectObject(null);
        quad.SetActive(false);
    }

    public void OnUpdate()
    {
        switch (status)
        {
            case STATUS.SELECT:
                sceneManager.UpdateObjectHighlight();
                sceneManager.StartRender();
                break;
            case STATUS.CUT:
                sceneManager.StartRenderFace(edgeEffectMat);
                sceneManager.StartRenderFace(faceMat);
                sceneManager.StartRender();
                break;

        }
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        switch (status)
        {
            case STATUS.SELECT:
                if (sceneManager.activeEntity.obj != null)
                {
                    SelectObject(sceneManager.activeEntity.obj);
                    status = STATUS.CUT;
                    quad.SetActive(true);
                }
                break;
            case STATUS.CUT:
                List<MObject> objects = curObj.PlaneSplit(quad.transform.TransformDirection(Vector3.forward).normalized, quad.transform.TransformPoint(Vector3.zero));
                if(objects.Count > 1)
                {
                    sceneManager.objects.Remove(curObj);
                    foreach (MObject obj in objects)
                    {
                        sceneManager.objects.Add(obj);
                    }
                    curObj.Destroy();
                    curObj = null;
                    sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.TRANSFORM, null);
                }
                else
                {
                    foreach(MObject obj in objects)
                    {
                        obj.Destroy();
                    }
                }
                break;
        }
        
    }

    private void RightGripPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        ResetStatus();
    }

    private void ResetStatus()
    {
        SelectObject(null);
        status = STATUS.SELECT;
        quad.SetActive(false);
    }

    private void SelectObject(MObject obj)
    {
        if (curObj != obj)
        {
            if (curObj != null)
            {
                curObj.ResetStatus();
            }
            if (obj != null)
            {
                obj.Select();
            }
            curObj = obj;
        }
    }
}