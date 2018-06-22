using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {
    public GameObject leftController;
    public GameObject rightController;

    public PanelMenuListener panelMenuListener;

    public LoadListListener loadListListener;

    public GameObject objTemplate;

    public GameObject statisticActiveMesh;

    public GameObject globalInfoMesh;

    public GameObject quad;

    public GameObject blackboard;


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
            , CONNECT_POINT, CREATE_POINT, CREATE_VERTICAL_LINE, CREATE_ANGLE, LOOP_TO_FACE, REMOVE_ENTITY
            , ADD_PREFAB, ADD_RECTANGLE, ADD_RIGHT_ANGLED_TRI};

	// Use this for initialization
	void Start () {
        objects = new List<MObject>();
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
        if(camera != null && blackboard != null && blackboard.activeSelf)
        {
            blackboard.transform.rotation = Quaternion.LookRotation((camera.transform.position - blackboard.transform.position) * -1, Vector3.up) * Quaternion.Euler(0, 180, 0);
        }
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
        sceneStateMachine.RegisterState(new LoadObjectState(this, panelMenuListener, loadListListener));
        sceneStateMachine.RegisterState(new CreateAngleState(this, statisticActiveMesh));
        sceneStateMachine.RegisterState(new LoopToFaceState(this));
        sceneStateMachine.RegisterState(new AddRectangleState(this, statisticActiveMesh));
        sceneStateMachine.RegisterState(new AddRightAngledTriangleState(this, statisticActiveMesh));
		sceneStateMachine.SwitchState((uint)SceneStatus.ADD_RECTANGLE, null);
    }

    private void BetweenSwitch(IState from, IState to)
    {
        if (from == null || to == null) return;
        string text = "Status Switch: from " + (SceneStatus)from.GetStateID() + " to " + (SceneStatus)to.GetStateID();
        Debug.Log(text);
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

    public void StartRenderFace(Material mat)
    {
        foreach(MObject obj in objects)
        {
            obj.RenderFace(mat);
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
