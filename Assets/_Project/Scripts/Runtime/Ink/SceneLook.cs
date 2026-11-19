using System;
using UnityEngine;

namespace JLPTQuest
{
    public enum DriftKind { None, Fireflies, Petals, Birds, Embers }

    /*
        How one realm's scene is coloured at each time of day. One painted scene per realm, recoloured four ways.
        Every layer is a white silhouette for now, tinted from here. Painted layers can replace the sprites later
        and keep these tints (set them to white to show the art as painted).
    */
    [CreateAssetMenu(menuName = "JLPT Quest/Scene Look", fileName = "SceneLook")]
    public class SceneLook : ScriptableObject
    {
        public SceneTint dawn = new SceneTint();
        public SceneTint day = new SceneTint();
        public SceneTint dusk = new SceneTint();
        public SceneTint night = new SceneTint();

        public SceneTint For(TimeOfDay time)
        {
            switch (time)
            {
                case TimeOfDay.Dawn: return dawn;
                case TimeOfDay.Day: return day;
                case TimeOfDay.Dusk: return dusk;
                default: return night;
            }
        }
    }

    [Serializable]
    public class SceneTint
    {
        [Header("Sky, top to bottom")]
        public Color[] sky = { new Color(0.11f, 0.1f, 0.2f), new Color(0.07f, 0.06f, 0.11f) };
        public float[] skyStops = { 0f, 0.75f }; //0 is the top of the screen. Past the last stop the sky turns into ground
        public Color ground = new Color(0.07f, 0.06f, 0.11f); //The bottom of the screen stays dark so cards and buttons read
        public Color stars = Color.clear;

        [Header("Sun or moon")]
        public bool woodblockSun; //A flat rough-edged disc instead of the soft round one
        public Color celestial = Color.white;
        public Color halo = Color.clear; //Soft glow right around it
        public Color skyGlow = Color.clear; //Big wash of colour behind it
        public Vector2 position = new Vector2(0.64f, 0.82f); //Fraction of the screen, from the bottom left
        public float size = 340f;
        public Color ghostMoon = Color.clear; //The pale moon still up at dawn

        [Header("Brush strokes")]
        public Color cloudHigh = Color.clear; //Across the sun or moon
        public Color cloudLow = Color.clear;
        public Color hillMist = Color.clear; //Pale wash lying over the far hills

        [Header("Land, far to near")]
        public Color farHills = Color.gray;
        public Color torii = Color.gray;
        public Color nearHills = Color.gray;
        public Color mist = Color.clear; //Fades the land into the ground colour

        [Header("Life")]
        public DriftKind drift = DriftKind.None;
        public Color driftColor = Color.white;
        public Color companionGlow = Color.clear; //Rim light behind the companion so they sit in the scene
    }
}
