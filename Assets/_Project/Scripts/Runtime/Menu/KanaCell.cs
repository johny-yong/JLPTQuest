using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //One square on the kana chart: the kana big, its romaji under it. Blank squares just hold the gap
    public class KanaCell : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text kana;
        [SerializeField] private TMP_Text romaji;

        private Action onPicked;

        void Awake()
        {
            button.onClick.AddListener(() => onPicked?.Invoke());
        }

        public void Show(string text, string reading, Action onPicked)
        {
            bool blank = string.IsNullOrEmpty(text);
            this.onPicked = onPicked;
            button.interactable = !blank;
            background.enabled = !blank;
            kana.text = blank ? "" : text;
            romaji.text = blank ? "" : reading;
            SetPicked(false);
        }

        //The tapped square goes ink coloured so you can see which one the detail bar is about
        public void SetPicked(bool picked)
        {
            background.color = picked ? InkColors.Ink : InkColors.Card;
            kana.color = picked ? InkColors.Paper : InkColors.Ink;
            romaji.color = picked ? InkColors.Panel : InkColors.InkSoft;
        }
    }
}
