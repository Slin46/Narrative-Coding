using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ritual.Tests
{
    public sealed class BlockPlayerTest : MonoBehaviour
    {
        private RitualTestScene host;
        private Transform player, scanner, target;
        private UnityEngine.UI.Image staminaFill;
        private TextMeshProUGUI hint;
        private float stamina = 100, scanTime, scanWarmup = 2;
        private bool exhausted, finished;
        public float Stamina => stamina;
        public bool Crouching { get; private set; }
        public int Interactions { get; private set; }
        public int Detections { get; private set; }
        public bool Finished => finished;
        public float ScanX => scanner == null ? 0 : scanner.localPosition.x;
        public Vector3 PlayerPosition => player.localPosition;

        public void Initialize(RitualTestScene owner, Transform actor, Transform scan, Transform goal)
        {
            host = owner; player = actor; scanner = scan; target = goal;
            host.UI.Label(host.UI.Root, "A / D  Move     SHIFT  Sprint     CTRL  Hold crouch", 32, 140, 1100, 36);
            hint = host.UI.Label(host.UI.Root, "", 32, 184, 1160, 75);
            host.UI.Panel(host.UI.Root, "StaminaTrack", 32, 595, 260, 20, new Color(.2f, .26f, .3f));
            staminaFill = host.UI.Panel(host.UI.Root, "Stamina", 32, 595, 260, 20, TestUI.Accent).GetComponent<UnityEngine.UI.Image>();
            host.UI.Label(host.UI.Root, "STAMINA", 32, 565, 260, 32, 18);
            host.UI.Button(host.UI.Root, "Reset", 1090, 580, 150, 50, ResetTest);
            ResetTest();
        }

        public void ResetTest()
        {
            player.localPosition = new Vector3(-8, -2.1f, 0);
            player.localScale = new Vector3(.8f, 1.8f, 1);
            stamina = 100; scanTime = 0; scanWarmup = 2; exhausted = false; finished = false;
            Crouching = false; Interactions = 0; Detections = 0;
            if (scanner != null) scanner.localPosition = new Vector3(-9, .2f, 0);
            host.Status.text = "Ready. Walk toward the yellow block, then press E.";
            if (scanner != null) host.Status.text = "Reach the green exit. Hold CTRL whenever the red scan passes you.";
        }

        private void Update()
        {
            if (host == null || host.Blocked || finished) return;
            var keys = host.Controls;
            float dt = Time.deltaTime;
            Crouching = keys.Held(Key.LeftCtrl) || keys.Held(Key.RightCtrl);
            float direction = (keys.Held(Key.D) ? 1 : 0) - (keys.Held(Key.A) ? 1 : 0);
            if (exhausted && stamina >= 25) exhausted = false;
            bool sprint = direction != 0 && !Crouching && !exhausted && stamina > 0 && (keys.Held(Key.LeftShift) || keys.Held(Key.RightShift));
            stamina = Mathf.Clamp(stamina + (sprint ? -28 : 19) * dt, 0, 100);
            if (stamina <= 0) exhausted = true;
            float speed = Crouching ? 1.7f : sprint ? 6.2f : 3.2f;
            float x = Mathf.Clamp(player.localPosition.x + direction * speed * dt, -10, 10);
            player.localPosition = new Vector3(x, Crouching ? -2.55f : -2.1f, 0);
            player.localScale = new Vector3(.8f, Crouching ? .9f : 1.8f, 1);
            staminaFill.rectTransform.sizeDelta = new Vector2(stamina * 2.6f, 20);
            staminaFill.color = exhausted ? new Color(.9f, .4f, .2f) : TestUI.Accent;
            if (scanner == null)
            {
                bool near = Mathf.Abs(x - target.localPosition.x) < 1.7f;
                hint.text = near ? "[ E ] Interact with the yellow block     |     TAB opens your notes" : "Approach the yellow block to interact. TAB opens your notes.";
                if (near && keys.Pressed(Key.E))
                {
                    Interactions++;
                    target.GetComponent<SpriteRenderer>().color = Interactions % 2 == 1 ? Color.green : new Color(.96f, .7f, .25f);
                    host.Status.text = $"Interaction successful! Count: {Interactions}";
                }
            }
            else UpdateScanner(dt, x);
        }

        private void UpdateScanner(float dt, float x)
        {
            if (scanWarmup > 0) scanWarmup -= dt;
            else scanTime += dt;
            float scanX = Mathf.PingPong(scanTime * 3.4f, 18) - 9;
            scanner.localPosition = new Vector3(scanX, .2f, 0);
            hint.text = scanWarmup > 0 ? "Scanner warming up... Hold CTRL to crouch below its detection height." : "RED = scan zone. Crouching hides your silhouette from the background enemy.";
            if (scanWarmup <= 0 && Mathf.Abs(x - scanX) < 1.1f && !Crouching)
            {
                Detections++;
                player.localPosition = new Vector3(-8, -2.1f, 0);
                scanTime = 0; scanWarmup = 2;
                host.Status.text = $"Detected! Returned to start. Attempts: {Detections}. Hold CTRL during the scan.";
            }
            else if (x >= 8.5f)
            {
                finished = true;
                host.Status.text = "SUCCESS / You passed the enemy scan and reached the exit.";
            }
        }
    }
}
