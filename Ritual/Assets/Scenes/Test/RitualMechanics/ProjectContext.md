# Ritual project context — 2026-11-09

Confirmed from project files: Unity 6000.5.2f1; URP 17.6.0 with 2D packages; Input System 1.20.1; uGUI 2.6.0; Test Framework 1.8.0; Yarn Spinner 3.2.8. Player Settings enables both input backends (`activeInputHandler: 2`). TMP resources already exist.

Existing content: `Assets/Scenes/{StartScene,Office,Village,ContentWarning}.unity`; `Assets/Scripts/PlayerMovement.cs` uses legacy input, Rigidbody2D and Animator; `DialogueTrigger.cs` uses mouse picking and Yarn. These existing files were not changed. No first-party assembly definitions were observed in the inspected scripts directory. Build Settings referenced the missing `Assets/Scenes/SampleScene.unity` before this work and were intentionally not edited.

The five prototypes are isolated in `Assets/Scenes/Test`, with supporting files in `RitualMechanics`. Keyboard polling prefers the existing legacy backend when enabled, with a new Input System fallback; UI uses InputSystemUIInputModule and uGUI/TMP, basic SpriteRenderer blocks and an orthographic camera. Runtime UI is built on entering Play; serialized scenes retain their static blocks, camera, controller and references. No networking, persistent saves, character animation or extra packages are required.

There is an open Ritual Editor. No Unity MCP or Pipeline package was available. The local authoring helper generates only missing scenes additively, closes them, and restores the previous active scene. The opt-in smoke check restores saved scene setup after Play and writes evidence under `Logs/RitualTests`. Keyboard gameplay tests inject held/pressed commands through an editor-only adapter; this is distinct from physical keyboard testing. No player build or audio output-device listening is implied by smoke-test success.

Inspected sources: `ProjectSettings/ProjectVersion.txt`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/EditorBuildSettings.asset`, `Packages/manifest.json`, the two existing scripts listed above, TMP resources, editor log, and generated test scene assets. This is scoped onboarding for the prototypes; full architecture and production performance were not audited.
