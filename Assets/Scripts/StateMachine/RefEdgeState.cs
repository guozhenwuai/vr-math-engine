using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefEdgeState : IState
{
    private SceneManager sceneManager;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    public RefEdgeState(SceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.SELECT_REFEDGE;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        foreach (MObject obj in sceneManager.objects)
        {
            MLinearEdge refEdge = obj.refEdge;
            if (refEdge != null)
            {
                refEdge.entityStatus = MEntity.MEntityStatus.SPECIAL;
            }
        }
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        foreach (MObject obj in sceneManager.objects)
        {
            MLinearEdge refEdge = obj.refEdge;
            if (refEdge != null)
            {
                refEdge.entityStatus = MEntity.MEntityStatus.DEFAULT;
            }
        }
    }

    public void OnUpdate()
    {
        sceneManager.UpdateEntityHighlight(MObject.MInteractMode.EDGE_ONLY);
        sceneManager.StartRender();
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if(sceneManager.activeEntity.entity != null)
        {
            MEdge edge = sceneManager.activeEntity.entity as MEdge;
            if(edge.edgeType == MEdge.MEdgeType.LINEAR)
            {
                MLinearEdge refEdge = sceneManager.activeEntity.obj.refEdge;
                if (refEdge != null)
                {
                    refEdge.entityStatus = MEntity.MEntityStatus.DEFAULT;
                }
                sceneManager.activeEntity.obj.SetRefEdge((MLinearEdge)edge);
                edge.entityStatus = MEntity.MEntityStatus.SPECIAL;
            }
        }
    }
}