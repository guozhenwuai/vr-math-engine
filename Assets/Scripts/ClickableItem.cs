using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClickableItem : MonoBehaviour, IPointerClickHandler, IPointerUpHandler, IPointerDownHandler
{
    public GameObject blackboardDropZone;

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        GameObject newObj = Instantiate(gameObject);
        Destroy(newObj.GetComponent<ClickableItem>());
        newObj.transform.SetParent(blackboardDropZone.transform);
        newObj.transform.localPosition = Vector3.zero;
        newObj.transform.localEulerAngles = Vector3.zero;
        newObj.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        newObj.AddComponent<DraggableItem>();
        Debug.Log("on pointer click");
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        
    }
}