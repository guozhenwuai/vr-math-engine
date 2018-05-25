using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {
    public GameObject leftController;
    public GameObject rightController;

    public TextMesh textMesh;

    public TextMesh statisticText;

    public TextMesh relationText;

    private List<MObject> objects;

    private MEntity activeEntity;

    private List<MEntity> selectedEntity;

    private MObject activeObject;

	private MObject selectedObject;

    private MObject.MInteractMode interactMode;

    private enum SceneStatus { DISPLAY, SELECT_REFEDGE, CREATE_LINE, SHOW_RELATION};

    private enum SelectType { ALL, NO_POINT, LINEAR_EDGE, POINT};

    private SceneStatus sceneStatus;

	// Use this for initialization
	void Start () {
        objects = new List<MObject>();
        selectedEntity = new List<MEntity>();
        interactMode = MObject.MInteractMode.ALL;
        AddPrefabObject(MObject.MPrefabType.CUBE);
        sceneStatus = SceneStatus.DISPLAY;
        textMesh.text = "展示线段长度和表面积";
        statisticText.text = "";
        relationText.text = "";
	}
	
	// Update is called once per frame
	void Update () {
        UpdateHighlight();
        StartRender();
        UpdateText();
	}

    public void HandleTriggerClicked(InteractionHandler.ControllerType type, VRTK.ControllerInteractionEventArgs e)
    {
        if(type == InteractionHandler.ControllerType.LEFT)
        {
            SwitchSceneStatus();
            return;
        }
        switch (sceneStatus)
        {
            case SceneStatus.DISPLAY:
                SelectActiveEntity(SelectType.NO_POINT, -1);
                break;
		    case SceneStatus.SELECT_REFEDGE:
			    SelectActiveEntity (SelectType.LINEAR_EDGE, 1);
			    if (selectedEntity.Count == 1)
				    selectedObject.SetRefEdge ((MLinearEdge)(selectedEntity[0]));
				else if(selectedEntity.Count == 0)selectedObject.SetRefEdge(null);
                break;
		    case SceneStatus.CREATE_LINE:
                SelectActiveEntity(SelectType.POINT, 2);
                if (selectedEntity.Count == 2) {
				    selectedObject.CreateLinearEdge ((MPoint)(selectedEntity[0]), (MPoint)(selectedEntity[1]));
                    ClearSelectedEntity();
			    }
			    break;
            case SceneStatus.SHOW_RELATION:
                SelectActiveEntity(SelectType.ALL, 2);
                UpdateRelationText();
                break;
            default:
                Debug.Log("SceneManager: HandleTriggerClicked: Unhandled scene status " + sceneStatus);
                break;
        }
    }

    private void SwitchSceneStatus()
    {
        switch (sceneStatus)
        {
		    case SceneStatus.DISPLAY:
			    sceneStatus = SceneStatus.SELECT_REFEDGE;
			    textMesh.text = "设置基准边";
			    ResetScene();
                break;
            case SceneStatus.SELECT_REFEDGE:
                sceneStatus = SceneStatus.CREATE_LINE;
                textMesh.text = "连点成线";
			    ResetScene();
                break;
            case SceneStatus.CREATE_LINE:
                sceneStatus = SceneStatus.SHOW_RELATION;
                textMesh.text = "展示点线面的相对关系";
                ResetScene();
                break;
            case SceneStatus.SHOW_RELATION:
                sceneStatus = SceneStatus.DISPLAY;
                textMesh.text = "展示线段长度和表面积";
			    ResetScene();
                break;
            default:
                Debug.Log("SceneManager: Unhandled scene status: " + sceneStatus);
                break;
        }
    }

	private void ResetScene(){
		ClearSelectedEntity();
		statisticText.text = "";
        relationText.text = "";
	}

	private void ClearSelectedEntity()
	{
		foreach (MEntity entity in selectedEntity) {
			if (entity.entityStatus == MEntity.MEntityStatus.SELECT) {
				entity.entityStatus = MEntity.MEntityStatus.DEFAULT;
			}
		}
		selectedEntity.Clear();
	}

    private void UpdateText()
    {
        if(sceneStatus == SceneStatus.DISPLAY)
        {
            string text = "";
            if(activeEntity != null)
            {
                switch (activeEntity.entityType)
                {
                    case MEntity.MEntityType.EDGE:
					text += "该边长度：" + activeObject.GetEdgeLength(((MEdge)activeEntity)) + "\n";
                        break;
                    case MEntity.MEntityType.FACE:
					text += "该面面积： " + activeObject.GetFaceSurface(((MFace)activeEntity)) + "\n";
                        break;
                }
            }
			if (activeObject != null && activeObject == selectedObject) {
				float totalLength = 0;
				float totalSurface = 0;
				foreach(MEntity entity in selectedEntity)
				{
					switch (entity.entityType)
					{
					    case MEntity.MEntityType.EDGE:
						    totalLength += activeObject.GetEdgeLength(((MEdge)entity));
						    break;
					    case MEntity.MEntityType.FACE:
						    totalSurface += activeObject.GetFaceSurface(((MFace)entity));
						    break;
					}
				}
				text += "总长度： " + totalLength + "\n";
				text += "总面积： " + totalSurface + "\n";
			}
            statisticText.text = text;
        }
    }

    private void UpdateRelationText()
    {
        if(sceneStatus == SceneStatus.SHOW_RELATION)
        {
            if(selectedEntity.Count == 2)
            {
                MRelation relation = selectedObject.getEntityRelation(selectedEntity[0], selectedEntity[1]);
                if(relation != null)
                {
                    string text = "";
                    switch (relation.relationType)
                    {
                        case MRelation.EntityRelationType.POINT_POINT:
                            text += "两点距离： " + relation.distance + "\n";
                            break;
                        case MRelation.EntityRelationType.POINT_EDGE:
                            if(MHelperFunctions.FloatEqual(relation.distance, 0))
                            {
                                text += "点在直线上\n";
                            } else
                            {
                                text += "点到直线距离： " + relation.distance + "\n";
                            }
                            break;
                        case MRelation.EntityRelationType.POINT_FACE:
                            if (MHelperFunctions.FloatEqual(relation.distance, 0))
                            {
                                text += "点在平面上\n";
                            }
                            else
                            {
                                text += "点到平面距离： " + relation.distance + "\n";
                            }
                            break;
                        case MRelation.EntityRelationType.EDGE_EDGE:
                            if (MHelperFunctions.FloatEqual(relation.distance, 0) || MHelperFunctions.FloatEqual(relation.angle, 0)){
                                text += "两线共面\n";
                            }
                            if(MHelperFunctions.FloatEqual(relation.angle, 0))
                            {
                                text += "两线平行\n";
                            } else if(MHelperFunctions.FloatEqual(relation.angle, 90))
                            {
                                text += "两线垂直\n";
                            } else
                            {
                                text += "两线夹角：" + relation.angle + "\n";
                            }
                            if(!MHelperFunctions.FloatEqual(relation.distance, 0))
                            {
                                text += "两线距离： " + relation.distance + "\n";
                            }
                            break;
                        case MRelation.EntityRelationType.EDGE_FACE:
                            if (MHelperFunctions.FloatEqual(relation.distance, 0))
                            {
                                text += "线在面上\n";
                            } else if(MHelperFunctions.FloatEqual(relation.distance, -1))
                            {
                                text += "线面相交\n";
                                
                            }
                            if(!MHelperFunctions.FloatEqual(relation.distance, 0))
                            {
                                if (MHelperFunctions.FloatEqual(relation.angle, 0))
                                {
                                    text += "线面平行\n";
                                    text += "线面距离：" + relation.distance + "\n";
                                }
                                else if (MHelperFunctions.FloatEqual(relation.angle, 90))
                                {
                                    text += "线面垂直\n";
                                }
                                else
                                {
                                    text += "线面夹角：" + relation.angle + "\n";
                                }
                            }
                            break;
                        case MRelation.EntityRelationType.FACE_FACE:
                            if(MHelperFunctions.FloatEqual(relation.angle, 0))
                            {
                                text += "面面平行\n";
                                text += "面面距离：" + relation.distance + "\n";
                            } else if(MHelperFunctions.FloatEqual(relation.distance, 90))
                            {
                                text += "面面垂直\n";
                            } else
                            {
                                text += "面面夹角： " + relation.angle + "\n";
                            }
                            break;
                    }
                    relationText.text = text;
                }
            }
        }
    }

    private void AddPrefabObject(MObject.MPrefabType type)
    {
        MObject obj = new MObject(type);
        objects.Add(obj);
    }

	private void SelectActiveEntity(SelectType type, int maxSelect)
    {
        if(activeEntity != null)
        {
            switch (type)
            {
                case SelectType.NO_POINT:
                    if (activeEntity.entityType == MEntity.MEntityType.POINT) return;
                    break;
                case SelectType.LINEAR_EDGE:
                    if (!(activeEntity.entityType == MEntity.MEntityType.EDGE) || !(((MEdge)activeEntity).edgeType == MEdge.MEdgeType.LINEAR)) return;
                    break;
				case SelectType.ALL:
					break;
			    case SelectType.POINT:
				    if (activeEntity.entityType != MEntity.MEntityType.POINT) return;
				    break;
            }
			if (selectedObject != null && activeObject != selectedObject) {
				ClearSelectedEntity ();
			}
			selectedObject = activeObject;
			if (selectedEntity.Contains(activeEntity))
            {
				UnselectEntity(activeEntity);
				activeEntity.entityStatus = MEntity.MEntityStatus.ACTIVE;
            }
            else
            {
				if (maxSelect > 0) {
					if (selectedEntity.Count == maxSelect) {
						UnselectEntity (selectedEntity [0]);
					}
				}
				SelectEntity(activeEntity);
            }
        }
    }

	private void UnselectEntity(MEntity entity){
		if (entity.entityStatus == MEntity.MEntityStatus.SELECT) {
			entity.entityStatus = MEntity.MEntityStatus.DEFAULT;
		}
		selectedEntity.Remove (entity);
	}

	private void SelectEntity(MEntity entity){
		if (entity.entityStatus == MEntity.MEntityStatus.ACTIVE || entity.entityStatus == MEntity.MEntityStatus.SPECIAL) {
			if (entity.entityStatus == MEntity.MEntityStatus.ACTIVE) entity.entityStatus = MEntity.MEntityStatus.SELECT;
			selectedEntity.Add(entity);
		}
	}

    private void UpdateHighlight()
    {
        MEntity e = GetAvailEntity(rightController.transform.position, interactMode);
        if (activeEntity != null && activeEntity.entityStatus == MEntity.MEntityStatus.ACTIVE)
        {
            activeEntity.entityStatus = MEntity.MEntityStatus.DEFAULT;
        }
        if (e != null && e.entityStatus == MEntity.MEntityStatus.DEFAULT)
        {
            e.entityStatus = MEntity.MEntityStatus.ACTIVE;
        }
        activeEntity = e;
    }

    private void StartRender()
    {
        foreach(MObject obj in objects)
        {
            obj.Render();
        }
    }

    private MEntity GetAvailEntity(Vector3 pos, MObject.MInteractMode mode)
    {
		activeObject = null;
        MEntity entity = null;
        MEntity temp;
        float dis = float.MaxValue;
        float f;
        foreach(MObject obj in objects)
        {
            if (obj.HitObject(pos))
            {
                if((f = obj.Response(out temp, pos, mode)) < dis)
                {
					activeObject = obj;
                    dis = f;
                    entity = temp;
                }
            }
        }
        return entity;
    }
}
