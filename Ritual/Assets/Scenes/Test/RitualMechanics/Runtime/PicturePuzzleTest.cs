using TMPro;
using UnityEngine;

namespace Ritual.Tests
{
    public sealed class PicturePuzzleTest : MonoBehaviour
    {
        private RitualTestScene host;
        private readonly int[] tiles = new int[9];
        private readonly UnityEngine.UI.RawImage[] pictures = new UnityEngine.UI.RawImage[9];
        private readonly UnityEngine.UI.Image[] frames = new UnityEngine.UI.Image[9];
        private Texture2D picture;
        private int selected = -1, moves;
        public bool Solved { get; private set; }
        public int TileAt(int index) => tiles[index];

        public void Initialize(RitualTestScene owner)
        {
            host = owner;
            var ui = host.UI;
            ui.Label(ui.Root, "Click two tiles to swap them. Restore the full block picture.", 32, 140, 1200, 38);
            picture = CreatePicture();
            for (int i = 0; i < 9; i++)
            {
                int index = i;
                var button = ui.Button(ui.Root, "", 170 + i % 3 * 144, 192 + i / 3 * 144, 140, 140, () => Choose(index));
                frames[i] = button.GetComponent<UnityEngine.UI.Image>();
                var rect = ui.Rect("PictureTile", button.transform, 4, 4, 132, 132);
                pictures[i] = rect.gameObject.AddComponent<UnityEngine.UI.RawImage>();
                pictures[i].texture = picture;
                pictures[i].raycastTarget = false;
            }
            ui.Label(ui.Root, "REFERENCE", 800, 205, 310, 40, 24);
            var reference = ui.Rect("ReferencePicture", ui.Root, 800, 255, 280, 280).gameObject.AddComponent<UnityEngine.UI.RawImage>();
            reference.texture = picture;
            reference.raycastTarget = false;
            ui.Button(ui.Root, "Shuffle / Reset", 800, 565, 280, 50, ResetPuzzle);
            ResetPuzzle();
        }

        public void ResetPuzzle()
        {
            selected = -1; moves = 0; Solved = false;
            for (int i = 0; i < 9; i++) tiles[i] = i;
            for (int i = 8; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int old = tiles[i]; tiles[i] = tiles[j]; tiles[j] = old;
            }
            bool ordered = true;
            for (int i = 0; i < 9; i++) ordered &= tiles[i] == i;
            if (ordered) { tiles[0] = 1; tiles[1] = 0; }
            Refresh();
        }

        public void Choose(int index)
        {
            if (host.Blocked || Solved || index < 0 || index >= 9) return;
            if (selected < 0) selected = index;
            else if (selected == index) selected = -1;
            else
            {
                int old = tiles[selected]; tiles[selected] = tiles[index]; tiles[index] = old;
                selected = -1; moves++;
                Solved = true;
                for (int i = 0; i < 9; i++) Solved &= tiles[i] == i;
            }
            Refresh();
        }

        private void Refresh()
        {
            for (int i = 0; i < 9; i++)
            {
                pictures[i].uvRect = new Rect((tiles[i] % 3) / 3f, (2 - tiles[i] / 3) / 3f, 1f / 3, 1f / 3);
                frames[i].color = selected == i ? new Color(1, .75f, .25f) : new Color(.25f, .36f, .43f);
            }
            host.Status.text = Solved ? $"SUCCESS / Picture restored in {moves} swaps." : $"Swaps: {moves} / {(selected < 0 ? "Select a tile" : "Select its swap partner")}";
        }

        private static Texture2D CreatePicture()
        {
            var texture = new Texture2D(300, 300, TextureFormat.RGBA32, false);
            texture.name = "Generated_Block_Picture";
            texture.filterMode = FilterMode.Point;
            var pixels = new Color[300 * 300];
            for (int y = 0; y < 300; y++)
            for (int x = 0; x < 300; x++)
            {
                Color c = new Color(.15f + x / 1200f, .3f + y / 650f, .5f);
                if (y < 45) c = new Color(.22f, .38f, .24f);
                if (x > 48 && x < 250 && y > 45 && y < 192) c = new Color(.82f, .55f, .3f);
                if (x > 30 && x < 265 && y >= 192 && y < 228) c = new Color(.75f, .25f, .22f);
                if (x > 135 && x < 180 && y > 45 && y < 130) c = new Color(.18f, .25f, .35f);
                if (x > 70 && x < 110 && y > 115 && y < 160) c = TestUI.Accent;
                if (x > 200 && x < 230 && y > 130 && y < 165) c = TestUI.Accent;
                if (x > 228 && x < 275 && y > 245 && y < 285) c = new Color(1, .8f, .3f);
                pixels[y * 300 + x] = c;
            }
            texture.SetPixels(pixels); texture.Apply();
            return texture;
        }

        private void OnDestroy() { if (picture != null) Destroy(picture); }
    }
}
