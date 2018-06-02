using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPointerListener : MonoBehaviour
{
    public SceneManager sceneManager;

    // Use this for initialization
    void Start()
    {
        if (GetComponent<VRTK.VRTK_UIPointer>())
        {
            GetComponent<VRTK.VRTK_UIPointer>().UIPointerElementEnter += UIPointerElementEnter;
            GetComponent<VRTK.VRTK_UIPointer>().UIPointerElementExit += UIPointerElementExit;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void UIPointerElementEnter(object sender, VRTK.UIPointerEventArgs e)
    {
        if (GetComponent<VRTK.VRTK_Pointer>())
        {
            sceneManager.pointerOnMenu = true;
            GetComponent<VRTK.VRTK_Pointer>().Toggle(true);
        }
    }

    private void UIPointerElementExit(object sender, VRTK.UIPointerEventArgs e)
    {
        if (GetComponent<VRTK.VRTK_Pointer>())
        {
            sceneManager.pointerOnMenu = false;
            GetComponent<VRTK.VRTK_Pointer>().Toggle(false);
        }
    }
}