using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelMenuListener : MonoBehaviour {
    public SceneManager sceneManager;

    public GameObject controllerMenu;

    private bool transforming = false;

    private float lastX;

    private float target = -1;

    private float angleThreshold = 5;

    private float transformSpeed = 2f;

	// Use this for initialization
	void Start () {
        sceneManager.leftEvents.TouchpadTouchStart += new VRTK.ControllerInteractionEventHandler(LeftTouchpadTouchStart);
        sceneManager.leftEvents.TouchpadTouchEnd += new VRTK.ControllerInteractionEventHandler(LeftTouchpadTouchEnd);
        sceneManager.leftEvents.TouchpadAxisChanged += new VRTK.ControllerInteractionEventHandler(LeftTouchpadAxisChanged);
    }
	
	// Update is called once per frame
	void Update () {
        if (transforming)
        {
            float rz = controllerMenu.transform.localEulerAngles.z;
            if(target < 0)
            {
                target = GetClosetAngle(rz);
            }
            if(Mathf.Abs(target - rz) < angleThreshold)
            {
                controllerMenu.transform.localEulerAngles = new Vector3(0, 0, target);
                transforming = false;
            }
            else
            {
                controllerMenu.transform.Rotate(Vector3.forward, transformSpeed * (target > rz? -1: 1));
            }
        }
    }

    private float GetClosetAngle(float x)
    {
        return Mathf.Round(x / 90) * 90;
    }

    private void LeftTouchpadAxisChanged(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        float newX = e.touchpadAxis.x;
        controllerMenu.transform.Rotate(Vector3.forward, lastX - newX);
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
}
