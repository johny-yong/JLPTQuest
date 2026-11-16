using UnityEngine;

namespace InkLook
{
    /// <summary>
    /// Settings for the painted scene transition. One lives at Resources/InkLook/InkTransitionSettings;
    /// SceneTransition loads it on first use. Edit that asset to change timings, colours or sounds.
    /// </summary>
    [CreateAssetMenu(menuName = "Ink Look/Transition Settings", fileName = "InkTransitionSettings")]
    public class InkTransitionSettings : ScriptableObject
    {
        [Tooltip("A material using the Ink/Transition shader.")]
        public Material material;
        [Tooltip("Used if no InkWorld has set the grain texture.")]
        public Texture2D grain;

        [Header("Timing (seconds, unscaled)")]
        public float coverTime = 0.75f;
        public float holdTime = 0.12f;
        public float revealTime = 0.95f;

        [Header("Behaviour")]
        [Tooltip("The first scene of the game opens out of ink.")]
        public bool revealOnStart = true;
        [Tooltip("Canvas sorting order. Keep it above every other canvas.")]
        public int sortingOrder = 1000;

        [Header("Optional sounds (leave empty to use the events instead)")]
        public AudioClip coverSound;
        public AudioClip revealSound;
        [Range(0f, 1f)] public float soundVolume = 0.7f;
    }
}
