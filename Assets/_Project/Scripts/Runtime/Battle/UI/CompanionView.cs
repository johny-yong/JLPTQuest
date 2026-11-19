using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        Your shikigami, big on the left behind the HUD, facing the enemy.
        The art comes from the equipped companion's CompanionData. With no art it falls back to a panel with a glyph,
        hurt and worried tint whichever is showing.
        Real art and Feel players come in Phase 8 through these same calls, so the battle code never changes.

        Rule: big moves only happen after an answer (attack, block, hurt, cheer).
        While the clock runs the only change is the worried tint, so it never pulls eyes off the question.
    */
    public class CompanionView : MonoBehaviour
    {
        [SerializeField] private RectTransform body;
        [SerializeField] private Image bodyImage;
        [SerializeField] private TMP_Text glyph;
        [SerializeField] private Image portrait;
        [SerializeField] private CompanionRoster roster;
        [SerializeField] private bool flipArt; //Tick if the art faces left, the companion should face the enemy
        [SerializeField, Range(0f, 1f)] private float worriedTint = 0.5f;
        [SerializeField] private TMP_Text popup;
        [SerializeField] private float moveSeconds = 0.3f;
        [SerializeField] private Vector2 lunge = new Vector2(60f, 50f); //Toward the enemy

        private Vector2 restPosition;
        private Vector2 popupStart;
        private Color restColor;
        private Graphic tintTarget; //The art when there is some, the stand-in panel when not
        private bool worried;
        private Coroutine moveRoutine;
        private Coroutine popupRoutine;

        void Awake()
        {
            CompanionData companion = roster != null ? roster.Equipped : null;
            if (companion != null && companion.portrait != null)
            {
                portrait.sprite = companion.portrait;
                flipArt = companion.artFacesLeft;
                glyph.text = companion.glyph;
            }
            bool hasArt = portrait.sprite != null;
            portrait.enabled = hasArt;
            glyph.enabled = !hasArt;
            bodyImage.enabled = !hasArt;
            portrait.rectTransform.localScale = new Vector3(flipArt ? -1f : 1f, 1f, 1f);
            tintTarget = hasArt ? (Graphic)portrait : bodyImage;

            restPosition = body.anchoredPosition;
            popupStart = popup.rectTransform.anchoredPosition;
            restColor = tintTarget.color;
            popup.alpha = 0f;
        }

        public Sprite Art => portrait.sprite;
        public string Glyph => glyph.text;

        public void ResetPose()
        {
            StopAllCoroutines();
            moveRoutine = null;
            popupRoutine = null;
            body.anchoredPosition = restPosition;
            body.localScale = Vector3.one;
            popup.alpha = 0f;
            worried = false;
            tintTarget.color = restColor;
        }

        public void Attack()
        {
            Move(Lunge());
        }

        public void Block()
        {
            Move(Pulse());
            Pop("Blocked!", InkColors.Matcha);
        }

        public void Hurt(int damage)
        {
            Move(Shake());
            Pop("-" + damage, InkColors.VermilionGlow);
        }

        public void Cheer()
        {
            Move(Hop());
        }

        //Low HP or the last seconds of the clock. A tint, no movement
        public void SetWorried(bool value)
        {
            if (value == worried) return;
            worried = value;
            tintTarget.color = RestingColor();
        }

        private Color RestingColor()
        {
            return worried ? Color.Lerp(restColor, InkColors.WrongTint, worriedTint) : restColor;
        }

        private void Move(IEnumerator routine)
        {
            if (moveRoutine != null) StopCoroutine(moveRoutine);
            body.anchoredPosition = restPosition;
            body.localScale = Vector3.one;
            moveRoutine = StartCoroutine(routine);
        }

        private IEnumerator Lunge()
        {
            for (float t = 0f; t < moveSeconds; t += Time.deltaTime)
            {
                float k = Mathf.Sin(t / moveSeconds * Mathf.PI); //Out and back
                body.anchoredPosition = restPosition + lunge * k;
                yield return null;
            }
            body.anchoredPosition = restPosition;
        }

        private IEnumerator Pulse()
        {
            for (float t = 0f; t < moveSeconds; t += Time.deltaTime)
            {
                body.localScale = Vector3.one * (1f + 0.15f * Mathf.Sin(t / moveSeconds * Mathf.PI));
                yield return null;
            }
            body.localScale = Vector3.one;
        }

        private IEnumerator Shake()
        {
            Color hurtColor = InkColors.WrongTint;
            for (float t = 0f; t < moveSeconds * 1.3f; t += Time.deltaTime)
            {
                float fade = 1f - t / (moveSeconds * 1.3f);
                body.anchoredPosition = restPosition + Vector2.right * (Mathf.Sin(t * 60f) * 14f * fade);
                tintTarget.color = Color.Lerp(RestingColor(), hurtColor, fade);
                yield return null;
            }
            body.anchoredPosition = restPosition;
            tintTarget.color = RestingColor();
        }

        private IEnumerator Hop()
        {
            for (int hop = 0; hop < 2; hop++)
            {
                for (float t = 0f; t < moveSeconds * 0.7f; t += Time.deltaTime)
                {
                    body.anchoredPosition = restPosition + Vector2.up * (36f * Mathf.Sin(t / (moveSeconds * 0.7f) * Mathf.PI));
                    yield return null;
                }
            }
            body.anchoredPosition = restPosition;
        }

        private void Pop(string text, Color color)
        {
            popup.text = text;
            popup.color = color;
            if (popupRoutine != null) StopCoroutine(popupRoutine);
            popupRoutine = StartCoroutine(FloatPopup());
        }

        private IEnumerator FloatPopup()
        {
            RectTransform rect = popup.rectTransform;
            for (float t = 0f; t < 0.8f; t += Time.deltaTime)
            {
                float k = t / 0.8f;
                rect.anchoredPosition = popupStart + Vector2.up * (60f * k);
                popup.alpha = 1f - k * k;
                yield return null;
            }
            popup.alpha = 0f;
            rect.anchoredPosition = popupStart;
            popupRoutine = null;
        }
    }
}
