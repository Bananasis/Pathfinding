using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Grid
{
    public class PlaneEventsHandler : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler,
        IDragHandler
    {
        public readonly UnityEvent<Vector2Int> OnTileInteract = new();

        private const float _planeScale = 0.1f;
        private Vector2Int _size;
        private Vector2Int _lastGridInteraction;
        private bool _hasLastGridInteraction;

        public void Init(Vector2Int size)
        {
            _size = size;
            var transform1 = transform;
            var localSize = new Vector3(_size.x, 1, _size.y);
            var localPos = new Vector3(_size.x, 0, _size.y) / 2;
            transform1.localScale = localSize;
            transform1.localPosition = localPos / _planeScale;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                OnTileInteract.Invoke(GetGridPos(eventData));
        }

        private Vector2Int GetGridPos(PointerEventData eventData)
        {
            var worldPos = eventData.pointerCurrentRaycast.worldPosition;
            return new Vector2Int((int)worldPos.x, (int)worldPos.z);
        }


        private void TryHandleNewPosition(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            var newInteractionPosition = GetGridPos(eventData);
            if (_hasLastGridInteraction && newInteractionPosition == _lastGridInteraction) return;
            OnTileInteract.Invoke(GetGridPos(eventData));
            _hasLastGridInteraction = true;
            _lastGridInteraction = newInteractionPosition;
        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            TryHandleNewPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            TryHandleNewPosition(eventData);
            _hasLastGridInteraction = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            TryHandleNewPosition(eventData);
        }
    }
}