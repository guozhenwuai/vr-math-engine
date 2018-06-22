using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelMenuListener : MonoBehaviour {
    public SceneManager sceneManager;

    public GameObject controllerMenu;

    public Text blackboardButtonText;

    private bool transforming = false;

    private float lastX;

    private float target = -1;

    private VRTK.ControllerInteractionEventHandler leftTouchpadTouchStart;

    private VRTK.ControllerInteractionEventHandler leftTouchpadTouchEnd;

    private VRTK.ControllerInteractionEventHandler leftTouchpadAxisChanged;

    // Use this for initialization
    void Start () {
        leftTouchpadTouchStart = new VRTK.ControllerInteractionEventHandler(LeftTouchpadTouchStart);
        leftTouchpadTouchEnd = new VRTK.ControllerInteractionEventHandler(LeftTouchpadTouchEnd);
        leftTouchpadAxisChanged = new VRTK.ControllerInteractionEventHandler(LeftTouchpadAxisChanged);
        AttachTouchpadEventHandler();
    }
	
	// Update is called once per frame
	void Update () {
        if (transforming)
        {
            float rz = controllerMenu.transform.localEulerAngles.z;
            if(target < 0)
            {
                target = GetClosetAngle(rz);
				Debug.Log ("rz: " + rz + ", target: " + target);
            }
			Debug.Log ("stop transform");
            controllerMenu.transform.localEulerAngles = new Vector3(0, 0, target);
            transforming = false;
        }
    }

    public void AttachTouchpadEventHandler()
    {
        sceneManager.leftEvents.TouchpadTouchStart += leftTouchpadTouchStart;
        sceneManager.leftEvents.TouchpadTouchEnd += leftTouchpadTouchEnd;
        sceneManager.leftEvents.TouchpadAxisChanged += leftTouchpadAxisChanged;
    }

    public void DetachTouchpadEventHandler()
    {
        sceneManager.leftEvents.TouchpadTouchStart -= leftTouchpadTouchStart;
        sceneManager.leftEvents.TouchpadTouchEnd -= leftTouchpadTouchEnd;
        sceneManager.leftEvents.TouchpadAxisChanged -= leftTouchpadAxisChanged;
    }

    private float GetClosetAngle(float x)
    {
		return (Mathf.Round(x / 90) * 90) % 360;
    }

    private void LeftTouchpadAxisChanged(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        float newX = e.touchpadAxis.x;
        controllerMenu.transform.Rotate(Vector3.forward, (lastX - newX) * 90);
        lastX = newX;
    }

    private void LeftTouchpadTouchStart(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        transforming = false;
        lastX = e.touchpadAxis.x;
    }

    private void LeftTouchpadTouchEnd(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        transforming = true;
		target = -1;
    }

    public void OnTransformButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.TRANSFORM, null);
    }

    public void OnRemoveObjectButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.REMOVE_OBJECT, null);
    }

    public void OnStatisticButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.STATISTIC_DISPLAY, null);
    }

    public void OnRelationshipButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.RELATION_DISPLAY, null);
    }

    public void OnSaveObjectButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.SAVE_OBJECT, null);
    }

    public void OnLoadObjectButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.LOAD_OBJECT, null);
    }

    public void OnRefedgeButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.SELECT_REFEDGE, null);
    }

    public void OnConnectPointButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.CONNECT_POINT, null);
    }

    public void OnCreatePointButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.CREATE_POINT, null);
    }

    public void OnVerticalLineButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.CREATE_VERTICAL_LINE, null);
    }

    public void OnAngleSpecifiedLineButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.CREATE_ANGLE, null);
    }

    public void OnLoopToFaceButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.LOOP_TO_FACE, null);
    }

    public void OnRemoveEntityButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.REMOVE_ENTITY, null);
    }

    public void OnPrefabCubeButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.ADD_PREFAB, MObject.MPrefabType.CUBE);
    }

    public void OnPrefabSphereButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.ADD_PREFAB, MObject.MPrefabType.SPHERE);
    }

    public void OnPrefabConeButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.ADD_PREFAB, MObject.MPrefabType.CONE);
    }

    public void OnPrefabCylinderButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.ADD_PREFAB, MObject.MPrefabType.CYLINDER);
    }

    public void OnPrefabPrismButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.ADD_PREFAB, MObject.MPrefabType.PRISM);
    }

    public void OnPrefabPyramidButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.ADD_PREFAB, MObject.MPrefabType.PYRAMID);
    }

    public void OnPrefabRectangleButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.ADD_RECTANGLE, null);
    }

    public void OnPrefabRegularTriangleButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.ADD_PREFAB, MObject.MPrefabType.REGULAR_TRIANGLE);
    }

    public void OnPrefabRightAngledTriangleButtonClick()
    {
        sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.ADD_RIGHT_ANGLED_TRI, null);
    }

	public void OnPlaneCutButtonClick()
	{
		sceneManager.sceneStateMachine.SwitchState ((uint)SceneManager.SceneStatus.OBJECT_CUTTING, null);
	}

    public void OnBlackboardButtonClick()
    {
        if(string.Equals(blackboardButtonText.text, "Blackboard OFF"))
        {
            sceneManager.blackboard.SetActive(false);
            blackboardButtonText.text = "Blackboard ON";
        }
        else
        {
            sceneManager.blackboard.SetActive(true);
            blackboardButtonText.text = "Blackboard OFF";
        }
    }
}
