using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        The hint charges and the four hint buttons, in the HP row.
        Buttons line up with HintKind: translate, 50/50, first kana, +5 seconds.
        A button is only lit while that hint can actually be used on the question that's up.
    */
    public class HintBarView : MonoBehaviour
    {
        [SerializeField] private Button[] buttons = new Button[4];
        [SerializeField] private Image[] backgrounds = new Image[4];
        [SerializeField] private TMP_Text[] labels = new TMP_Text[4];
        [SerializeField] private Image[] chargeDots = new Image[2];
        [SerializeField] private TMP_Text refillPopup;
        [SerializeField, Range(0f, 1f)] private float offAlpha = 0.35f;

        public event Action<HintKind> Picked;

        private Coroutine popupRoutine;

        void Awake()
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                HintKind kind = (HintKind)i; //Captured per button
                buttons[i].onClick.AddListener(() => Picked?.Invoke(kind));
            }
            refillPopup.alpha = 0f;
        }

        public void Refresh(BattleSession session)
        {
            for (int i = 0; i < chargeDots.Length; i++)
                chargeDots[i].color = i < session.HintCharges ? InkColors.VermilionGlow : InkColors.LacquerLow;

            QuestionRound round = session.Phase == BattlePhase.Asking ? session.Round : null;
            for (int i = 0; i < buttons.Length; i++)
            {
                var kind = (HintKind)i;
                bool used = round != null && round.UsedHint(kind);
                bool usable = session.CanUseHint(kind);

                buttons[i].interactable = usable;
                backgrounds[i].color = used ? InkColors.TileRight : InkColors.Lacquer;
                Color text = InkColors.Moon;
                text.a = usable || used ? 1f : offAlpha;
                labels[i].color = text;
            }
        }

        public void ShowRefill()
        {
            if (popupRoutine != null) StopCoroutine(popupRoutine);
            popupRoutine = StartCoroutine(Popup());
        }

        private IEnumerator Popup()
        {
            refillPopup.alpha = 1f;
            yield return new WaitForSeconds(0.9f);
            for (float t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                refillPopup.alpha = 1f - t / 0.3f;
                yield return null;
            }
            refillPopup.alpha = 0f;
            popupRoutine = null;
        }
    }
}
