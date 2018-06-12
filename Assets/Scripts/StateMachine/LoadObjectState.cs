using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class LoadObjectState : IState
{
    private SceneManager sceneManager;

    private PanelMenuListener panelMenuListener;

    private LoadListListener loadListListener;

    public LoadObjectState(SceneManager sceneManager, PanelMenuListener panelMenuListener, LoadListListener loadListListener)
    {
        this.sceneManager = sceneManager;
        this.panelMenuListener = panelMenuListener;
        this.loadListListener = loadListListener;
    }

    public uint GetStateID()
    {
        return (uint)SceneManager.SceneStatus.LOAD_OBJECT;
    }

    public void OnEnter(StateMachine machine, IState prevState, object param)
    {
        loadListListener.loadList.SetActive(true);
        panelMenuListener.controllerMenu.SetActive(false);
        panelMenuListener.DetachTouchpadEventHandler();
        loadListListener.AttachTouchpadEventHandler();
        LoadObjects();
        loadListListener.UpdateList();
    }

    public void OnLeave(IState nextState)
    {
        loadListListener.loadList.SetActive(false);
        panelMenuListener.controllerMenu.SetActive(true);
        panelMenuListener.AttachTouchpadEventHandler();
        loadListListener.AttachTouchpadEventHandler();
    }

    public void OnUpdate()
    {
        sceneManager.StartRender();
    }

    private void LoadObjects()
    {
        List<string> objects = new List<string>();
        FileInfo[] files = new DirectoryInfo(MDefinitions.SAVE_PATH).GetFiles();
        foreach(FileInfo f in files)
        {
            try
            {
                string name = Path.GetFileNameWithoutExtension(f.FullName);
                DateTime.FromFileTime(Convert.ToInt64(name));
                if(!objects.Contains(name))objects.Add(name);
            }
            catch(Exception e)
            {
            }
        }
        loadListListener.ResetObjects(objects);
    }
}