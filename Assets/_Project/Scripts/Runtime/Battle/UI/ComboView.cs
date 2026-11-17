using System.Collections;
using TMPro;
using UnityEngine;

namespace JLPTQuest
{
    //The streak as a big red number between your companion and the enemy. Shows from 2 up, fades when it breaks
    public class ComboView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text count;
        [SerializeField] private int showFrom = 2;
        [SerializeField] private float popScale = 1.35f;
        [SerializeField] private float popSeconds = 0.25f;
        [SerializeField] private float fadeSeconds = 0.4f;

        private Coroutine routine;
        private int shown;

        void Awake()
        {
            group.alpha = 0f;
        }

        public void Show(int streak)
        {
            if (streak == shown) return;
            bool broke = streak < shown;
            shown = streak;

            if (routine != null) StopCoroutine(routine);
            if (streak >= showFrom)
            {
                count.text = streak.ToString();
                routine = StartCoroutine(Pop());
            }
            else if (broke && group.alpha > 0f)
            {
                routine = StartCoroutine(Fade());
            }
        }

        public void Hide()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            shown = 0;
            group.alpha = 0f;
            count.transform.localScale = Vector3.one;
        }

        private IEnumerator Pop()
        {
            group.alpha = 1f;
            Transform number = count.transform;
            for (float t = 0f; t < popSeconds; t += Time.deltaTime)
            {
                number.localScale = Vector3.one * Mathf.Lerp(popScale, 1f, t / popSeconds);
                yield return null;
            }
            number.localScale = Vector3.one;
            routine = null;
        }

        private IEnumerator Fade()
        {
            float from = group.alpha;
            for (float t = 0f; t < fadeSeconds; t += Time.deltaTime)
            {
                group.alpha = Mathf.Lerp(from, 0f, t / fadeSeconds);
                yield return null;
            }
            group.alpha = 0f;
            routine = null;
        }
    }
}
