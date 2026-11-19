using UnityEngine;

namespace JLPTQuest
{
    /*
        Moves a layer by the shared sway, scaled by how close it is. Depth 0 never moves (the sky),
        1 moves the full range (the companion). Far things move least, which is what sells the depth.
        Never put one on the question card or the answer tiles, reading stays still.
    */
    [RequireComponent(typeof(RectTransform))]
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField, Range(0f, 1.5f)] private float depth = 0.5f;

        private RectTransform rect;
        private Vector2 rest;

        public float Depth { get => depth; set => depth = value; }

        void Awake()
        {
            rect = (RectTransform)transform;
            rest = rect.anchoredPosition;
        }

        void LateUpdate()
        {
            rect.anchoredPosition = rest + ParallaxSway.Offset * depth;
        }

        void OnDisable()
        {
            if (rect != null) rect.anchoredPosition = rest;
        }
    }
}
