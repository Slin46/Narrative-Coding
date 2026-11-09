using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Ritual.Tests
{
    
    public sealed class TestUI : MonoBehaviour
    {
        public RectTransform Root { get; private set; }
        public static readonly Color Ink = new Color(.08f, .11f, .16f);
        public static readonly Color Accent = new Color(.16f, .68f, .77f);

        public void Initialize()
        {
            var go = new GameObject("TestCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            go.transform.SetParent(transform, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = go.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
            Root = go.GetComponent<RectTransform>();
            var events = new GameObject("TestEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            events.transform.SetParent(transform, false);
        }

        public RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
            return rect;
        }

        public RectTransform Panel(Transform parent, string name, float x, float y, float w, float h, Color color, bool blocks = false)
        {
            var rect = Rect(name, parent, x, y, w, h);
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = blocks;
            return rect;
        }

        public TextMeshProUGUI Label(Transform parent, string text, float x, float y, float w, float h, int size = 22)
        {
            var rect = Rect("Label", parent, x, y, w, h);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }

        public UnityEngine.UI.Button Button(Transform parent, string text, float x, float y, float w, float h, Action click)
        {
            var rect = Panel(parent, text, x, y, w, h, new Color(.19f, .27f, .35f), true);
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = rect.GetComponent<UnityEngine.UI.Image>();
            button.onClick.AddListener(() => click());
            var label = Label(rect, text, 6, 3, w - 12, h - 6, 21);
            label.alignment = TextAlignmentOptions.Center;
            return button;
        }
    }
}
