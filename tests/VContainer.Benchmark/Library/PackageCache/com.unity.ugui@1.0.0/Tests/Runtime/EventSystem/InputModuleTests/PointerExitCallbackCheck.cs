using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class PointerExitCallbackCheck : MonoBehaviour, IPointerExitHandler
{
    public PointerEventData pointerData { get; private set; }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerData = eventData;
    }
}
