using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        The companion's big moment: a band across the enemy where the art slides in with "Counter!" or "Perfect!".
        Only over the enemy area, so the filled in sentence and the red ○ below stay readable.
        Lasts about as long as the reveal, so it never runs into the next question.
    */
    public class CutInView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private RectTransform art; //Slides in from the left
        [SerializeField] private Image artImage;
        [SerializeField] private TMP_Text glyph; //Stands in when the companion has no art
        [SerializeField] private RectTransform words; //Slides in from the right
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField, Range(0.05f, 0.5f)] private float slideShare = 0.3f; //Part of the time spent sliding in
        [SerializeField] private float slideDistance = 600f;

        private Vector2 artRest;
        private Vector2 wordsRest;
        private Coroutine routine;

        void Awake()
        {
            artRest = art.anchoredPosition;
            wordsRest = words.anchoredPosition;
            group.alpha = 0f;
        }

        public void Play(Sprite companionArt, string companionGlyph, string titleText, string subtitleText, float seconds)
        {
            bool hasArt = companionArt != null;
            artImage.sprite = companionArt;
            artImage.enabled = hasArt;
            glyph.text = companionGlyph;
            glyph.enabled = !hasArt;
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
            float slide = seconds * slideShare;
            float fade = seconds * 0.25f;

            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                float inK = Mathf.Clamp01(t / slide);
                float ease = 1f - (1f - inK) * (1f - inK) * (1f - inK); //Fast in, soft landing
                art.anchoredPosition = artRest + Vector2.left * (slideDistance * (1f - ease));
                words.anchoredPosition = wordsRest + Vector2.right * (slideDistance * (1f - ease));

                float left = seconds - t;
                group.alpha = Mathf.Min(inK * 3f, left < fade ? left / fade : 1f);
                yield return null;
            }

            group.alpha = 0f;
            art.anchoredPosition = artRest;
            words.anchoredPosition = wordsRest;
            routine = null;
        }
    }
}
