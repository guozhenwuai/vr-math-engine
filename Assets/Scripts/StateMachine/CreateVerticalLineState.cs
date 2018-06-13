using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateVerticalLineState : IState
{
    private SceneManager sceneManager;

    private GameObject activeTextMesh;

    private List<MEntity> selectedEntity;

    private MObject curObj;

    private MPoint activePoint;

    private MLinearEdge activeEdge;

    private enum STATUS { DEFAULT, CONNECTING, SELECT_POINT, ADJUST_LEN};

    private STATUS status;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    private VRTK.ControllerInteractionEventHandler rightGripPressed;

    public CreateVerticalLineState(SceneManager sceneManager, GameObject activeTextMesh)
    {
        this.sceneManager = sceneManager;
        this.activeTextMesh = activeTextMesh;
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
        rightGripPressed = new VRTK.ControllerInteractionEventHandler(RightGripPressed);
        selectedEntity = new List<MEntity>();
        activePoint = new MPoint(Vector3.zero);
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.CREATE_VERTICAL_LINE;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        sceneManager.rightEvents.GripPressed += rightGripPressed;
        status = STATUS.DEFAULT;
        activePoint.entityStatus = MEntity.MEntityStatus.ACTIVE;
        curObj = null;
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
        sceneManager.rightEvents.GripPressed -= rightGripPressed;
        ResetStatus();
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
        switch (status)
        {
            case STATUS.DEFAULT:
                sceneManager.UpdateEntityHighlight(MObject.MInteractMode.EDGE_EXPT);
                break;
            case STATUS.CONNECTING:
                sceneManager.UpdateEntityHighlight(MObject.MInteractMode.POINT_EXPT, curObj);
                UpdateConnecting();
                break;
            case STATUS.SELECT_POINT:
                sceneManager.UpdateEntityHighlight(MObject.MInteractMode.POINT_ONLY, curObj);
                break;
            case STATUS.ADJUST_LEN:
                UpdateAdjustLen();
                break;
        }
        sceneManager.StartRender();
    }

    private void UpdateConnecting()
    {
        Vector3 pos = curObj.worldToLocalMatrix.MultiplyPoint(sceneManager.rightControllerPosition);
        MPoint p = selectedEntity[0] as MPoint;
        if(sceneManager.activeEntity.entity != null)
        {
            pos = sceneManager.activeEntity.entity.GetProjection(p.position, pos);
        }
        activePoint.SetPosition(pos);
        if(activeEdge == null)
        {
            activeEdge = new MLinearEdge(p, activePoint);
            activeEdge.entityStatus = MEntity.MEntityStatus.ACTIVE;
            activeEdge.end.edges.Add(activeEdge);
        }
        activePoint.Render(curObj.localToWorldMatrix);
        if (activeEdge != null && activeEdge.IsValid())
        {
            activeEdge.Render(curObj.localToWorldMatrix);
        }
    }

    private void UpdateAdjustLen()
    {
        Vector3 pos = curObj.worldToLocalMatrix.MultiplyPoint(sceneManager.rightControllerPosition);
        MFace f = selectedEntity[0] as MFace;
        MPoint p = selectedEntity[1] as MPoint;
        pos = f.GetVerticalPoint(p.position, pos);
        float relativeLen = curObj.RefEdgeRelativeLength(Vector3.Distance(p.position, pos));
        pos = RevisePos(ref relativeLen, p.position, pos);
        activePoint.SetPosition(pos);
        activeTextMesh.GetComponentInChildren<TextMesh>().text = relativeLen.ToString();
        activeTextMesh.transform.position = sceneManager.rightControllerPosition + MDefinitions.DEFAULT_ACTIVE_TEXT_OFFSET;
        if (sceneManager.camera != null)
            activeTextMesh.transform.rotation = Quaternion.LookRotation((sceneManager.camera.transform.position - activeTextMesh.transform.position) * -1, Vector3.up);
        if (activeEdge == null)
        {
            activeEdge = new MLinearEdge(p, activePoint);
            activeEdge.entityStatus = MEntity.MEntityStatus.ACTIVE;
            activeEdge.end.edges.Add(activeEdge);
        }
        activePoint.Render(curObj.localToWorldMatrix);
        if (activeEdge != null && activeEdge.IsValid())
        {
            activeEdge.Render(curObj.localToWorldMatrix);
        }
    }

    private Vector3 RevisePos(ref float length, Vector3 startPos, Vector3 curPos)
    {
        if(Mathf.Abs(length - 0.5f) < MDefinitions.AUTO_REVISE_FACTOR)
        {
            length = 0.5f;
            curPos = (curPos - startPos).normalized * 0.5f + startPos;
        } else if(Mathf.Abs(length - Mathf.Round(length)) < MDefinitions.AUTO_REVISE_FACTOR)
        {
            length = Mathf.Round(length);
            curPos = (curPos - startPos).normalized * length + startPos;
        }
        return curPos;
    }

    private void RightGripPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        ResetStatus();
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (sceneManager.pointerOnMenu) return;
        switch (status)
        {
            case STATUS.DEFAULT:
                if(sceneManager.activeEntity.entity != null)
                {
                    if (sceneManager.activeEntity.entity.entityType == MEntity.MEntityType.POINT)
                    {
                        status = STATUS.CONNECTING;
                        activeEdge = null;
                        SelectEntity(sceneManager.activeEntity.entity);
                        curObj = sceneManager.activeEntity.obj;
                    }
                    else if(((MFace)sceneManager.activeEntity.entity).faceType == MFace.MFaceType.POLYGON 
                        || ((MFace)sceneManager.activeEntity.entity).faceType == MFace.MFaceType.CIRCLE
                        || ((MFace)sceneManager.activeEntity.entity).faceType == MFace.MFaceType.GENERAL_FLAT)
                    {
                        status = STATUS.SELECT_POINT;
                        SelectEntity(sceneManager.activeEntity.entity);
                        curObj = sceneManager.activeEntity.obj;
                    }
                }
                break;
            case STATUS.SELECT_POINT:
                if(sceneManager.activeEntity.entity == null)
                {
                    ResetStatus();
                }
                else
                {
                    SelectEntity(sceneManager.activeEntity.entity);
                    activeTextMesh.SetActive(true);
                    status = STATUS.ADJUST_LEN;
                    activeEdge = null;
                }
                break;
            case STATUS.CONNECTING:
                if(sceneManager.activeEntity.entity == null 
                    || (sceneManager.activeEntity.entity.entityType == MEntity.MEntityType.EDGE 
                        && ((MEdge)sceneManager.activeEntity.entity).edgeType != MEdge.MEdgeType.LINEAR)
                    || (sceneManager.activeEntity.entity.entityType == MEntity.MEntityType.FACE
                        && ((MFace)sceneManager.activeEntity.entity).faceType == MFace.MFaceType.GENERAL_CURVE)
                  )
                {
                    ResetStatus();
                }
                else
                {
                    curObj.CreateLinearEdge((MPoint)selectedEntity[0], new MPoint(activePoint.position));
                    ResetStatus();
                }
                break;
            case STATUS.ADJUST_LEN:
                curObj.CreateLinearEdge((MPoint)selectedEntity[1], new MPoint(activePoint.position));
                ResetStatus();
                break;
        }
    }

    private void ResetStatus()
    {
        status = STATUS.DEFAULT;
        ClearSelectedEntity();
        activeTextMesh.SetActive(false);
        activeEdge = null;
        curObj = null;
    }

    private void SelectEntity(MEntity entity)
    {
        if (selectedEntity.Contains(entity))
        {
            RemoveEntity(entity);
        }
        else
        {
            AddEntity(entity);
        }
    }

    private void RemoveEntity(MEntity entity)
    {
        if (entity.entityStatus == MEntity.MEntityStatus.SELECT)
        {
            entity.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
        selectedEntity.Remove(entity);
    }

    private void AddEntity(MEntity entity)
    {
        if (entity.entityStatus != MEntity.MEntityStatus.SPECIAL)
        {
            entity.entityStatus = MEntity.MEntityStatus.SELECT;
        }
        selectedEntity.Add(entity);
    }

    private void ClearSelectedEntity()
    {
        foreach (MEntity entity in selectedEntity)
        {
            entity.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
        selectedEntity.Clear();
    }
}