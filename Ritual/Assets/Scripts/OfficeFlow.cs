using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class OfficeFlow : MonoBehaviour
{
    [SerializeField] private DialogueTrigger employeeId;
    [SerializeField] private DialogueTrigger groupPicture;
    [SerializeField] private GameObject email;
    [SerializeField] private SpriteRenderer emailImageLink;
    [SerializeField] private GameObject fixer;

    private const int GridSize = 3;
    private const int TileCount = GridSize * GridSize;
    private readonly int[] tileOrder = new int[TileCount];
    private readonly RawImage[] tileImages = new RawImage[TileCount];
    private readonly Image[] tileFrames = new Image[TileCount];

    private Canvas canvas;
    private TextMeshProUGUI puzzleStatus;
    private Collider2D emailLinkCollider;
    private SceneManagerScript sceneManager;
    private Sprite puzzleSprite;
    private Rect puzzleTextureRect;
    private int selectedTile = -1;
    private bool mailUnlocked;
    private bool puzzleOpened;
    private bool puzzleSolved;
    private bool transitioning;

    private async void Start()
    {
        sceneManager = GetComponent<SceneManagerScript>();
        if (sceneManager == null || employeeId == null || groupPicture == null || email == null ||
            emailImageLink == null || fixer == null)
        {
            Debug.LogError("OfficeFlow needs SceneManagerScript and all Office scene references.", this);
            enabled = false;
            return;
        }

        Transform villagePic = fixer.transform.Find("VillagePic");
        var puzzleRenderer = villagePic != null ? villagePic.GetComponent<SpriteRenderer>() : null;
        if (puzzleRenderer == null || puzzleRenderer.sprite == null)
        {
            Debug.LogError("The Fixer object needs a VillagePic child with an assigned SpriteRenderer image.", fixer);
            enabled = false;
            return;
        }
        puzzleSprite = puzzleRenderer.sprite;

        BuildCanvas();
        var opening = CreateFullScreenPanel("Office opening", Color.black, true);
        canvas.sortingOrder = -1;

        employeeId.enabled = false;
        groupPicture.enabled = false;
        email.SetActive(false);
        fixer.SetActive(false);

        var runner = employeeId.dialogueRunner;
        if (runner != null && runner.YarnProject != null && runner.Dialogue.NodeExists("OfficeOpening"))
        {
            await runner.StartDialogue("OfficeOpening");
            await runner.DialogueTask;
        }

        if (this == null) return;

        opening.gameObject.SetActive(false);
        emailLinkCollider = emailImageLink.GetComponent<Collider2D>();
        if (emailLinkCollider == null)
        {
            var box = emailImageLink.gameObject.AddComponent<BoxCollider2D>();
            if (emailImageLink.sprite != null)
                box.size = emailImageLink.sprite.bounds.size;
            emailLinkCollider = box;
        }

        employeeId.enabled = true;
        groupPicture.enabled = true;
    }

    private void Update()
    {
        if (!mailUnlocked && employeeId != null && groupPicture != null &&
            employeeId.HasCompleted && groupPicture.HasCompleted)
        {
            mailUnlocked = true;
            email.SetActive(true);
        }

        if (!mailUnlocked || puzzleOpened || transitioning || !Input.GetMouseButtonDown(0))
            return;

        Camera camera = Camera.main;
        if (camera == null || emailLinkCollider == null || !emailLinkCollider.enabled)
            return;

        Vector2 mouseWorld = camera.ScreenToWorldPoint(Input.mousePosition);
        if (emailLinkCollider.OverlapPoint(mouseWorld))
            OpenFixerPuzzle();
    }

    private void BuildCanvas()
    {
        var canvasObject = new GameObject("Office Flow UI", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = .5f;
    }

    private void OpenFixerPuzzle()
    {
        puzzleOpened = true;
        email.SetActive(false);
        canvas.sortingOrder = 20;

        CreateFullScreenPanel("Fixer puzzle dim", new Color(.025f, .04f, .065f, .98f), true);
        var frame = CreatePanel("Puzzle frame", canvas.transform, Vector2.zero,
            new Vector2(1450, 920), new Color(.10f, .14f, .18f, 1f), true);
        CreateLabel(frame, "RESTORE THE IMAGE", new Vector2(0, 388), new Vector2(1320, 62), 36,
            TextAlignmentOptions.Center, Color.white);
        CreateLabel(frame, "Click two fragments to swap them.", new Vector2(0, 333),
            new Vector2(1320, 42), 22, TextAlignmentOptions.Center, new Color(.70f, .79f, .84f));

        // Fixer artwork is the only image source. There is intentionally no reference preview.
        BuildTileGrid(frame);
        puzzleStatus = CreateLabel(frame, "Select a fragment to begin.", new Vector2(0, -408),
            new Vector2(1320, 44), 22, TextAlignmentOptions.Center, new Color(.72f, .88f, .91f));
    }

    private void BuildTileGrid(Transform parent)
    {
        try
        {
            puzzleTextureRect = puzzleSprite.textureRect;
        }
        catch (UnityException)
        {
            puzzleTextureRect = puzzleSprite.rect;
        }

        Texture texture = puzzleSprite.texture;
        if (texture == null || texture.width == 0 || texture.height == 0)
        {
            Debug.LogError("The Fixer image has no texture.", fixer);
            return;
        }

        const float tileSize = 224f;
        const float gap = 10f;
        for (int i = 0; i < TileCount; i++)
        {
            tileOrder[i] = i;
            int row = i / GridSize;
            int column = i % GridSize;
            float x = (column - 1) * (tileSize + gap);
            float y = (1 - row) * (tileSize + gap) - 28f;
            int slot = i;

            var buttonRect = CreateRect("Fixer fragment " + (i + 1), parent,
                new Vector2(x, y), new Vector2(tileSize, tileSize));
            var background = buttonRect.gameObject.AddComponent<Image>();
            background.color = new Color(.25f, .33f, .39f, 1f);
            var button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.transition = Selectable.Transition.ColorTint;
            button.onClick.AddListener(() => SelectTile(slot));
            tileFrames[i] = background;

            var imageRect = CreateRect("Fragment image", buttonRect,
                Vector2.zero, new Vector2(tileSize - 12f, tileSize - 12f));
            tileImages[i] = imageRect.gameObject.AddComponent<RawImage>();
            tileImages[i].texture = texture;
            tileImages[i].raycastTarget = false;
            tileImages[i].uvRect = GetTileUv(puzzleTextureRect, texture, i);
        }

        ShuffleTiles();
        RefreshTiles();
    }

    private Rect GetTileUv(Rect source, Texture texture, int tile)
    {
        int row = tile / GridSize;
        int column = tile % GridSize;
        float width = source.width / GridSize / texture.width;
        float height = source.height / GridSize / texture.height;
        float x = source.x / texture.width + column * width;
        float y = source.y / texture.height + (GridSize - 1 - row) * height;
        return new Rect(x, y, width, height);
    }

    private void ShuffleTiles()
    {
        for (int i = 0; i < TileCount; i++) tileOrder[i] = i;
        for (int i = 0; i < 32; i++)
        {
            int first = Random.Range(0, TileCount);
            int second = Random.Range(0, TileCount);
            int old = tileOrder[first];
            tileOrder[first] = tileOrder[second];
            tileOrder[second] = old;
        }

        if (IsSolved())
        {
            tileOrder[0] = 1;
            tileOrder[1] = 0;
        }
    }

    private void SelectTile(int index)
    {
        if (puzzleSolved || transitioning) return;

        if (selectedTile < 0)
        {
            selectedTile = index;
            puzzleStatus.text = "Choose the fragment to swap with it.";
        }
        else if (selectedTile == index)
        {
            selectedTile = -1;
            puzzleStatus.text = "Select a fragment to begin.";
        }
        else
        {
            int old = tileOrder[selectedTile];
            tileOrder[selectedTile] = tileOrder[index];
            tileOrder[index] = old;
            selectedTile = -1;
            puzzleSolved = IsSolved();
            puzzleStatus.text = puzzleSolved ? "Image restored." : "Keep restoring the Fixer image.";
        }

        RefreshTiles();
        if (puzzleSolved)
        {
            foreach (var tile in tileFrames)
                if (tile != null) tile.GetComponent<Button>().interactable = false;
            StartCoroutine(PlayLightningFlash());
        }
    }

    private bool IsSolved()
    {
        for (int i = 0; i < TileCount; i++)
            if (tileOrder[i] != i) return false;
        return true;
    }

    private void RefreshTiles()
    {
        for (int i = 0; i < TileCount; i++)
        {
            tileImages[i].uvRect = GetTileUv(puzzleTextureRect, puzzleSprite.texture, tileOrder[i]);
            tileFrames[i].color = selectedTile == i
                ? new Color(.93f, .72f, .32f, 1f)
                : new Color(.25f, .33f, .39f, 1f);
        }
    }

    private IEnumerator PlayLightningFlash()
    {
        transitioning = true;
        var flashRect = CreateFullScreenPanel("Lightning flash", new Color(.82f, .92f, 1f, .96f), true);
        var image = flashRect.GetComponent<Image>();

        yield return new WaitForSecondsRealtime(.08f);
        image.color = new Color(.82f, .92f, 1f, .06f);
        yield return new WaitForSecondsRealtime(.07f);
        image.color = new Color(.92f, .97f, 1f, .82f);
        yield return new WaitForSecondsRealtime(.09f);
        image.color = new Color(.82f, .92f, 1f, .18f);
        yield return new WaitForSecondsRealtime(.06f);
        image.color = Color.white;
        yield return new WaitForSecondsRealtime(.2f);

        sceneManager.NextScene();
    }

    private RectTransform CreateFullScreenPanel(string name, Color color, bool blocksRaycasts)
    {
        var rect = CreateRect(name, canvas.transform, Vector2.zero, Vector2.zero);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = blocksRaycasts;
        return rect;
    }

    private RectTransform CreatePanel(string name, Transform parent, Vector2 position,
        Vector2 size, Color color, bool blocksRaycasts)
    {
        var rect = CreateRect(name, parent, position, size);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = blocksRaycasts;
        return rect;
    }

    private TextMeshProUGUI CreateLabel(Transform parent, string text, Vector2 position,
        Vector2 size, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        var rect = CreateRect("Puzzle label", parent, position, size);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        return label;
    }

    private RectTransform CreateRect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }
}

