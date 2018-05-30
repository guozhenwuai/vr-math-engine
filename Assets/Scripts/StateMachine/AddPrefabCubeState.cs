using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddPrefabCubeState : IState
{
    private SceneManager sceneManager;

    private MObject curObject;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    public AddPrefabCubeState(SceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.ADD_PREFAB_CUBE;
    }

    public void OnEnter(StateMachine machine, IState prevState)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        curObject = new MObject(sceneManager.objTemplate, MObject.MPrefabType.CUBE);
        curObject.Select();
        curObject.transform.parent = sceneManager.rightController.transform;
        curObject.transform.localPosition = MDefinitions.DEFAULT_PREFAB_OFFSET;
        curObject.transform.localEulerAngles = Vector3.zero;
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        if(curObject != null)
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
        if(curObject != null)curObject.Render();
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        curObject.transform.parent = null;
        sceneManager.objects.Add(curObject);
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.TRANSFORM);
    }
}