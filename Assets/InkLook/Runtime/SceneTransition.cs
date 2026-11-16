using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace InkLook
{
    /// <summary>
    /// Scene changes are painted: broad strokes of ink sweep across until the page is black,
    /// the next scene loads underneath, and the wet ink lifts off stroke by stroke to reveal it.
    ///
    /// Needs nothing in the scene. Call SceneTransition.Go("SceneName"), or Run(callback) to cover the screen,
    /// do something (respawn, teleport) and reveal again. Settings come from Resources/InkLook/InkTransitionSettings.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        /// <summary>True while a transition is on screen. Ignore input while it is.</summary>
        public static bool Busy { get; private set; }
        /// <summary>Fired when the strokes start to cover the screen. Hook your own sound here.</summary>
        public static event Action CoverStarted;
        /// <summary>Fired when the ink starts to lift.</summary>
        public static event Action RevealStarted;
        /// <summary>Fired when the screen is clear again.</summary>
        public static event Action Finished;

        static SceneTransition inst;
        InkTransitionSettings settings;
        Material mat;
        RawImage image;
        AudioSource audioSource;
        static readonly int CoverId = Shader.PropertyToID("_Cover");
        static readonly int RevealId = Shader.PropertyToID("_Reveal");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            inst = null;
            Busy = false;
            CoverStarted = null;
            RevealStarted = null;
            Finished = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            var s = Resources.Load<InkTransitionSettings>("InkLook/InkTransitionSettings");
            if (s != null && s.revealOnStart && Ensure()) inst.StartCoroutine(inst.OpenOnly());
        }

        static bool Ensure()
        {
            if (inst != null) return true;
            var s = Resources.Load<InkTransitionSettings>("InkLook/InkTransitionSettings");
            if (s == null || s.material == null) return false;

            var go = new GameObject("InkSceneTransition");
            DontDestroyOnLoad(go);
            inst = go.AddComponent<SceneTransition>();
            inst.settings = s;
            InkWorld.EnsureGrain(s.grain);

            var canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(go.transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = s.sortingOrder;
            canvasGo.AddComponent<CanvasScaler>();

            var imgGo = new GameObject("Ink", typeof(RectTransform));
            var rt = (RectTransform)imgGo.transform;
            rt.SetParent(canvasGo.transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            inst.image = imgGo.AddComponent<RawImage>();
            inst.image.raycastTarget = true; // swallows clicks while the screen is covered
            inst.mat = new Material(s.material);
            inst.image.material = inst.mat;
            inst.image.texture = s.material.mainTexture;
            inst.image.enabled = false;

            inst.audioSource = go.AddComponent<AudioSource>();
            inst.audioSource.playOnAwake = false;
            inst.audioSource.ignoreListenerPause = true;
            return true;
        }

        /// <summary>Paint the screen out, load the scene, and open onto it.</summary>
        public static void Go(string sceneName)
        {
            if (Busy) return;
            if (!Ensure()) { SceneManager.LoadScene(sceneName); return; }
            inst.StartCoroutine(inst.Run(() => SceneManager.LoadSceneAsync(sceneName)));
        }

        /// <summary>Paint the screen out, load the scene by build index, and open onto it.</summary>
        public static void Go(int buildIndex)
        {
            if (Busy) return;
            if (!Ensure()) { SceneManager.LoadScene(buildIndex); return; }
            inst.StartCoroutine(inst.Run(() => SceneManager.LoadSceneAsync(buildIndex)));
        }

        /// <summary>Paint the screen out, call atMidpoint while it is covered, and open again.</summary>
        public static void Run(Action atMidpoint)
        {
            if (Busy) return;
            if (!Ensure()) { if (atMidpoint != null) atMidpoint(); return; }
            inst.StartCoroutine(inst.Run(() => { if (atMidpoint != null) atMidpoint(); return null; }));
        }

        static float Step() { return Mathf.Min(Time.unscaledDeltaTime, 0.05f); }

        void Play(AudioClip clip)
        {
            if (clip != null && audioSource != null) audioSource.PlayOneShot(clip, settings.soundVolume);
        }

        IEnumerator Run(Func<AsyncOperation> work)
        {
            Busy = true;
            image.enabled = true;
            mat.SetFloat(RevealId, 0f);
            Play(settings.coverSound);
            if (CoverStarted != null) CoverStarted();
            for (float t = 0f; t < settings.coverTime; t += Step())
            {
                float k = t / settings.coverTime;
                mat.SetFloat(CoverId, k * k * (3f - 2f * k));
                yield return null;
            }
            mat.SetFloat(CoverId, 1f);
            yield return null;

            Time.timeScale = 1f;
            AsyncOperation load = work();
            while (load != null && !load.isDone) yield return null;
            Time.timeScale = 1f;
            for (float t = 0f; t < settings.holdTime; t += Step()) yield return null;
            yield return Open();
            Busy = false;
            if (Finished != null) Finished();
        }

        IEnumerator OpenOnly()
        {
            Busy = true;
            image.enabled = true;
            mat.SetFloat(CoverId, 1f);
            mat.SetFloat(RevealId, 0f);
            yield return null;
            yield return Open();
            Busy = false;
            if (Finished != null) Finished();
        }

        IEnumerator Open()
        {
            Play(settings.revealSound);
            if (RevealStarted != null) RevealStarted();
            for (float t = 0f; t < settings.revealTime; t += Step())
            {
                float k = t / settings.revealTime;
                mat.SetFloat(RevealId, 1f - (1f - k) * (1f - k));
                yield return null;
            }
            mat.SetFloat(RevealId, 1f);
            mat.SetFloat(CoverId, 0f);
            image.enabled = false;
        }
    }
}
