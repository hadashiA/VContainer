using UnityEngine;
using UnityEngine.EventSystems;

public class DragCallbackCheck : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler
{
    private bool loggedOnDrag = false;
    public bool onBeginDragCalled = false;
    public bool onDragCalled = false;
    public bool onEndDragCalled = false;
    public bool onDropCalled = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        onBeginDragCalled = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (loggedOnDrag)
            return;

        loggedOnDrag = true;
        onDragCalled = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        onEndDragCalled = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        onDropCalled = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Empty to ensure we get the drop if we have a pointer handle as well.
    }
}
