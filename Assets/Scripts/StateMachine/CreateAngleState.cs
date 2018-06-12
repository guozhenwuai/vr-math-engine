using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateAngleState : IState
{
    private SceneManager sceneManager;

    private GameObject activeTextMesh;

    private MEntity selectFace;

    private MObject curObj;

    private MPoint activePoint;

    private MPoint[] anglePoints;

    private MLinearEdge activeEdge;

    private bool validAngle;

    private enum STATUS { SELECT_FACE, SELECT_POINT, ADJUST_ANGLE };

    private STATUS status;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    private VRTK.ControllerInteractionEventHandler rightGripPressed;

    public CreateAngleState(SceneManager sceneManager, GameObject activeTextMesh)
    {
        this.sceneManager = sceneManager;
        this.activeTextMesh = activeTextMesh;
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
        rightGripPressed = new VRTK.ControllerInteractionEventHandler(RightGripPressed);
        activePoint = new MPoint(Vector3.zero);
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.CREATE_ANGLE;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        sceneManager.rightEvents.GripPressed += rightGripPressed;
        status = STATUS.SELECT_FACE;
        activePoint.entityStatus = MEntity.MEntityStatus.ACTIVE;
        curObj = null;
        anglePoints = new MPoint[3];
        validAngle = false;
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        sceneManager.rightEvents.GripPressed -= rightGripPressed;
        ResetStatus();
    }

    public void OnUpdate()
    {
        switch (status)
        {
            case STATUS.SELECT_FACE:
                sceneManager.UpdateEntityHighlight(MObject.MInteractMode.FACE_ONLY);
                break;
            case STATUS.SELECT_POINT:
                sceneManager.UpdateEntityHighlight(MObject.MInteractMode.POINT_ONLY, curObj);
                break;
            case STATUS.ADJUST_ANGLE:
                UpdateAdjustAngle();
                break;
        }
        sceneManager.StartRender();
    }

    private void UpdateAdjustAngle()
    {
        validAngle = false;
        Vector3 pos = curObj.worldToLocalMatrix.MultiplyPoint(sceneManager.rightControllerPosition);
        MPolygonFace polyf = selectFace as MPolygonFace;
        pos = MHelperFunctions.PointProjectionInFace(pos, polyf.normal, anglePoints[0].position);
        Vector3 axisPoint = anglePoints[2].position;
        Vector3 dir1 = pos - axisPoint;
        Vector3 dir2 = anglePoints[0].position - axisPoint;
        Vector3 dir3 = anglePoints[1].position - axisPoint;
        float totalAngle = MHelperFunctions.CalcRadAngle(dir2, dir3);
        float angle1 = MHelperFunctions.CalcRadAngle(dir2, dir1);
        float angle2 = MHelperFunctions.CalcRadAngle(dir3, dir1);
        if (Mathf.Abs(angle1 + angle2 - totalAngle) < MDefinitions.VECTOR3_PRECISION)
        {
            dir1 = ReviseAngle(dir1, dir2, dir3);
            Vector3 intersect = new Vector3();
            foreach (MLinearEdge le in polyf.edgeList)
            {
                if (le.start == anglePoints[2] || le.end == anglePoints[2]) continue;
                Vector3 v;
                if(MHelperFunctions.LineLineIntersection(out v, le.start.position, le.direction, anglePoints[2].position, dir1))
                {
                    if(Vector3.Dot(le.start.position - v, le.end.position - v) <= 0)
                    {
                        validAngle = true;
                        intersect = v;
                        break;
                    }
                }
            }
            if (validAngle)
            {
                activePoint.SetPosition(intersect);
                activeTextMesh.GetComponentInChildren<TextMesh>().text = MHelperFunctions.CalcRealAngle(dir1, dir2).ToString() + " " + MHelperFunctions.CalcRealAngle(dir1, dir3).ToString();
                activeTextMesh.transform.position = sceneManager.rightControllerPosition + MDefinitions.DEFAULT_ACTIVE_TEXT_OFFSET;
                if (sceneManager.camera != null)
                    activeTextMesh.transform.rotation = Quaternion.LookRotation((sceneManager.camera.transform.position - activeTextMesh.transform.position) * -1, Vector3.up);
                if (activeEdge == null)
                {
                    activeEdge = new MLinearEdge(anglePoints[2], activePoint);
                    activeEdge.entityStatus = MEntity.MEntityStatus.ACTIVE;
                    activeEdge.end.edges.Add(activeEdge);
                }
                activePoint.Render(curObj.localToWorldMatrix);
                if (activeEdge != null && activeEdge.IsValid())
                {
                    activeEdge.Render(curObj.localToWorldMatrix);
                }
            }
        }
        if (!validAngle)
        {
            activeTextMesh.GetComponentInChildren<TextMesh>().text = "";
        }
    }

    private Vector3 ReviseAngle(Vector3 dir1, Vector3 dir2, Vector3 dir3)
    {
        float totalAngle = MHelperFunctions.CalcRadAngle(dir2, dir3);
        float angle1 = MHelperFunctions.CalcRadAngle(dir2, dir1);
        float angle2 = MHelperFunctions.CalcRadAngle(dir3, dir1);
        if(Mathf.Abs(angle1 - angle2) < MDefinitions.AUTO_REVISE_FACTOR) //角平分线
        {
            float angle = totalAngle / 2;
            return MHelperFunctions.CalcDirectionByAngle(dir2, dir3, angle);
        } else if(Mathf.Abs(2 * angle1 - angle2) < MDefinitions.AUTO_REVISE_FACTOR) //三等分线
        {
            float angle = totalAngle / 3;
            return MHelperFunctions.CalcDirectionByAngle(dir2, dir3, angle);
        } else if(Mathf.Abs(2 * angle2 - angle1) < MDefinitions.AUTO_REVISE_FACTOR) //三等分线
        {
            float angle = totalAngle * 2 / 3;
            return MHelperFunctions.CalcDirectionByAngle(dir2, dir3, angle);
        }
        else
        {
            int pieces = 12;
            for(int i = 1; i < pieces - 1; i++)
            {
                if(Mathf.Abs(angle1 - i * Mathf.PI / pieces) < MDefinitions.AUTO_REVISE_FACTOR)
                {
                    float angle = i * Mathf.PI / pieces;
                    return MHelperFunctions.CalcDirectionByAngle(dir2, dir3, angle);
                }
                if (Mathf.Abs(angle2 - i * Mathf.PI / pieces) < MDefinitions.AUTO_REVISE_FACTOR)
                {
                    float angle = i * Mathf.PI / pieces;
                    return MHelperFunctions.CalcDirectionByAngle(dir3, dir2, angle);
                }
            }
        }
        return dir1;
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
            case STATUS.SELECT_FACE:
                if (sceneManager.activeEntity.entity != null && ((MFace)sceneManager.activeEntity.entity).faceType == MFace.MFaceType.POLYGON)
                {
                    status = STATUS.SELECT_POINT;
                    SelectEntity(ref selectFace, sceneManager.activeEntity.entity);
                    curObj = sceneManager.activeEntity.obj;
                }
                break;
            case STATUS.SELECT_POINT:
                MPolygonFace polyf = selectFace as MPolygonFace;
                if(sceneManager.activeEntity.entity == null || !polyf.PointInFace((MPoint)sceneManager.activeEntity.entity))
                {
                    ResetStatus();
                }
                else
                {
                    MPoint p = (MPoint)sceneManager.activeEntity.entity;
                    int i = 0;
                    foreach (MLinearEdge le in polyf.edgeList)
                    {
                        if (le.start == p)
                        {
                            anglePoints[i] = le.end;
                            i++;
                            if (i == 2) break;
                        }
                        else if (le.end == p)
                        {
                            anglePoints[i] = le.start;
                            i++;
                            if (i == 2) break;
                        }
                    }
                    if(i == 2)
                    {
                        status = STATUS.ADJUST_ANGLE;
                        anglePoints[2] = p;
                        activeTextMesh.SetActive(true);
                    }
                    else
                    {
                        Debug.Log("CreateAngleState: wrong refEdges");
                        ResetStatus();
                    }
                }
                break;
            case STATUS.ADJUST_ANGLE:
                if (validAngle)
                {
                    curObj.CreateLinearEdge(anglePoints[2], new MPoint(activePoint.position));
                    ResetStatus();
                }
                break;
        }
    }

    private void ResetStatus()
    {
        status = STATUS.SELECT_FACE;
        SelectEntity(ref selectFace, null);
        anglePoints = null;
        activeTextMesh.SetActive(false);
        activeEdge = null;
        curObj = null;
        validAngle = false;
    }

    private void SelectEntity(ref MEntity oldEntity, MEntity newEntity)
    {
        if(newEntity != oldEntity)
        {
            if (oldEntity != null)
            {
                oldEntity.entityStatus = MEntity.MEntityStatus.DEFAULT;
                oldEntity = null;
            }
            if (newEntity != null)
            {
                newEntity.entityStatus = MEntity.MEntityStatus.SELECT;
            }
            oldEntity = newEntity;
        }
    }
}