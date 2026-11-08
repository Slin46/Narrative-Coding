using System.Collections;
using TMPro;
using UnityEngine;

namespace Ritual.Tests
{
    public sealed class MorsePuzzleTest : MonoBehaviour
    {
        public static readonly string[] Codes = { "-----", ".----", "..---", "...--", "....-", ".....", "-....", "--...", "---..", "----." };
        private RitualTestScene host;
        private AudioSource source;
        private AudioClip dot, dash;
        private TextMeshProUGUI entryLabel, playbackLabel;
        private string entered = "", answer;
        private Coroutine playback;
        private bool wasPaused;
        private const float Unit = .2f;
        public string Answer => answer;
        public string Entered => entered;
        public bool Solved { get; private set; }
        public bool Playing => playback != null;

        public void Initialize(RitualTestScene owner)
        {
            host = owner;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.spatialBlend = 0;
            dot = Tone(Unit); dash = Tone(Unit * 3);
            var ui = host.UI;
            ui.Label(ui.Root, "Listen to four digits. Decode using the book on the RIGHT.", 32, 140, 1200, 40);
            ui.Panel(ui.Root, "Codebook", 850, 195, 360, 435, new Color(.17f, .23f, .3f));
            ui.Label(ui.Root, "MORSE CODE BOOK", 875, 210, 310, 42, 25);
            string book = "";
            for (int i = 0; i < 10; i++) book += $"{i}     {Codes[i]}\n";
            var bookLabel = ui.Label(ui.Root, book, 900, 262, 280, 335, 24);
            bookLabel.fontStyle = FontStyles.Bold;
            ui.Label(ui.Root, "DOT = 1 beat / DASH = 3 beats", 865, 597, 330, 30, 17);
            ui.Button(ui.Root, "Play / Replay signal", 70, 200, 350, 55, PlaySignal);
            ui.Button(ui.Root, "Stop", 440, 200, 180, 55, StopSignal);
            playbackLabel = ui.Label(ui.Root, "Press Play. Headphones recommended.", 70, 270, 700, 55, 23);
            entryLabel = ui.Label(ui.Root, "", 70, 330, 700, 60, 32);
            for (int i = 0; i < 10; i++)
            {
                int digit = i;
                ui.Button(ui.Root, i.ToString(), 70 + i % 5 * 130, 405 + i / 5 * 65, 115, 55, () => EnterDigit(digit));
            }
            ui.Button(ui.Root, "Backspace", 70, 550, 200, 55, Backspace);
            ui.Button(ui.Root, "Submit", 290, 550, 200, 55, Submit);
            ui.Button(ui.Root, "New code", 510, 550, 200, 55, ResetPuzzle);
            ResetPuzzle();
        }

        public void ResetPuzzle()
        {
            StopSignal(); Solved = false; entered = ""; answer = "";
            for (int i = 0; i < 4; i++) answer += Random.Range(0, 10).ToString();
            RefreshEntry();
            host.Status.text = "A new four-digit signal is ready. Listen and use the code book.";
        }

        public void EnterDigit(int digit)
        {
            if (host.Blocked || Solved || entered.Length >= 4 || digit < 0 || digit > 9) return;
            entered += digit.ToString(); RefreshEntry();
        }
        public void Backspace()
        {
            if (host.Blocked || Solved || entered.Length == 0) return;
            entered = entered.Substring(0, entered.Length - 1); RefreshEntry();
        }
        private void RefreshEntry() { entryLabel.text = "CODE:  " + entered.PadRight(4, '_'); }
        public void Submit()
        {
            if (host.Blocked || Solved) return;
            Solved = entered == answer;
            host.Status.text = Solved ? "SUCCESS / Correct Morse code. Lock opened." : "Incorrect code. Replay the signal and try again.";
            if (Solved) StopSignal();
        }
        public void PlaySignal()
        {
            if (host.Blocked || Solved) return;
            StopSignal(); playback = StartCoroutine(Transmit());
        }
        public void StopSignal()
        {
            if (playback != null) StopCoroutine(playback);
            playback = null;
            if (source != null) source.Stop();
            if (playbackLabel != null) playbackLabel.text = "Signal stopped. Press Play to hear it again.";
        }
        private IEnumerator Transmit()
        {
            playbackLabel.text = "Get ready...";
            yield return Wait(Unit * 5);
            for (int i = 0; i < answer.Length; i++)
            {
                playbackLabel.text = $"Listening: digit {i + 1} / 4";
                string code = Codes[answer[i] - '0'];
                for (int j = 0; j < code.Length; j++)
                {
                    source.clip = code[j] == '.' ? dot : dash;
                    source.volume = host.Volume;
                    source.Play();
                    yield return Wait(code[j] == '.' ? Unit : Unit * 3);
                    source.Stop();
                    if (j < code.Length - 1) yield return Wait(Unit);
                }
                if (i < answer.Length - 1) yield return Wait(Unit * 3);
            }
            playbackLabel.text = "Signal complete. Replay as often as needed.";
            playback = null;
        }
        private IEnumerator Wait(float seconds)
        {
            float elapsed = 0;
            while (elapsed < seconds)
            {
                yield return null;
                if (!host.Paused) elapsed += Time.deltaTime;
            }
        }
        private void Update()
        {
            if (host == null) return;
            source.volume = host.Volume;
            if (host.Paused && !wasPaused) source.Pause();
            else if (!host.Paused && wasPaused) source.UnPause();
            wasPaused = host.Paused;
        }
        private static AudioClip Tone(float duration)
        {
            const int rate = 44100;
            var clip = AudioClip.Create("Generated_Morse_Tone", Mathf.RoundToInt(duration * rate), 1, rate, false);
            var samples = new float[clip.samples];
            for (int i = 0; i < samples.Length; i++)
            {
                float fade = Mathf.Min(1, Mathf.Min(i, samples.Length - 1 - i) / (rate * .008f));
                samples[i] = Mathf.Sin(2 * Mathf.PI * 650 * i / rate) * .35f * fade;
            }
            clip.SetData(samples, 0); return clip;
        }
        private void OnDisable() { StopSignal(); }
        private void OnDestroy() { if (dot != null) Destroy(dot); if (dash != null) Destroy(dash); }
    }
}
