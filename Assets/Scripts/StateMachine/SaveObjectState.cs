using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveObjectState : IState
{
    private SceneManager sceneManager;

    private MObject curObj;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    private VRTK.ControllerInteractionEventHandler rightGripPressed;

    public SaveObjectState(SceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
        rightGripPressed = new VRTK.ControllerInteractionEventHandler(RightGripPressed);
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.SAVE_OBJECT;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        sceneManager.rightEvents.GripPressed += rightGripPressed;
        curObj = null;
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        sceneManager.rightEvents.GripPressed -= rightGripPressed;
        SelectObject(null);
    }

    public void OnUpdate()
    {
        sceneManager.UpdateObjectHighlight();
		if (curObj != null && sceneManager.camera != null) {
			curObj.RotateTextMesh (sceneManager.camera.transform.position);
		}
        sceneManager.StartRender();
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (sceneManager.activeEntity.obj != null)
        {
            SelectObject(sceneManager.activeEntity.obj);
        }
    }

    private void RightGripPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if(curObj != null)
        {
			curObj.ExportObject(System.DateTime.Now.ToFileTime().ToString());
            SelectObject(null);
        }
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
                obj.Select();
                obj.ActiveTextMesh();
                obj.SetMeshText("按右手手柄侧键，\n保存当前选中模型");
            }
            curObj = obj;
        }
    }
}