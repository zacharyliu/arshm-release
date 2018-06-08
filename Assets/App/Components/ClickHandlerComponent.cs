using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickHandlerComponent : MonoBehaviour, IPointerClickHandler
{
	public event Action<PointerEventData> OnClick = delegate { };
	
	public void OnPointerClick(PointerEventData pointerEventData)
	{
		OnClick(pointerEventData);
	}	
}
