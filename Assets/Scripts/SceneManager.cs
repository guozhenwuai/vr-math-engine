using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {
    public GameObject leftController;
    public GameObject rightController;

    public GameObject objTemplate;

    public GameObject statisticActiveMesh;

    public GameObject globalInfoMesh;

    public GameObject statusMesh;

    public GameObject quad;

    [HideInInspector]
    public VRTK.VRTK_ControllerEvents leftEvents
    {
        get
        {
            return leftController.GetComponent<VRTK.VRTK_ControllerEvents>();
        }
    }

    [HideInInspector]
    public VRTK.VRTK_ControllerEvents rightEvents
    {
        get
        {
            return rightController.GetComponent<VRTK.VRTK_ControllerEvents>();
        }
    }

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

    [HideInInspector]
    public bool pointerOnMenu = false;

    public enum SceneStatus { STATISTIC_DISPLAY, SELECT_REFEDGE, RELATION_DISPLAY
            , TRANSFORM, SAVE_OBJECT, LOAD_OBJECT, REMOVE_OBJECT, OBJECT_CUTTING
            , CONNECT_POINT, CREATE_POINT, CREATE_VERTICAL_LINE, REMOVE_ENTITY
            , ADD_PREFAB};

	// Use this for initialization
	void Start () {
        objects = new List<MObject>();
        AddPrefabObject(MObject.MPrefabType.CYLINDER);
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
        sceneStateMachine.BetweenSwitchStateCallBack += new StateMachine.BetweenSwitchState(BetweenSwitch);
        sceneStateMachine.RegisterState(new StatisticDisplayState(this, statisticActiveMesh));
        sceneStateMachine.RegisterState(new RefEdgeState(this));
        sceneStateMachine.RegisterState(new TransformState(this));
        sceneStateMachine.RegisterState(new ConnectPointState(this));
        sceneStateMachine.RegisterState(new RelationDisplayState(this, globalInfoMesh));
        sceneStateMachine.RegisterState(new SaveObjectState(this));
        sceneStateMachine.RegisterState(new AddPrefabState(this));
        sceneStateMachine.RegisterState(new RemoveObjectState(this));
        sceneStateMachine.RegisterState(new CreatePointState(this));
        sceneStateMachine.RegisterState(new CreateVerticalLineState(this, statisticActiveMesh));
        sceneStateMachine.RegisterState(new RemoveEntityState(this));
        sceneStateMachine.RegisterState(new ObjectCuttingState(this, quad));
		sceneStateMachine.SwitchState((uint)SceneStatus.OBJECT_CUTTING, null);
    }

    private void BetweenSwitch(IState from, IState to)
    {
        if (from == null || to == null) return;
        string text = "Status Switch: from " + (SceneStatus)from.GetStateID() + " to " + (SceneStatus)to.GetStateID();
        statusMesh.GetComponent<TextMesh>().text = text;
        Debug.Log(text);
    }

    private void AddPrefabObject(MObject.MPrefabType type)
    {
        MObject obj = new MObject(objTemplate, type);
        objects.Add(obj);
    }

    public void UpdateEntityHighlight(MObject.MInteractMode interactMode)
    {
        MEntityPair e = GetAvailEntity(rightControllerPosition, interactMode);
        if(activeEntity.entity != e.entity)
        {
            UnhighlightEntity(activeEntity.entity);
            HighlightEntity(e.entity);
        }
        activeEntity = e;
    }

    public void UpdateEntityHighlight(MObject.MInteractMode interactMode, MObject obj)
    {
        MEntity entity = null;
        if (obj.HitObject(rightControllerPosition))
        {
            obj.Response(out entity, rightControllerPosition, interactMode);
        }
        if (activeEntity.entity != entity)
        {
            UnhighlightEntity(activeEntity.entity);
            HighlightEntity(entity);
        }
        if(entity == null)
        {
            activeEntity = new MEntityPair(null, null);
        }
        else
        {
            activeEntity = new MEntityPair(entity, obj);
        }
    }

    public void UpdateObjectHighlight()
    {
        foreach(MObject obj in objects)
        {
            if (obj.HitObject(rightControllerPosition))
            {
                if(activeEntity.obj != obj)
                {
                    if(activeEntity.obj != null && activeEntity.obj.objectState == MObject.MObjectState.ACTIVE)
                    {
                        activeEntity.obj.ResetStatus();
                    }
                    if(obj.objectState == MObject.MObjectState.DEFAULT)obj.Highlight();
                    activeEntity = new MEntityPair(null, obj);
                }
                return;
            }
        }
        if (activeEntity.obj != null)
        {
			if(activeEntity.obj.objectState == MObject.MObjectState.ACTIVE)
				activeEntity.obj.ResetStatus();
            activeEntity = new MEntityPair(null, null);
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
