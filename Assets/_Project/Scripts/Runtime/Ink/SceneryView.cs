using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        The painted scene behind Home and battles, stacked back to front:
        sky, stars, glow, sun or moon, clouds, far hills, torii, hill mist, near hills, mist, ground, drifting life.
        Every layer carries a ParallaxLayer, nearer layers sway more, so the scene has depth without any 3D.
        Colours come from the SceneLook for the current time of day and are checked again every minute.
        The veil is a night wash laid over the scene on screens that aren't Home (Tower, Codex...), so the scene
        is still there, just far behind.
    */
    public class SceneryView : MonoBehaviour
    {
        [SerializeField] private SceneLook look;
        [SerializeField] private GradientGraphic sky;
        [SerializeField] private Image stars;
        [SerializeField] private Image skyGlow;
        [SerializeField] private Image halo;
        [SerializeField] private Image moon;
        [SerializeField] private Image woodblockSun;
        [SerializeField] private Image ghostMoon;
        [SerializeField] private Image cloudHigh;
        [SerializeField] private Image cloudLow;
        [SerializeField] private Image farHills;
        [SerializeField] private Image hillMist;
        [SerializeField] private Image torii;
        [SerializeField] private Image nearHills;
        [SerializeField] private Image mist;
        [SerializeField] private GradientGraphic ground;
        [SerializeField] private DriftParticles drift;
        [SerializeField] private CanvasGroup veil;
        [SerializeField] private float veilSeconds = 0.25f;
        [SerializeField] private Vector2 celestialShift; //Moves the sun or moon out of the way, battles push it up and right so it's not behind the enemy

        private static readonly List<SceneryView> live = new List<SceneryView>();

        private TimeOfDay shown;
        private bool applied;
        private float nextCheck;
        private float veilTarget;

        public TimeOfDay Shown => shown;
        public SceneTint Tint => look != null ? look.For(shown) : null;

        //Repaints every open scene, for the editor's Time of day menu
        public static void RefreshAll()
        {
            foreach (SceneryView view in live) view.Apply(true);
        }

        void OnEnable()
        {
            live.Add(this);
            Apply(true);
            if (veil != null) veil.alpha = veilTarget;
        }

        void OnDisable()
        {
            live.Remove(this);
        }

        void Update()
        {
            if (Time.unscaledTime >= nextCheck)
            {
                nextCheck = Time.unscaledTime + 60f;
                Apply(false);
            }
            if (veil != null && !Mathf.Approximately(veil.alpha, veilTarget))
                veil.alpha = Mathf.MoveTowards(veil.alpha, veilTarget, Time.unscaledDeltaTime / Mathf.Max(0.01f, veilSeconds));
        }

        //0 shows the scene in full, 1 pushes it far back behind a night wash
        public void SetVeil(float amount, bool instant = false)
        {
            veilTarget = Mathf.Clamp01(amount);
            if (instant && veil != null) veil.alpha = veilTarget;
        }

        public void Apply(bool force)
        {
            TimeOfDay now = TimeOfDayClock.Now;
            if (!force && applied && now == shown) return;
            shown = now;
            applied = true;
            if (look == null) return;

            SceneTint tint = look.For(now);
            int n = tint.sky.Length;
            var colors = new Color[n + 1];
            var stops = new float[n + 1];
            for (int i = 0; i < n; i++)
            {
                colors[i] = tint.sky[i];
                stops[i] = i < tint.skyStops.Length ? tint.skyStops[i] : i / (float)n;
            }
            colors[n] = tint.ground;
            stops[n] = Mathf.Min(1f, stops[n - 1] + 0.22f);
            sky.SetColors(colors, stops);
            ground.SetColors(new[] { Fade(tint.ground), tint.ground }, new[] { 0f, 0.6f });

            Paint(stars, tint.stars);
            Paint(skyGlow, tint.skyGlow);
            Paint(halo, tint.halo);
            Paint(moon, tint.woodblockSun ? Color.clear : tint.celestial);
            Paint(woodblockSun, tint.woodblockSun ? tint.celestial : Color.clear);
            Paint(ghostMoon, tint.ghostMoon);
            Place(tint);

            Paint(cloudHigh, tint.cloudHigh);
            Paint(cloudLow, tint.cloudLow);
            Paint(farHills, tint.farHills);
            Paint(hillMist, tint.hillMist);
            Paint(torii, tint.torii);
            Paint(nearHills, tint.nearHills);
            Paint(mist, tint.mist);
            if (drift != null) drift.Show(tint.drift, tint.driftColor);
        }

        //Sun, moon and their glows sit together wherever the look puts them
        private void Place(SceneTint tint)
        {
            Vector2 at = tint.position + celestialShift;
            foreach (Image image in new[] { moon, woodblockSun })
                PlaceAt(image, at, tint.size);
            PlaceAt(halo, at, tint.size * 1.6f);
            PlaceAt(skyGlow, at, tint.size * 4f);

            //The high cloud is one brush stroke drawn across the lower half of the sun or moon
            if (cloudHigh != null)
            {
                RectTransform rect = cloudHigh.rectTransform;
                rect.anchorMin = rect.anchorMax = at + new Vector2(-0.05f, -0.025f);
                rect.sizeDelta = new Vector2(tint.size * 2.1f, Mathf.Max(48f, tint.size * 0.2f));
            }
        }

        //The ParallaxLayer on these rests at (0, 0) from its anchors, so moving the anchors is enough
        private static void PlaceAt(Image image, Vector2 at, float size)
        {
            if (image == null) return;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = at;
            rect.sizeDelta = new Vector2(size, size);
        }

        private static void Paint(Graphic graphic, Color color)
        {
            if (graphic == null) return;
            graphic.color = color;
            graphic.enabled = color.a > 0.001f;
        }

        private static Color Fade(Color color)
        {
            color.a = 0f;
            return color;
        }
    }
}
