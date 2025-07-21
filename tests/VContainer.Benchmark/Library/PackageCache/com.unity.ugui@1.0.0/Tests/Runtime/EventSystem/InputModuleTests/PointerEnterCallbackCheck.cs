using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class PointerEnterCallbackCheck : MonoBehaviour, IPointerEnterHandler
{
    public PointerEventData pointerData { get; private set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerData = eventData;
    }
}
