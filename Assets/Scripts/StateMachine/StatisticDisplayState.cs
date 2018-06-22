using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatisticDisplayState : IState
{
    private SceneManager sceneManager;

    private GameObject activeTextMesh;

    private List<MEntityPair> selectedEntity;

    private float[] selectLengthCount;

    private float[] selectSurfaceCount;

    private float[] objectSurface;

    private float[] objectVolumn;

    private VRTK.ControllerInteractionEventHandler rightTriggerPressed;

    public StatisticDisplayState(SceneManager sceneManager, GameObject activeTextMesh)
    {
        this.sceneManager = sceneManager;
        this.activeTextMesh = activeTextMesh;
        selectedEntity = new List<MEntityPair>();
        rightTriggerPressed = new VRTK.ControllerInteractionEventHandler(RightTriggerPressed);
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.STATISTIC_DISPLAY;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        sceneManager.rightEvents.TriggerPressed += rightTriggerPressed;
        foreach(MObject obj in sceneManager.objects)
        {
            MLinearEdge refEdge = obj.refEdge;
            if(refEdge != null)
            {
                refEdge.entityStatus = MEntity.MEntityStatus.SPECIAL;
            }
            obj.ActiveTextMesh();
        }
        selectLengthCount = new float[sceneManager.objects.Count];
        selectSurfaceCount = new float[sceneManager.objects.Count];
        objectSurface = new float[sceneManager.objects.Count];
        CalcSurfaceAndVolumn();
        UpdateTextPlane();
    }

    public void OnLeave(IState nextState)
    {
        sceneManager.rightEvents.TriggerPressed -= rightTriggerPressed;
        ClearSelectedEntity();
        activeTextMesh.SetActive(false);
        foreach (MObject obj in sceneManager.objects)
        {
            MLinearEdge refEdge = obj.refEdge;
            if (refEdge != null)
            {
                refEdge.entityStatus = MEntity.MEntityStatus.DEFAULT;
            }
            obj.InactiveTextMesh();
        }
    }

    public void OnUpdate()
    {
        sceneManager.UpdateEntityHighlight(MObject.MInteractMode.ALL);
        UpdateActiveTextMesh();
        if(sceneManager.camera != null)UpdateObjectTextMesh();
        sceneManager.StartRender();
    }

    private void UpdateTextPlane()
    {
        for(int i = 0; i < sceneManager.objects.Count; i++)
        {
            string text = "";
            text += "Select Length: " + selectLengthCount[i] + "\n";
            text += "Select Surface: " + selectSurfaceCount[i] + "\n";
            text += "Object Surface: " + objectSurface[i] + "\n";
            sceneManager.objects[i].SetMeshText(text);
        }
    }

    private void CalcSurfaceAndVolumn()
    {
        for(int i = 0; i < sceneManager.objects.Count; i++)
        {
            objectSurface[i] = sceneManager.objects[i].GetSurfaceSum();
        }
    }

    private void UpdateObjectTextMesh()
    {
        Vector3 pos = sceneManager.camera.transform.position;
        foreach (MObject obj in sceneManager.objects)
        {
            obj.RotateTextMesh(pos);
        }
    }

    private void UpdateActiveTextMesh()
    {
        if(sceneManager.activeEntity.entity == null || sceneManager.activeEntity.entity.entityType == MEntity.MEntityType.POINT)
        {
            activeTextMesh.SetActive(false);
        } else
        {
            string text = "";
            switch (sceneManager.activeEntity.entity.entityType)
            {
                case MEntity.MEntityType.EDGE:
                    text = "Length: " + sceneManager.activeEntity.obj.GetEdgeLength((MEdge)(sceneManager.activeEntity.entity));
                    break;
                case MEntity.MEntityType.FACE:
                    text = "Surface: " + sceneManager.activeEntity.obj.GetFaceSurface((MFace)(sceneManager.activeEntity.entity));
                    break;
            }
            activeTextMesh.GetComponentInChildren<TextMesh>().text = text;
            activeTextMesh.transform.position = sceneManager.rightControllerPosition + MDefinitions.DEFAULT_ACTIVE_TEXT_OFFSET;
            if (sceneManager.camera != null)
                activeTextMesh.transform.rotation = Quaternion.LookRotation((sceneManager.camera.transform.position - activeTextMesh.transform.position) * -1, Vector3.up);
            activeTextMesh.SetActive(true);
        }
    }

    private void RightTriggerPressed(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if(sceneManager.activeEntity.entity != null && sceneManager.activeEntity.entity.entityType != MEntity.MEntityType.POINT)
        {
            SelectEntity(sceneManager.activeEntity);
            UpdateTextPlane();
        }
    }

    private void SelectEntity(MEntityPair entityPair)
    {
        if (selectedEntity.Contains(entityPair))
        {
            RemoveEntity(entityPair);
        } else
        {
            AddEntity(entityPair);
        }
    }

    private void RemoveEntity(MEntityPair entityPair)
    {
        MEntity entity = entityPair.entity;
        MObject obj = entityPair.obj;
        if (entity.entityStatus == MEntity.MEntityStatus.SELECT)
        {
            entity.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
        switch (entity.entityType)
        {
            case MEntity.MEntityType.EDGE:
                selectLengthCount[sceneManager.objects.IndexOf(obj)] -= obj.GetEdgeLength((MEdge)entity);
                break;
            case MEntity.MEntityType.FACE:
                selectSurfaceCount[sceneManager.objects.IndexOf(obj)] -= obj.GetFaceSurface((MFace)entity);
                break;
        }
        selectedEntity.Remove(entityPair);
    }

    private void AddEntity(MEntityPair entityPair)
    {
        MEntity entity = entityPair.entity;
        MObject obj = entityPair.obj;
        if (entityPair.entity.entityStatus != MEntity.MEntityStatus.SPECIAL)
        {
            entityPair.entity.entityStatus = MEntity.MEntityStatus.SELECT;
        }
        switch (entity.entityType)
        {
            case MEntity.MEntityType.EDGE:
                selectLengthCount[sceneManager.objects.IndexOf(obj)] += obj.GetEdgeLength((MEdge)entity);
                break;
            case MEntity.MEntityType.FACE:
                selectSurfaceCount[sceneManager.objects.IndexOf(obj)] += obj.GetFaceSurface((MFace)entity);
                break;
        }
        selectedEntity.Add(entityPair);
    }

    private void ClearSelectedEntity()
    {
        foreach (MEntityPair pair in selectedEntity)
        {
            pair.entity.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
        selectedEntity.Clear();
    }
}