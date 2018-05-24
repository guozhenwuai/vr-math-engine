using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionHandler : MonoBehaviour {
    public SceneManager sceneManager;

    public enum ControllerType { LEFT, RIGHT};

    public ControllerType type;

    private VRTK.VRTK_ControllerEvents controllerEvents;

	// Use this for initialization
	void Start () {
        controllerEvents = GetComponent<VRTK.VRTK_ControllerEvents>();
        controllerEvents.TriggerPressed += new VRTK.ControllerInteractionEventHandler(DoTriggerPressed);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void DoTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        sceneManager.HandleTriggerClicked(type, e);
    }
}
