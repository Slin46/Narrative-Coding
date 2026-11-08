using UnityEngine;

namespace Ritual.Tests
{
    public sealed class StackSearchTest : MonoBehaviour
    {
        private RitualTestScene host;
        private RectTransform key;
        private UnityEngine.UI.Button keyButton;
        private readonly RectTransform[] objects = new RectTransform[5];
        public bool Collected { get; private set; }
        public int CoveringCount { get; private set; }
        public RectTransform ItemAt(int index) => objects[index];
        public RectTransform KeyRect => key;
        public bool Blocked => host.Blocked;

        public void Initialize(RitualTestScene owner)
        {
            host = owner;
            var ui = host.UI;
            ui.Label(ui.Root, "Drag the blocks aside with the LEFT mouse button. Uncover and click the KEY.", 32, 140, 1220, 40);
            var board = ui.Panel(ui.Root, "SearchTable", 40, 200, 1200, 420, new Color(.18f, .23f, .28f));
            keyButton = ui.Button(board, "KEY", 540, 185, 120, 65, Collect);
            key = keyButton.GetComponent<RectTransform>();
            keyButton.GetComponent<UnityEngine.UI.Image>().color = new Color(1, .78f, .2f);
            Color[] colors = { new Color(.35f,.42f,.53f), new Color(.64f,.38f,.28f), new Color(.32f,.52f,.43f), new Color(.55f,.4f,.61f), new Color(.4f,.54f,.64f) };
            for (int i = 0; i < objects.Length; i++)
            {
                var rect = ui.Panel(board, "MovableBlock_" + (i + 1), 420 - i * 7, 125 - i * 8, 330 + i * 13, 205 + i * 10, colors[i], true);
                objects[i] = rect;
                ui.Label(rect, "BLOCK " + (i + 1) + " / DRAG", 16, 18, 300, 40, 22);
                rect.gameObject.AddComponent<DraggableTestBlock>().Initialize(this);
            }
            // Reset is above the table so moving objects cannot cover it.
            ui.Button(ui.Root, "Reset stack", 1030, 77, 205, 44, ResetPuzzle);
            ResetPuzzle();
        }

        public void ResetPuzzle()
        {
            Collected = false; key.gameObject.SetActive(true); key.SetAsFirstSibling();
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].anchoredPosition = new Vector2(420 - i * 7, -(125 - i * 8));
                objects[i].SetAsLastSibling();
            }
            Refresh();
        }
        public void Refresh()
        {
            if (Collected) return;
            var keyBounds = new Rect(key.anchoredPosition.x, key.anchoredPosition.y - key.sizeDelta.y, key.sizeDelta.x, key.sizeDelta.y);
            CoveringCount = 0;
            foreach (var item in objects)
            {
                var bounds = new Rect(item.anchoredPosition.x, item.anchoredPosition.y - item.sizeDelta.y, item.sizeDelta.x, item.sizeDelta.y);
                if (bounds.Overlaps(keyBounds)) CoveringCount++;
            }
            keyButton.interactable = CoveringCount == 0;
            host.Status.text = CoveringCount == 0 ? "Key fully uncovered. Click the yellow KEY to collect it." : $"Objects covering the key: {CoveringCount}. Drag each one away.";
        }
        public void Collect()
        {
            if (host.Blocked || Collected || CoveringCount != 0) return;
            Collected = true; key.gameObject.SetActive(false);
            host.Status.text = "SUCCESS / Key collected into your test inventory.";
        }
    }
}
