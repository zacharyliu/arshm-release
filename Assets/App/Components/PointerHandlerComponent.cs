using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace App
{
    public class PointerHandlerComponent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public event EventHandler<PointerEventData> OnPointerDownHandler;
        public event EventHandler<PointerEventData> OnPointerUpHandler;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDownHandler?.Invoke(this, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnPointerUpHandler?.Invoke(this, eventData);
        }
    }
}
