using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ritual.Tests.Editor
{
    [InitializeOnLoad]
    public static class RitualTestAuthoring
    {
        public const string Folder = "Assets/Scenes/Test";
        public const string ToolsFolder = Folder + "/RitualMechanics";
        public static readonly string[] Names = { "01_PlayerControls", "02_PicturePuzzle", "03_CrouchScanner", "04_MorseCode", "05_HiddenKey" };
        private const string RestoreKey = "RitualTests.Restore";
        private const string RunningKey = "RitualTests.Running";
        [Serializable] private sealed class SavedScene { public string path; public bool loaded; public bool active; }
        [Serializable] private sealed class SavedSetup { public SavedScene[] scenes; }

        static RitualTestAuthoring()
        {
            EditorApplication.delayCall += ProcessRequests;
            EditorApplication.playModeStateChanged += OnPlayState;
        }

        private static void ProcessRequests()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            { EditorApplication.delayCall += ProcessRequests; return; }
            string request = ToolsFolder + "/Generate.request.txt";
            if (File.Exists(request))
            {
                try { GenerateMissing(); AssetDatabase.DeleteAsset(request); }
                catch (Exception e) { Debug.LogException(e); }
            }
            string validate = ToolsFolder + "/Validate.request.txt";
            if (File.Exists(validate) && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                AssetDatabase.DeleteAsset(validate);
                RunSmokeTests();
            }
        }

        [MenuItem("Ritual Tests/Create missing five test scenes")]
        public static void GenerateMissing()
        {
            Directory.CreateDirectory(ToolsFolder + "/Generated");
            string png = ToolsFolder + "/Generated/Block.png";
            if (!File.Exists(png))
            {
                var texture = new Texture2D(16, 16);
                texture.SetPixels(Enumerable.Repeat(Color.white, 256).ToArray()); texture.Apply();
                File.WriteAllBytes(png, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(png, ImportAssetOptions.ForceSynchronousImport);
                var importer = (TextureImporter)AssetImporter.GetAtPath(png);
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 16;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            Sprite square = AssetDatabase.LoadAssetAtPath<Sprite>(png);
            if (square == null) throw new InvalidOperationException("Block sprite was not imported.");
            var previous = SceneManager.GetActiveScene();
            string report = "Unity " + Application.unityVersion + "\n";
            for (int i = 0; i < Names.Length; i++)
            {
                string path = Folder + "/" + Names[i] + ".unity";
                if (File.Exists(path)) { report += "PRESERVED " + path + "\n"; continue; }
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                try
                {
                    var root = new GameObject("RitualTest_" + Names[i]).AddComponent<RitualTestScene>();
                    root.Configure((TestKind)(i + 1), square);
                    root.BuildEnvironment();
                    if (root.GetComponentsInChildren<Camera>().Length != 1) throw new InvalidOperationException("Missing camera");
                    foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>())
                        if (renderer.sprite == null) throw new InvalidOperationException("Missing block sprite");
                    if (!EditorSceneManager.SaveScene(scene, path)) throw new IOException("Could not save " + path);
                    report += "CREATED + REFERENCES CHECKED " + path + "\n";
                }
                finally { EditorSceneManager.CloseScene(scene, true); }
            }
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
            Directory.CreateDirectory("Logs/RitualTests");
            File.WriteAllText("Logs/RitualTests/Generation.txt", report);
            AssetDatabase.SaveAssets();
            Debug.Log("Ritual tests: all five scenes are available in " + Folder);
        }

        [MenuItem("Ritual Tests/Run five-scene smoke tests")]
        public static void RunSmokeTests()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty || string.IsNullOrEmpty(scene.path))
                {
                    Debug.LogWarning("Ritual tests: save your open scenes before smoke tests; no scene was changed.");
                    return;
                }
            }
            var setup = EditorSceneManager.GetSceneManagerSetup();
            SessionState.SetString(RestoreKey, JsonUtility.ToJson(new SavedSetup { scenes = setup.Select(s => new SavedScene { path = s.path, loaded = s.isLoaded, active = s.isActive }).ToArray() }));
            SessionState.SetBool(RunningKey, true);
            Directory.CreateDirectory("Logs/RitualTests");
            File.WriteAllText("Logs/RitualTests/Smoke.txt", "Started " + DateTime.Now.ToString("s") + "\n");
            EditorSceneManager.OpenScene(Folder + "/" + Names[0] + ".unity");
            EditorApplication.ExecuteMenuItem("Window/General/Game");
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayState(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(RunningKey, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                var runner = new GameObject("RitualTestSmokeRunner").AddComponent<RitualTestSmokeRunner>();
                UnityEngine.Object.DontDestroyOnLoad(runner.gameObject);
            }
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool(RunningKey, false);
                var saved = JsonUtility.FromJson<SavedSetup>(SessionState.GetString(RestoreKey, ""));
                if (saved != null)
                    EditorSceneManager.RestoreSceneManagerSetup(saved.scenes.Select(s => new SceneSetup { path = s.path, isLoaded = s.loaded, isActive = s.active }).ToArray());
            }
        }
    }
}
