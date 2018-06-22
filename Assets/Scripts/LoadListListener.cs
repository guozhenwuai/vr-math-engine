using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LoadListListener : MonoBehaviour
{
    public SceneManager sceneManager;

    public GameObject loadList;

    public List<GameObject> listItems;

    public Text pageText;

    private float lastX;

    private int curPage;

    private int totalPage;

    private List<string> objects;

    private VRTK.ControllerInteractionEventHandler leftTouchpadTouchStart;

    private VRTK.ControllerInteractionEventHandler leftTouchpadTouchEnd;


    void Start()
    {
        curPage = 0;
        leftTouchpadTouchStart = new VRTK.ControllerInteractionEventHandler(LeftTouchpadTouchStart);
        leftTouchpadTouchEnd = new VRTK.ControllerInteractionEventHandler(LeftTouchpadTouchEnd);
    }

    void Update()
    {
    }

    public void AttachTouchpadEventHandler()
    {
        sceneManager.leftEvents.TouchpadTouchStart += leftTouchpadTouchStart;
        sceneManager.leftEvents.TouchpadTouchEnd += leftTouchpadTouchEnd;
    }

    public void DetachTouchpadEventHandler()
    {
        sceneManager.leftEvents.TouchpadTouchStart -= leftTouchpadTouchStart;
        sceneManager.leftEvents.TouchpadTouchEnd -= leftTouchpadTouchEnd;
    }

    private void LeftTouchpadTouchStart(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        lastX = e.touchpadAxis.x;
    }

    private void LeftTouchpadTouchEnd(object sender, VRTK.ControllerInteractionEventArgs e)
    {
        if (e.touchpadAxis.x - lastX >= MDefinitions.TOUCHPAD_AXIS_CHANGE_THRESHOLD)
        {
            PrevPage();
        }
        else if (lastX - e.touchpadAxis.x >= MDefinitions.TOUCHPAD_AXIS_CHANGE_THRESHOLD)
        {
            NextPage();
        }
    }

    private void PrevPage()
    {
        if (curPage > 0)
        {
            curPage--;
            UpdateList();
        }
    }

    private void NextPage()
    {
        if (curPage < totalPage - 1)
        {
            curPage++;
            UpdateList();
        }
    }

    public void ResetObjects(List<string> objects)
    {
        this.objects = objects;
        curPage = 0;
        totalPage = Mathf.CeilToInt(((float)objects.Count) / listItems.Count);
    }

    public void UpdateList()
    {
        int index = curPage * listItems.Count;
        int i;
        for(i = index; i < index + listItems.Count && i < objects.Count; i++)
        {
            GameObject item = listItems[i - index];
			item.SetActive(true);
            item.GetComponentInChildren<Text>().text = DateTime.FromFileTime(Convert.ToInt64(objects[i])).ToString();
            Button button = item.GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            string name = objects[i];
            button.onClick.AddListener(delegate ()
            {
                MObject obj = new MObject(sceneManager.objTemplate, name);
                sceneManager.objects.Add(obj);
            });
        }
        for(; i < index + listItems.Count; i++)
        {
            listItems[i - index].SetActive(false);
        }
        pageText.text = (totalPage == 0? "0":(curPage + 1).ToString()) + "/" + totalPage.ToString();
    }

}