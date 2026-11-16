using System.Collections;
using TMPro;
using UnityEngine;

namespace JLPTQuest
{
    //The mojibake enemy: a face made of glyphs, its HP and attack countdown, and the damage number
    public class EnemyView : MonoBehaviour
    {
        [SerializeField] private TMP_Text eyes;
        [SerializeField] private TMP_Text nose;
        [SerializeField] private TMP_Text mouth;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text cooldownLabel;
        [SerializeField] private TMP_Text damageLabel;
        [SerializeField] private BarFill hpBar;
        [SerializeField] private CanvasGroup body; //The face, fades out when beaten
        [SerializeField] private float enterSeconds = 0.25f;
        [SerializeField] private float damageRise = 90f;
        [SerializeField] private float damageSeconds = 0.7f;

        private Vector2 damageStart;
        private int shownCooldown;
        private Coroutine damageRoutine;
        private Coroutine attackRoutine;

        void Awake()
        {
            damageStart = damageLabel.rectTransform.anchoredPosition;
            damageLabel.alpha = 0f;
        }

        public void Show(EnemyData data, EnemyState state)
        {
            StopAllCoroutines();
            damageRoutine = null;
            attackRoutine = null;

            eyes.text = data.faceEyes;
            nose.text = data.faceNose;
            mouth.text = data.faceMouth;
            nameLabel.text = NameLine(data);
            cooldownLabel.color = InkColors.Ink;
            damageLabel.alpha = 0f;
            damageLabel.rectTransform.anchoredPosition = damageStart;
            Refresh(state);
            StartCoroutine(FadeBody(0f, 1f, 1f, 1f, enterSeconds));
        }

        public void ShowDefeated(float seconds)
        {
            StartCoroutine(FadeBody(1f, 0f, 1f, 0.85f, seconds));
        }

        //"Dokodemo  どこでも", with Elite or Boss in front when it's not a grunt
        public static string NameLine(EnemyData data)
        {
            string name = string.IsNullOrEmpty(data.nameJp) ? data.displayName : data.displayName + "  " + data.nameJp;
            if (data.kind == EnemyKind.Grunt) return name;
            return "<color=" + InkColors.ToTag(InkColors.Vermilion) + ">" + data.kind + "</color>  " + name;
        }

        public void Refresh(EnemyState state)
        {
            hpBar.Value = state.HpFraction;
            shownCooldown = Mathf.Max(0, state.CooldownLeft);
            cooldownLabel.text = "CD " + shownCooldown;
        }

        public void ShowHit(DamageBreakdown hit)
        {
            damageLabel.text = hit.perfect ? hit.total + "  会心" : hit.total.ToString();
            damageLabel.color = hit.perfect ? InkColors.Vermilion : InkColors.Ink;
            if (damageRoutine != null) StopCoroutine(damageRoutine);
            damageRoutine = StartCoroutine(FloatDamage());
        }

        //Parry turns replace this in Phase 3, for now the badge just flashes
        public void ShowAttack()
        {
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(FlashCooldown());
        }

        private IEnumerator FloatDamage()
        {
            RectTransform rect = damageLabel.rectTransform;
            for (float t = 0f; t < damageSeconds; t += Time.deltaTime)
            {
                float k = t / damageSeconds;
                rect.anchoredPosition = damageStart + Vector2.up * (damageRise * k);
                damageLabel.alpha = 1f - k * k;
                yield return null;
            }
            damageLabel.alpha = 0f;
            rect.anchoredPosition = damageStart;
        }

        private IEnumerator FadeBody(float fromAlpha, float toAlpha, float fromScale, float toScale, float seconds)
        {
            Transform face = body.transform;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                float k = t / seconds;
                body.alpha = Mathf.Lerp(fromAlpha, toAlpha, k);
                face.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, k);
                yield return null;
            }
            body.alpha = toAlpha;
            face.localScale = Vector3.one * toScale;
        }

        private IEnumerator FlashCooldown()
        {
            cooldownLabel.text = "Attack!";
            cooldownLabel.color = InkColors.Vermilion;
            yield return new WaitForSeconds(0.8f);
            cooldownLabel.color = InkColors.Ink;
            cooldownLabel.text = "CD " + shownCooldown;
        }
    }
}
