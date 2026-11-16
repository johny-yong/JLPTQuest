using UnityEngine;

namespace JLPTQuest
{
    //Fills a bar by stretching its anchor, stays crisp at any size and needs no sprite
    [ExecuteAlways]
    public class BarFill : MonoBehaviour
    {
        [SerializeField] private RectTransform fill;
        [SerializeField, Range(0f, 1f)] private float amount = 1f;

        public float Value
        {
            get => amount;
            set
            {
                amount = Mathf.Clamp01(value);
                Apply();
            }
        }

        void OnValidate()
        {
            Apply();
        }

        private void Apply()
        {
            if (fill == null) return;
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(amount, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
        }
    }
}
