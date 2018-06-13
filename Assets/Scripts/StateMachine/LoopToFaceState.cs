using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopToFaceState : IState
{
    private SceneManager sceneManager;

    private List<MLinearEdge> selectEdges;

    private MObject curObject;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    public LoopToFaceState(SceneManager sceneManager)
    {
        this.sceneManager = sceneManager;
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
        selectEdges = new List<MLinearEdge>();
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.LOOP_TO_FACE;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;

        curObject = null;
    }

    public void OnUpdate()
    {
        sceneManager.UpdateEntityHighlight(MObject.MInteractMode.EDGE_ONLY);
        sceneManager.StartRender();
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if(sceneManager.activeEntity.entity != null && ((MEdge)sceneManager.activeEntity.entity).edgeType == MEdge.MEdgeType.LINEAR)
        {
            if(curObject == null)
            {
                curObject = sceneManager.activeEntity.obj;
            }
            else
            {
                if (sceneManager.activeEntity.obj != curObject)
                {
                    ResetStatus();
                    return;
                }
            }
            MLinearEdge le = sceneManager.activeEntity.entity as MLinearEdge;
            if(selectEdges.Count > 1)
            {
                Vector3 normal = Vector3.Cross(selectEdges[0].direction, selectEdges[1].direction).normalized;
                if(!MHelperFunctions.Perpendicular(normal, le.direction))
                {
                    ResetStatus();
                    return;
                }
            }
            SelectEdge(le);
            if(selectEdges.Count < 3)
            {
                curObject = null;
                return;
            }
            MPolygonFace face = new MPolygonFace(selectEdges);
            if (face.IsValid())
            {
                curObject.CreatePolygonFace(new List<MLinearEdge>(selectEdges));
                ResetStatus();
            }
        }
        else
        {
            ResetStatus();
        }
    }

    private void ResetStatus()
    {
        curObject = null;
        ClearSelectedEdge();
    }

    private void SelectEdge(MLinearEdge edge)
    {
        if (selectEdges.Contains(edge))
        {
            RemoveEdge(edge);
        }
        else
        {
            AddEdge(edge);
        }
    }

    private void RemoveEdge(MLinearEdge edge)
    {
        if (edge.entityStatus == MEntity.MEntityStatus.SELECT)
        {
            edge.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
        selectEdges.Remove(edge);
    }

    private void AddEdge(MLinearEdge edge)
    {
        if (edge.entityStatus != MEntity.MEntityStatus.SPECIAL)
        {
            edge.entityStatus = MEntity.MEntityStatus.SELECT;
        }
        selectEdges.Add(edge);
    }

    private void ClearSelectedEdge()
    {
        foreach (MLinearEdge edge in selectEdges)
        {
            edge.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
        selectEdges.Clear();
    }
}