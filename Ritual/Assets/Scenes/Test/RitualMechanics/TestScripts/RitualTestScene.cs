using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ritual.Tests
{
    public enum TestKind { Controls = 1, Picture = 2, Scanner = 3, Morse = 4, Search = 5 }

    public sealed class RitualTestScene : MonoBehaviour
    {
        [SerializeField] private TestKind kind = TestKind.Controls;
        [SerializeField] private Sprite square;
        [SerializeField] private Transform player;
        [SerializeField] private Transform scanner;
        [SerializeField] private Transform interactable;
        public TestKind Kind => kind;
        public TestUI UI { get; private set; }
        public bool Blocked => settings != null && (settings.activeSelf || page.activeSelf);
        public bool SettingsOpen => settings != null && settings.activeSelf;
        public TextMeshProUGUI Status { get; private set; }
        private GameObject settings, page;
        private float volume = .6f;
        public float Volume => volume;
        public bool Paused => Blocked;
        public TestInput Controls { get; } = new TestInput();

        public void Configure(TestKind value, Sprite blockSprite)
        {
            kind = value;
            square = blockSprite;
        }

        public void BuildEnvironment()
        {
            var cameraObject = new GameObject("TestCamera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(transform, false);
            cameraObject.transform.localPosition = new Vector3(0, 0, -10);
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = TestUI.Ink;
            cameraObject.tag = "MainCamera";
            Block("Floor", new Vector2(0, -3.3f), new Vector2(24, .6f), new Color(.25f, .31f, .36f));
            if (kind == TestKind.Controls || kind == TestKind.Scanner)
            {
                player = Block("Player_Block", new Vector2(-8, -2.1f), new Vector2(.8f, 1.8f), TestUI.Accent).transform;
                if (kind == TestKind.Controls)
                    interactable = Block("E_Interact_Block", new Vector2(2, -2.35f), new Vector2(1.3f, 1.3f), new Color(.96f, .7f, .25f)).transform;
                else
                {
                    Block("Enemy_Background", new Vector2(0, .2f), new Vector2(1.2f, 1.8f), new Color(.8f, .24f, .31f));
                    Block("CrouchCover", new Vector2(0, -2.55f), new Vector2(2.4f, .9f), new Color(.3f, .4f, .48f), 2);
                    scanner = Block("Enemy_Scan", new Vector2(-9, .2f), new Vector2(1.4f, 4.4f), new Color(1, .18f, .18f, .28f), 3).transform;
                    interactable = Block("Exit", new Vector2(9, -2), new Vector2(.6f, 2), new Color(.3f, .85f, .5f)).transform;
                }
            }
            else
                Block("Puzzle_Backdrop", Vector2.zero, new Vector2(22, 11), new Color(.1f, .15f, .21f));
        }

        private GameObject Block(string name, Vector2 position, Vector2 size, Color color, int order = 0)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(transform, false);
            go.transform.localPosition = position;
            go.transform.localScale = size;
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            renderer.sortingOrder = order;
            return go;
        }

        private void Start()
        {
            UI = gameObject.AddComponent<TestUI>();
            UI.Initialize();
            UI.Panel(UI.Root, "Header", 0, 0, 1280, 130, TestUI.Ink);
            string[] titles = { "", "01 / PLAYER CONTROLS", "02 / RESTORE THE PICTURE", "03 / CROUCH UNDER THE SCAN", "04 / LISTEN TO THE MORSE CODE", "05 / SEARCH THE STACK" };
            UI.Label(UI.Root, titles[(int)kind], 32, 20, 1020, 45, 30);
            UI.Label(UI.Root, "ESC  Settings / pause     |     Use each scene's Reset button to retry", 32, 76, 1100, 36, 20);
            UI.Panel(UI.Root, "Footer", 0, 650, 1280, 70, TestUI.Ink);
            Status = UI.Label(UI.Root, "Ready", 32, 667, 1200, 42, 23);
            if (kind == TestKind.Controls || kind == TestKind.Scanner)
                gameObject.AddComponent<BlockPlayerTest>().Initialize(this, player, scanner, interactable);
            else if (kind == TestKind.Picture) gameObject.AddComponent<PicturePuzzleTest>().Initialize(this);
            else if (kind == TestKind.Morse) gameObject.AddComponent<MorsePuzzleTest>().Initialize(this);
            else gameObject.AddComponent<StackSearchTest>().Initialize(this);
            BuildOverlays();
        }

        private void BuildOverlays()
        {
            page = UI.Panel(UI.Root, "TabPage", 0, 0, 1280, 720, new Color(0, 0, 0, .7f), true).gameObject;
            var pageContent = UI.Panel(page.transform, "Notes", 200, 150, 880, 460, TestUI.Ink, true);
            UI.Label(pageContent, "FIELD NOTES / TAB", 32, 28, 800, 55, 30);
            UI.Label(pageContent, "Test inventory and notes page.\n\nA / D     Move left / right\nE           Interact with the yellow block\nSHIFT    Sprint while stamina remains\nCTRL     Hold to crouch\nTAB        Close this page", 32, 105, 800, 300, 24);
            page.SetActive(false);
            settings = UI.Panel(UI.Root, "Settings", 0, 0, 1280, 720, new Color(0, 0, 0, .7f), true).gameObject;
            var content = UI.Panel(settings.transform, "SettingsContent", 340, 160, 600, 440, TestUI.Ink, true);
            UI.Label(content, "SETTINGS / PAUSED", 30, 24, 540, 55, 30);
            var volumeLabel = UI.Label(content, "Audio volume: 60%", 30, 100, 540, 42);
            UI.Button(content, "Volume -", 30, 155, 250, 50, () => { volume = Mathf.Max(0, volume - .1f); volumeLabel.text = $"Audio volume: {Mathf.RoundToInt(volume * 100)}%"; });
            UI.Button(content, "Volume +", 310, 155, 250, 50, () => { volume = Mathf.Min(1, volume + .1f); volumeLabel.text = $"Audio volume: {Mathf.RoundToInt(volume * 100)}%"; });
            UI.Button(content, "Resume / ESC", 30, 245, 530, 55, () => settings.SetActive(false));
            UI.Label(content, "All test gameplay pauses while a panel is open.", 30, 330, 540, 70, 20);
            settings.SetActive(false);
        }

        private void Update()
        {
            if (settings == null) return;
            if (Controls.Pressed(Key.Escape))
            {
                if (page.activeSelf) page.SetActive(false);
                else settings.SetActive(!settings.activeSelf);
            }
            if (kind == TestKind.Controls && Controls.Pressed(Key.Tab) && !settings.activeSelf)
                page.SetActive(!page.activeSelf);
        }
    }
}
