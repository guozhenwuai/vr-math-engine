using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddRightAngledTriangleState : IState
{
    private SceneManager sceneManager;

    private GameObject activeTextMesh;

    private MObject obj;

    enum STATUS { DEFAULT, CREATE_EDGE, STRETCH };

    private STATUS status;

    Vector3 stablePoint;

    MPoint activePoint;

    MLinearEdge[] activeEdges;

    MPolygonFace activeFace;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    private VRTK.ControllerInteractionEventHandler rightGripPressed;

    public AddRightAngledTriangleState(SceneManager sceneManager, GameObject activeTextMesh)
    {
        this.sceneManager = sceneManager;
        this.activeTextMesh = activeTextMesh;
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
        rightGripPressed = new VRTK.ControllerInteractionEventHandler(RightGripPressed);
        stablePoint = new Vector3();
        activePoint = new MPoint(Vector3.zero);
        activePoint.entityStatus = MEntity.MEntityStatus.ACTIVE;
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.ADD_RIGHT_ANGLED_TRI;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        sceneManager.rightEvents.GripPressed += rightGripPressed;
        status = STATUS.DEFAULT;
        activeEdges = new MLinearEdge[3];
        obj = new MObject(sceneManager.objTemplate);
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        sceneManager.rightEvents.GripPressed -= rightGripPressed;
        ResetStatus();
        activeEdges = null;
        obj = null;
    }

    public void OnUpdate()
    {
        sceneManager.StartRender();
        switch (status)
        {
            case STATUS.CREATE_EDGE:
                UpdateCreateEdge();
                break;
            case STATUS.STRETCH:
                UpdateStretch();
                break;
        }
    }

    private void UpdateCreateEdge()
    {
        Vector3 pos = obj.worldToLocalMatrix.MultiplyPoint(sceneManager.rightControllerPosition);
        activePoint.SetPosition(pos);
        activeEdges[0].end.SetPosition(pos);
        if (activeEdges[0] != null && activeEdges[0].IsValid())
        {
            activeEdges[0].start.Render(obj.localToWorldMatrix);
            activeEdges[0].end.Render(obj.localToWorldMatrix);
            activeEdges[0].Render(obj.localToWorldMatrix);
        }
    }

    private void UpdateStretch()
    {
        Vector3 pos = obj.worldToLocalMatrix.MultiplyPoint(sceneManager.rightControllerPosition);
        MPoint a = activeEdges[0].start;
        MPoint b = activeEdges[0].end;
        MPoint c = activeEdges[1].end;
        float width = Vector3.Distance(a.position, b.position);
        Vector3 p = MHelperFunctions.VectorL2P(pos, activeEdges[0].direction, a.position);
        p = ReviseLength(p, width);
        c.SetPosition(p + b.position);
        if (activeFace != null && activeFace.IsValid())
        {
            activeFace.Render(obj.localToWorldMatrix);
            foreach (MLinearEdge edge in activeEdges)
            {
                edge.Render(obj.localToWorldMatrix);
            }
            a.Render(obj.localToWorldMatrix);
            b.Render(obj.localToWorldMatrix);
            c.Render(obj.localToWorldMatrix);
            activeTextMesh.GetComponentInChildren<TextMesh>().text = "1:" + (p.magnitude / width).ToString();
            activeTextMesh.transform.position = sceneManager.rightControllerPosition + MDefinitions.DEFAULT_ACTIVE_TEXT_OFFSET;
            if (sceneManager.camera != null)
                activeTextMesh.transform.rotation = Quaternion.LookRotation((sceneManager.camera.transform.position - activeTextMesh.transform.position) * -1, Vector3.up);
        }
        else
        {
            activeTextMesh.GetComponentInChildren<TextMesh>().text = "";
        }
    }

    private Vector3 ReviseLength(Vector3 p, float width)
    {
        float length = p.magnitude;
        if (Mathf.Abs(length - 0.5f * width) < MDefinitions.AUTO_REVISE_FACTOR)
        {
            return p.normalized * 0.5f * width;
        }
        else if (Mathf.Abs(length - Mathf.RoundToInt(length / width) * width) < MDefinitions.AUTO_REVISE_FACTOR)
        {
            return p.normalized * Mathf.RoundToInt(length / width) * width;
        }
        return p;
    }

    private void ResetStatus()
    {
        activeEdges[0] = null;
        activeEdges[1] = null;
        activeEdges[2] = null;
        activeFace = null;
        status = STATUS.DEFAULT;
        activeTextMesh.SetActive(false);
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        switch (status)
        {
            case STATUS.DEFAULT:
                stablePoint = obj.worldToLocalMatrix.MultiplyPoint(sceneManager.rightControllerPosition);
                status = STATUS.CREATE_EDGE;
                activeEdges[0] = new MLinearEdge(new MPoint(stablePoint), new MPoint(activePoint.position));
                activeEdges[0].end.edges.Add(activeEdges[0]);
                break;
            case STATUS.CREATE_EDGE:
                activeEdges[1] = new MLinearEdge(activeEdges[0].start, new MPoint(activePoint.position));
                activeEdges[1].end.edges.Add(activeEdges[1]);
                activeEdges[2] = new MLinearEdge(activeEdges[0].end, activeEdges[1].end);
                activeEdges[2].end.edges.Add(activeEdges[2]);
                activeFace = new MPolygonFace(new List<MLinearEdge>(activeEdges));
                activeEdges[1].faces.Add(activeFace);
                activeEdges[2].faces.Add(activeFace);
                activeTextMesh.SetActive(true);
                status = STATUS.STRETCH;
                break;
            case STATUS.STRETCH:
                if (activeFace != null && activeFace.IsValid())
                {
                    obj.CreatePolygonFace(new List<MLinearEdge>(activeEdges));
                    sceneManager.objects.Add(obj);
                    sceneManager.sceneStateMachine.SwitchState((uint)SceneManager.SceneStatus.TRANSFORM, null);
                }
                else
                {
                    ResetStatus();
                }
                break;
        }
    }

    private void RightGripPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        ResetStatus();
    }
}