using UnityEngine;
using UnityEngine.EventSystems;

namespace Ritual.Tests
{
    public sealed class DraggableTestBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private StackSearchTest owner;
        private RectTransform rect, board;
        private Vector2 offset;
        public void Initialize(StackSearchTest search)
        {
            owner = search;
            rect = GetComponent<RectTransform>();
            board = rect.parent.GetComponent<RectTransform>();
        }
        public void OnBeginDrag(PointerEventData data)
        {
            if (owner.Blocked) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(board, data.position, data.pressEventCamera, out var local);
            offset = rect.anchoredPosition - local;
            rect.SetAsLastSibling();
        }
        public void OnDrag(PointerEventData data)
        {
            if (owner.Blocked || data.button != PointerEventData.InputButton.Left) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(board, data.position, data.pressEventCamera, out var local);
            var position = local + offset;
            position.x = Mathf.Clamp(position.x, 0, board.rect.width - rect.rect.width);
            position.y = Mathf.Clamp(position.y, -board.rect.height + rect.rect.height, 0);
            rect.anchoredPosition = position;
            owner.Refresh();
        }
        public void OnEndDrag(PointerEventData data) { owner.Refresh(); }
    }
}
