#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace Ritual.Tests.Editor
{
    [DefaultExecutionOrder(-10000)]
    public sealed class RitualTestSmokeRunner : MonoBehaviour
    {
        private RitualTestScene currentScene;
        private int passed, failed, errors;
        private bool oldRunInBackground;
        private Key[] heldKeys = new Key[0];
        private const string Report = "Logs/RitualTests/Smoke.txt";
        private const string Folder = "Assets/Scenes/Test";
        private static readonly string[] Names = { "01_PlayerControls", "02_PicturePuzzle", "03_CrouchScanner", "04_MorseCode", "05_HiddenKey" };
        private void Check(bool condition, string message)
        {
            if (condition) passed++; else failed++;
            File.AppendAllText(Report, (condition ? "PASS " : "FAIL ") + message + "\n");
        }
        private void Keys(params Key[] keys) { heldKeys = keys; }
        private void Update()
        {
            if (currentScene != null) currentScene.Controls.Simulate(heldKeys);
        }
        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            { errors++; File.AppendAllText(Report, "ERROR " + message + "\n" + stack + "\n"); }
        }
        private IEnumerator Start()
        {
            oldRunInBackground = Application.runInBackground; Application.runInBackground = true;
            Application.logMessageReceived += OnLog;
            for (int sceneIndex = 0; sceneIndex < 5; sceneIndex++)
            {
                if (sceneIndex > 0)
                    yield return EditorSceneManager.LoadSceneAsyncInPlayMode(Folder + "/" + Names[sceneIndex] + ".unity", new LoadSceneParameters(LoadSceneMode.Single));
                yield return null; yield return null; yield return new WaitForSeconds(.15f);
                var root = UnityEngine.Object.FindFirstObjectByType<RitualTestScene>();
                Check(root != null && root.UI != null, "Scene " + (sceneIndex + 1) + " initializes");
                currentScene = root;
                Check(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length == 1, "Exactly one EventSystem");
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/RitualTests/" + Names[sceneIndex] + ".png"));
                if (sceneIndex == 0)
                {
                    var actor = root.GetComponent<BlockPlayerTest>();
                    float start = actor.PlayerPosition.x;
                    Keys(Key.D); yield return new WaitForSeconds(.6f);
                    Keys(); yield return null;
                    Check(actor.PlayerPosition.x > start + 1, "D moves right");
                    float right = actor.PlayerPosition.x;
                    Keys(Key.A); yield return new WaitForSeconds(.4f); Keys(); yield return null;
                    Check(actor.PlayerPosition.x < right - .5f, "A moves left");
                    float before = actor.Stamina;
                    Keys(Key.D, Key.LeftShift); yield return new WaitForSeconds(1); Keys(); yield return null;
                    Check(actor.Stamina < before - 15, "Sprint consumes stamina");
                    before = actor.Stamina; yield return new WaitForSeconds(.5f);
                    Check(actor.Stamina > before + 5, "Stamina recovers");
                    Keys(Key.LeftCtrl); yield return null; yield return null;
                    Check(actor.Crouching && root.transform.Find("Player_Block").localScale.y < 1, "CTRL changes crouch height");
                    Keys(); yield return null; yield return null;
                    root.transform.Find("Player_Block").localPosition = new Vector3(2, -2.1f, 0);
                    Keys(Key.E); yield return null; yield return null; Keys(); yield return null;
                    Check(actor.Interactions == 1, "E activates nearby block");
                    Keys(Key.Tab); yield return null; yield return null; Keys(); yield return null;
                    Check(root.Blocked, "TAB opens notes");
                    before = actor.PlayerPosition.x;
                    Keys(Key.D); yield return new WaitForSeconds(.2f); Keys(); yield return null;
                    Check(Mathf.Abs(actor.PlayerPosition.x - before) < .01f, "Notes block movement");
                    Keys(Key.Tab); yield return null; yield return null; Keys(); yield return null;
                    Keys(Key.Escape); yield return null; yield return null; Keys(); yield return null;
                    Check(root.SettingsOpen, "ESC opens settings");
                    Keys(Key.Escape); yield return null; yield return null; Keys(); yield return null;
                    Check(!root.Blocked, "ESC resumes gameplay");
                }
                if (sceneIndex == 1)
                {
                    var puzzle = root.GetComponent<PicturePuzzleTest>();
                    Check(!puzzle.Solved, "Picture starts shuffled");
                    for (int i = 0; i < 9; i++)
                    {
                        if (puzzle.TileAt(i) == i) continue;
                        int partner = 0; while (puzzle.TileAt(partner) != i) partner++;
                        puzzle.Choose(i); puzzle.Choose(partner);
                    }
                    Check(puzzle.Solved, "Restoring all nine image regions completes puzzle");
                    puzzle.ResetPuzzle(); Check(!puzzle.Solved, "Picture can be restarted");
                }
                if (sceneIndex == 2)
                {
                    var actor = root.GetComponent<BlockPlayerTest>();
                    yield return new WaitForSeconds(2.1f);
                    root.transform.Find("Player_Block").localPosition = new Vector3(actor.ScanX, -2.1f, 0);
                    yield return null; yield return null;
                    Check(actor.Detections == 1, "Standing in scan causes detection / respawn");
                    Keys(Key.LeftCtrl); yield return null; yield return null;
                    yield return new WaitForSeconds(2.1f);
                    root.transform.Find("Player_Block").localPosition = new Vector3(actor.ScanX, -2.55f, 0);
                    yield return null; yield return null;
                    Check(actor.Detections == 1 && actor.Crouching, "Crouching avoids active scan");
                    root.transform.Find("Player_Block").localPosition = new Vector3(9, -2.55f, 0);
                    yield return null; yield return null;
                    Check(actor.Finished, "Exit completes scanner scene"); Keys(); yield return null;
                }
                if (sceneIndex == 3)
                {
                    var morse = root.GetComponent<MorsePuzzleTest>();
                    morse.PlaySignal(); yield return new WaitForSeconds(1.4f);
                    Check(morse.Playing && root.GetComponent<AudioSource>().clip != null, "Morse playback starts generated tone");
                    morse.StopSignal();
                    for (int i = 0; i < 4; i++) morse.EnterDigit((morse.Answer[i] - '0' + 1) % 10);
                    morse.Submit(); Check(!morse.Solved, "Incorrect Morse answer rejected");
                    for (int i = 0; i < 4; i++) morse.Backspace();
                    foreach (char digit in morse.Answer) morse.EnterDigit(digit - '0');
                    morse.Submit(); Check(morse.Solved, "Correct Morse answer opens lock");
                    morse.ResetPuzzle(); Check(!morse.Solved && morse.Entered.Length == 0, "New Morse code resets state");
                }
                if (sceneIndex == 4)
                {
                    var search = root.GetComponent<StackSearchTest>();
                    Check(search.CoveringCount == 5, "All five objects cover key initially");
                    search.Collect(); Check(!search.Collected, "Covered key cannot be collected");
                    var eventData = new PointerEventData(EventSystem.current);
                    var block = search.ItemAt(4);
                    Vector2 original = block.anchoredPosition;
                    Vector3 world = block.TransformPoint(new Vector3(50, -50, 0));
                    eventData.position = RectTransformUtility.WorldToScreenPoint(null, world);
                    var drag = block.GetComponent<DraggableTestBlock>();
                    drag.OnBeginDrag(eventData);
                    eventData.position += new Vector2(-240, 0);
                    drag.OnDrag(eventData); drag.OnEndDrag(eventData);
                    Check(block.anchoredPosition.x < original.x, "Pointer drag moves block");
                    for (int i = 0; i < 5; i++) search.ItemAt(i).anchoredPosition = new Vector2(0, 0);
                    search.Refresh(); Check(search.CoveringCount == 0, "Moving all objects uncovers key");
                    search.KeyRect.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                    Check(search.Collected && !search.KeyRect.gameObject.activeSelf, "Click collects revealed key");
                    search.ResetPuzzle(); Check(!search.Collected && search.CoveringCount == 5, "Search can be restarted");
                }
                Keys(); yield return null;
            }
            File.AppendAllText(Report, $"RESULT passed={passed} failed={failed} runtimeErrors={errors}\n");
            Debug.Log($"Ritual smoke tests: {passed} passed / {failed} failed / {errors} runtime errors.");
            EditorApplication.isPlaying = false;
        }
        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLog;
            Application.runInBackground = oldRunInBackground;
        }
    }
}
#endif
