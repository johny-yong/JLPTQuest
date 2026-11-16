using System.Collections;
using TMPro;
using UnityEngine;

namespace JLPTQuest
{
    //The "Wave 2/3" band that fades over the enemy while the next one steps in
    public class WaveBanner : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private float fadeSeconds = 0.2f;

        private Coroutine routine;

        void Awake()
        {
            group.alpha = 0f;
        }

        //Fades in, holds, and is gone again after seconds
        public void Play(string titleText, string subtitleText, float seconds)
        {
            title.text = titleText;
            subtitle.text = subtitleText;
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Run(seconds));
        }

        public void Hide()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            group.alpha = 0f;
        }

        private IEnumerator Run(float seconds)
        {
            float fade = Mathf.Min(fadeSeconds, seconds * 0.5f);
            for (float t = 0f; t < fade; t += Time.deltaTime)
            {
                group.alpha = t / fade;
                yield return null;
            }

            group.alpha = 1f;
            yield return new WaitForSeconds(Mathf.Max(0f, seconds - fade * 2f));

            for (float t = 0f; t < fade; t += Time.deltaTime)
            {
                group.alpha = 1f - t / fade;
                yield return null;
            }

            group.alpha = 0f;
            routine = null;
        }
    }
}
