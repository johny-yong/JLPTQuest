using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        The last layer on screen: vignette and film grain. The Finish prefab sits in both the menu and the battle,
        so changing it here changes it everywhere. Untick Show Grain to turn the grain off.
    */
    public class FinishOverlay : MonoBehaviour
    {
        [SerializeField] private RawImage grain;
        [SerializeField] private bool showGrain = true;
        [SerializeField, Range(0f, 0.15f)] private float grainStrength = 0.035f;

        void OnEnable()
        {
            Apply();
        }

        void OnValidate()
        {
            Apply();
        }

        private void Apply()
        {
            if (grain == null) return;
            grain.enabled = showGrain && grainStrength > 0f;
            Color color = grain.color;
            color.a = grainStrength;
            grain.color = color;
        }
    }
}
