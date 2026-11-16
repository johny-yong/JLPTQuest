using UnityEngine;

namespace InkLook
{
    /// <summary>On-screen buttons for trying the ink effects. Only the demo scene uses it; delete freely.</summary>
    public class InkLookDemo : MonoBehaviour
    {
        float bleed;

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 16, 230, 260), GUI.skin.box);
            GUILayout.Label("Ink Look demo");
            if (GUILayout.Button("Brush strokes"))
            {
                Vector3 c = new Vector3(Random.Range(-3f, 3f), Random.Range(0.5f, 3f), 0f);
                BrushFx.Arc(c, 1.6f, 130f, -50f, 0.8f, BrushFx.Ink, 0.5f);
                BrushFx.Ring(c + Vector3.right * 2f, 0.8f, 0.4f, BrushFx.Ink, 0.7f, 1.5f);
                BrushFx.Splat(c - Vector3.right * 2f, 1.6f, BrushFx.Ink, 0.8f);
            }
            if (GUILayout.Button("Scene transition")) SceneTransition.Run(null);
            if (GUILayout.Button("Paper flash")) InkWorld.Flash = 1f;
            if (GUILayout.Button("Ink negative")) InkWorld.Invert = InkWorld.Invert > 0.5f ? 0f : 1f;
            GUILayout.Label("Screen bleed");
            bleed = GUILayout.HorizontalSlider(bleed, 0f, 1f);
            InkWorld.ScreenBleed = bleed;
            GUILayout.EndArea();
            InkWorld.Flash = Mathf.MoveTowards(InkWorld.Flash, 0f, Time.unscaledDeltaTime * 3f);
        }
    }
}
