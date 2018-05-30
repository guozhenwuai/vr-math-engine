﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {
    public GameObject leftController;
    public GameObject rightController;

    public GameObject objTemplate;

    public GameObject statisticActiveMesh;

    public GameObject globalInfoMesh;

    [HideInInspector]
    public VRTK.VRTK_ControllerEvents leftEvents;

    [HideInInspector]
    public VRTK.VRTK_ControllerEvents rightEvents;

    [HideInInspector]
    public Vector3 leftControllerPosition {
        get
        {
            return leftController.transform.position;
        }
    }

    [HideInInspector]
    public Vector3 rightControllerPosition
    {
        get
        {
            return rightController.transform.position;
        }
    }

    [HideInInspector]
    public List<MObject> objects;

    [HideInInspector]
    public MEntityPair activeEntity;

    [HideInInspector]
    public StateMachine sceneStateMachine;

    [HideInInspector]
    public GameObject camera;
    

    public enum SceneStatus { STATISTIC_DISPLAY, SELECT_REFEDGE, RELATION_DISPLAY
            , TRANSFORM, EXPORT_OBJECT, REMOVE_OBJECT
            , CONNECT_POINT
            , ADD_PREFAB_CUBE};

	// Use this for initialization
	void Start () {
        objects = new List<MObject>();
        AddPrefabObject(MObject.MPrefabType.CUBE);
        leftEvents = leftController.GetComponent<VRTK.VRTK_ControllerEvents>();
        rightEvents = rightController.GetComponent<VRTK.VRTK_ControllerEvents>();
        InitStateMachine();
	}
	
	// Update is called once per frame
	void Update () {
        if(camera == null)
        {
            if(VRTK.VRTK_DeviceFinder.HeadsetTransform() != null)
            {
                camera = VRTK.VRTK_DeviceFinder.HeadsetTransform().gameObject;
            }
        }
        sceneStateMachine.OnUpdate();
	}

    private void InitStateMachine()
    {
        sceneStateMachine = new StateMachine();
        sceneStateMachine.RegisterState(new StatisticDisplayState(this, statisticActiveMesh));
        sceneStateMachine.RegisterState(new RefEdgeState(this));
        sceneStateMachine.RegisterState(new TransformState(this));
        sceneStateMachine.RegisterState(new ConnectPointState(this));
        sceneStateMachine.RegisterState(new RelationDisplayState(this, globalInfoMesh));
        sceneStateMachine.RegisterState(new ExportObjectState(this));
        sceneStateMachine.RegisterState(new AddPrefabCubeState(this));
        sceneStateMachine.RegisterState(new RemoveObjectState(this));
        sceneStateMachine.SwitchState((uint)SceneStatus.STATISTIC_DISPLAY);
    }

    private void AddPrefabObject(MObject.MPrefabType type)
    {
        MObject obj = new MObject(objTemplate, type);
        objects.Add(obj);
    }

    public void UpdateEntityHighlight(MObject.MInteractMode interactMode)
    {
        MEntityPair e = GetAvailEntity(rightControllerPosition, interactMode);
        if(activeEntity.entity == null || activeEntity.entity != e.entity)
        {
            UnhighlightEntity(activeEntity.entity);
            HighlightEntity(e.entity);
        }
        activeEntity = e;
    }

    public void UpdateObjectHighlight()
    {
        foreach(MObject obj in objects)
        {
            if (obj.HitObject(rightControllerPosition))
            {
                if(activeEntity.obj != obj)
                {
                    if(activeEntity.obj != null)
                    {
                        activeEntity.obj.ResetStatus();
                    }
                    obj.Highlight();
                    activeEntity = new MEntityPair(null, obj);
                }
                return;
            }
        }
        if (activeEntity.obj != null)
        {
            activeEntity.obj.ResetStatus();
            activeEntity.obj = null;
        }
    }

    public void StartRender()
    {
        foreach (MObject obj in objects)
        {
            obj.Render();
        }
    }

    private void UnhighlightEntity(MEntity entity)
    {
        if (entity != null && entity.entityStatus == MEntity.MEntityStatus.ACTIVE)
        {
            entity.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
    }

    private void HighlightEntity(MEntity entity)
    {
        if (entity != null && entity.entityStatus == MEntity.MEntityStatus.DEFAULT)
        {
            entity.entityStatus = MEntity.MEntityStatus.ACTIVE;
        }
    }

    private MEntityPair GetAvailEntity(Vector3 pos, MObject.MInteractMode mode)
    {
        MEntity entity = null;
        MObject activeObj = null;
        MEntity temp;
        float dis = float.MaxValue;
        float f;
        foreach(MObject obj in objects)
        {
            if (obj.HitObject(pos))
            {
                if((f = obj.Response(out temp, pos, mode)) < dis)
                {
					activeObj = obj;
                    dis = f;
                    entity = temp;
                }
            }
        }
        return new MEntityPair(entity, activeObj);
    }
}
