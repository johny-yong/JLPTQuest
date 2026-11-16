using UnityEngine;

namespace InkLook
{
    /// <summary>
    /// Feeds the ink shaders the values they share: paper colour, distance fade, grain texture, and the four
    /// full-screen effects. Put exactly one in every scene that uses the ink look.
    /// Gameplay code drives the effects through the static fields; this pushes them to the shaders every frame.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class InkWorld : MonoBehaviour
    {
        [Tooltip("The colour of the paper. Everything fades towards it.")]
        public Color paper = new Color(0.957f, 0.937f, 0.894f);
        [Tooltip("World Z where things start to fade into the mist.")]
        public float fogZStart = 6f;
        [Tooltip("Distance over which they fade.")]
        public float fogZRange = 90f;
        [Range(0f, 1f)] public float fogMax = 0.92f;
        [Tooltip("r = bristle streaks, g = blotchy wash, b = paper fibre. Import as linear, repeat.")]
        public Texture2D grain;
        [Tooltip("Also set the main camera's background to the paper colour.")]
        public bool paintCameraBackground = true;

        /// <summary>0..1, wet ink stains creeping in from the screen edges.</summary>
        public static float ScreenBleed;
        /// <summary>0..1, the whole picture dissolves into the paper.</summary>
        public static float Dissolve;
        /// <summary>0..1, a flash of bare paper.</summary>
        public static float Flash;
        /// <summary>0..1, the picture turns into its ink negative.</summary>
        public static float Invert;

        static readonly int PaperId = Shader.PropertyToID("_InkPaperColor");
        static readonly int FogId = Shader.PropertyToID("_InkFogZ");
        static readonly int BleedId = Shader.PropertyToID("_InkBleed");
        static readonly int DissolveId = Shader.PropertyToID("_InkDissolve");
        static readonly int FlashId = Shader.PropertyToID("_InkFlash");
        static readonly int InvertId = Shader.PropertyToID("_InkInvert");
        internal static readonly int GrainId = Shader.PropertyToID("_InkGrainTex");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { ScreenBleed = Dissolve = Flash = Invert = 0f; }

        void OnEnable()
        {
            ScreenBleed = Dissolve = Flash = Invert = 0f;
            Apply();
        }

        void OnDisable()
        {
            Shader.SetGlobalFloat(BleedId, 0f);
            Shader.SetGlobalFloat(DissolveId, 0f);
            Shader.SetGlobalFloat(FlashId, 0f);
            Shader.SetGlobalFloat(InvertId, 0f);
        }

        void Update() { Apply(); }

        void Apply()
        {
            Color lin = paper.linear;
            Shader.SetGlobalVector(PaperId, new Vector4(lin.r, lin.g, lin.b, 1f));
            Shader.SetGlobalVector(FogId, new Vector4(fogZStart, 1f / Mathf.Max(0.01f, fogZRange), fogMax, 0f));
            if (grain != null) Shader.SetGlobalTexture(GrainId, grain);
            bool playing = Application.isPlaying;
            Shader.SetGlobalFloat(BleedId, playing ? ScreenBleed : 0f);
            Shader.SetGlobalFloat(DissolveId, playing ? Dissolve : 0f);
            Shader.SetGlobalFloat(FlashId, playing ? Flash : 0f);
            Shader.SetGlobalFloat(InvertId, playing ? Invert : 0f);
            if (paintCameraBackground)
            {
                Camera c = Camera.main;
                if (c != null && c.clearFlags == CameraClearFlags.SolidColor) c.backgroundColor = paper;
            }
        }

        /// <summary>Sets the grain texture globally if no InkWorld has done so yet. Lets the transition work on its own.</summary>
        internal static void EnsureGrain(Texture2D fallback)
        {
            if (fallback != null && Shader.GetGlobalTexture(GrainId) == null) Shader.SetGlobalTexture(GrainId, fallback);
        }
    }
}
