using UnityEngine;
using UnityEngine.InputSystem;

namespace JLPTQuest
{
    /*
        One shared sway for every ParallaxLayer, worked out once a frame.
        Phones tilt it with the accelerometer, desktop and WebGL follow the pointer a little,
        and there's always a slow idle drift so the scene breathes even when nothing moves.
        Calm mode keeps it much smaller.
    */
    public static class ParallaxSway
    {
        public const float Range = 36f; //Canvas units at depth 1
        public const float CalmScale = 0.35f;

        private static int lastFrame = -1;
        private static Vector2 offset;
        private static Vector2 velocity;

        public static Vector2 Offset
        {
            get
            {
                if (lastFrame != Time.frameCount) Step();
                return offset;
            }
        }

        private static void Step()
        {
            lastFrame = Time.frameCount;
            float t = Time.unscaledTime;
            Vector2 target = new Vector2(Mathf.Sin(t * 0.17f), Mathf.Sin(t * 0.11f + 1.3f)) * 0.35f;
            target += Lean();
            target = Vector2.ClampMagnitude(target, 1f) * Range * (GameSettings.CalmMode ? CalmScale : 1f);
            offset = Vector2.SmoothDamp(offset, target, ref velocity, 0.45f, Mathf.Infinity, Time.unscaledDeltaTime);
        }

        //-1..1 from the device tilt or the pointer, nothing if neither is there
        private static Vector2 Lean()
        {
            Accelerometer accelerometer = Accelerometer.current;
            if (accelerometer != null)
            {
                if (!accelerometer.enabled) InputSystem.EnableDevice(accelerometer);
                Vector3 g = accelerometer.acceleration.ReadValue();
                return new Vector2(Mathf.Clamp(g.x * 1.6f, -1f, 1f), Mathf.Clamp((g.y + 0.6f) * 1.6f, -1f, 1f));
            }
            Pointer pointer = Pointer.current;
            if (pointer != null && Screen.width > 0 && Screen.height > 0)
            {
                Vector2 p = pointer.position.ReadValue();
                return new Vector2(p.x / Screen.width - 0.5f, p.y / Screen.height - 0.5f) * -0.8f;
            }
            return Vector2.zero;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLaunch()
        {
            lastFrame = -1;
            offset = Vector2.zero;
            velocity = Vector2.zero;
        }
    }
}
