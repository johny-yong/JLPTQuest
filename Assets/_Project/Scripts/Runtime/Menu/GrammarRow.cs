using System;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //One grammar point in the Codex. Met ones show their name and pattern and open the scroll, the rest stay hidden
    public class GrammarRow : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private RubyText title;
        [SerializeField] private RubyText pattern;

        private Action onPicked;

        void Awake()
        {
            button.onClick.AddListener(() => onPicked?.Invoke());
            ColorBlock colors = button.colors;
            colors.disabledColor = Color.white; //The hidden look is done by hand
            button.colors = colors;
        }

        public void Show(GrammarData point, bool met, Action onPicked)
        {
            this.onPicked = onPicked;
            button.interactable = met;
            background.color = met ? InkColors.Card : InkColors.Panel;
            title.Markup = met ? point.name : "？  Not met yet";
            title.GetComponent<TMPro.TMP_Text>().color = met ? InkColors.Ink : InkColors.InkFaint;
            pattern.gameObject.SetActive(met);
            if (met) pattern.Markup = point.pattern;
        }
    }
}
