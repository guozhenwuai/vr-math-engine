using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : VRTK.VRTK_UIDraggableItem
{
    public override void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        dragTransform = null;
        transform.position += (transform.forward * forwardOffset);
        bool validDragEnd = true;
        if (restrictToDropZone)
        {
            if (validDropZone != null && validDropZone != startDropZone)
            {
                transform.SetParent(validDropZone.transform);
            }
            else
            {
                ResetElement();
                validDragEnd = false;
            }
        }

        Canvas destinationCanvas = (eventData.pointerEnter != null ? eventData.pointerEnter.GetComponentInParent<Canvas>() : null);
        if (restrictToOriginalCanvas)
        {
            if (destinationCanvas != null && destinationCanvas != startCanvas)
            {
                ResetElement();
                validDragEnd = false;
            }
        }

        if (destinationCanvas == null)
        {
            //We've been dropped off of a canvas
            ResetElement();
            validDragEnd = false;
            Destroy(gameObject);
        }

        if (validDragEnd)
        {
            VRTK.VRTK_UIPointer pointer = GetPointer(eventData);
            if (pointer != null)
            {
                pointer.OnUIPointerElementDragEnd(pointer.SetUIPointerEvent(pointer.pointerEventData.pointerPressRaycast, gameObject));
            }
            OnDraggableItemDropped(SetEventPayload(validDropZone));
        }

        validDropZone = null;
        startParent = null;
        startCanvas = null;
    }
}