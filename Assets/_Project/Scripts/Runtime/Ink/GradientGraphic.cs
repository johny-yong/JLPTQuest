using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //A vertical gradient drawn straight into the UI mesh, top colour first. Skies, ground fades and veils
    [RequireComponent(typeof(CanvasRenderer))]
    public class GradientGraphic : MaskableGraphic
    {
        [SerializeField] private Color[] colors = { Color.black, Color.white };
        [SerializeField] private float[] stops = new float[0]; //0 is the top, 1 the bottom. Leave empty to spread the colours evenly

        private readonly List<float> ts = new List<float>();
        private readonly List<Color> cs = new List<Color>();

        public void SetColors(Color[] newColors, float[] newStops)
        {
            colors = newColors;
            stops = newStops ?? new float[0];
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (colors == null || colors.Length == 0) return;

            ts.Clear();
            cs.Clear();
            bool even = stops == null || stops.Length != colors.Length;
            for (int i = 0; i < colors.Length; i++)
            {
                ts.Add(even ? (colors.Length == 1 ? 0f : i / (float)(colors.Length - 1)) : Mathf.Clamp01(stops[i]));
                cs.Add(colors[i]);
            }
            //Hold the end colours out to the edges when the stops don't reach them
            if (ts[0] > 0f) { ts.Insert(0, 0f); cs.Insert(0, cs[0]); }
            if (ts[ts.Count - 1] < 1f) { ts.Add(1f); cs.Add(cs[cs.Count - 1]); }

            Rect rect = GetPixelAdjustedRect();
            for (int i = 0; i < ts.Count; i++)
            {
                float y = Mathf.Lerp(rect.yMax, rect.yMin, ts[i]);
                Color32 tint = cs[i] * color;
                vh.AddVert(new Vector3(rect.xMin, y), tint, new Vector2(0f, 1f - ts[i]));
                vh.AddVert(new Vector3(rect.xMax, y), tint, new Vector2(1f, 1f - ts[i]));
                if (i == 0) continue;
                int b = (i - 1) * 2;
                vh.AddTriangle(b, b + 2, b + 3);
                vh.AddTriangle(b, b + 3, b + 1);
            }
        }
    }
}
