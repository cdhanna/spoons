using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Spoons.Behaviours
{
    public class DesktopDraggableBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {

        [Header("Scene References")]
        public Transform DragRoot;
        
        private Vector2 _start;
        
        public void OnBeginDrag(PointerEventData data)
        {
            var t = DragRoot as RectTransform;

            Debug.Log("OnBeginDrag: " + data.position);
            _start = data.position - t.anchoredPosition;
        }

        public void OnDrag(PointerEventData data)
        {
            if (data.dragging)
            {
                var t = DragRoot as RectTransform;
                var p = ((RectTransform)t.parent);
                var next = data.position - _start;

                next.x = Mathf.Clamp(next.x, t.rect.width*.5f, p.rect.width - (t.rect.width*.5f));
                next.y = Mathf.Clamp(next.y, t.rect.height*.5f, p.rect.height - (t.rect.height*.5f));
                
                
                // next.x = Mathf.Clamp(next.x, 0, p.rect.width);
                // next.y = Mathf.Clamp(next.y, 0, p.rect.height);
            
                //
                // if (next.x > p.rect.width)
                // {
                //     next.x = p.rect.width;
                // }
                t.anchoredPosition = next;
                // timeCount += Time.deltaTime;
                // if (timeCount > 1.0f)
                // {
                //     Debug.Log("Dragging:" + data.position);
                //     timeCount = 0.0f;
                // }
            }
        }

        public void OnEndDrag(PointerEventData data)
        {
            Debug.Log("OnEndDrag: " + data.position);
        }
    }
}