using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddPrefabState : IState
{
    private SceneManager sceneManager;

    private MObject curObject;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    public AddPrefabState(SceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.ADD_PREFAB;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        curObject = new MObject(sceneManager.objTemplate, (MObject.MPrefabType)param);
        if (curObject == null) return;
        curObject.Select();
        curObject.transform.parent = sceneManager.rightController.transform;
        curObject.transform.localPosition = MDefinitions.DEFAULT_PREFAB_OFFSET;
        curObject.transform.localEulerAngles = Vector3.zero;
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        if (curObject != null)
        {
            curObject.ResetStatus();
            if (curObject.transform.parent != null)
            {
                curObject = null;
            }
        }
    }

    public void OnUpdate()
    {
        sceneManager.StartRender();
        if (curObject != null) curObject.Render();
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (sceneManager.pointerOnMenu) return;
        curObject.transform.parent = null;
        sceneManager.objects.Add(curObject);
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.TRANSFORM, null);
    }
}