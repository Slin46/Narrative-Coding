using UnityEngine;
using UnityEngine.InputSystem;

namespace Ritual.Tests
{
    public sealed class TestInput
    {
#if UNITY_EDITOR
        private Key[] current, previous;
        public void Simulate(Key[] keys) { previous = current; current = keys; }
#endif
        public bool Held(Key key)
        {
#if UNITY_EDITOR
            if (current != null) return System.Array.IndexOf(current, key) >= 0;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(Legacy(key));
#else
            return Keyboard.current != null && Keyboard.current[key].isPressed;
#endif
        }
        public bool Pressed(Key key)
        {
#if UNITY_EDITOR
            if (current != null) return System.Array.IndexOf(current, key) >= 0 && (previous == null || System.Array.IndexOf(previous, key) < 0);
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(Legacy(key));
#else
            return Keyboard.current != null && Keyboard.current[key].wasPressedThisFrame;
#endif
        }
#if ENABLE_LEGACY_INPUT_MANAGER
        private static KeyCode Legacy(Key key)
        {
            switch (key)
            {
                case Key.A: return KeyCode.A;
                case Key.D: return KeyCode.D;
                case Key.E: return KeyCode.E;
                case Key.Tab: return KeyCode.Tab;
                case Key.Escape: return KeyCode.Escape;
                case Key.LeftCtrl: return KeyCode.LeftControl;
                case Key.RightCtrl: return KeyCode.RightControl;
                case Key.LeftShift: return KeyCode.LeftShift;
                case Key.RightShift: return KeyCode.RightShift;
                default: return KeyCode.None;
            }
        }
#endif
    }
}
