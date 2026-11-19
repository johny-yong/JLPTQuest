using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //Home 家, Tower 塔, Codex 辞, Companions 友 and Settings 設 along the bottom. Arrays go in that same order, the MenuScreen order
    public class TabBar : MonoBehaviour
    {
        [SerializeField] private Button[] tabs;
        [SerializeField] private Image[] icons;
        [SerializeField] private TMP_Text[] glyphs;
        [SerializeField] private TMP_Text[] labels;
        [SerializeField] private Image[] glows; //Soft vermilion glow behind the open tab
        [SerializeField] private Sprite onIcon; //Filled disc
        [SerializeField] private Sprite offIcon; //Thin gold ring

        public event Action<MenuScreen> Picked;

        void Awake()
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                MenuScreen screen = (MenuScreen)i; //Own copy per tab, or every button would open the last one
                tabs[i].onClick.AddListener(() => Picked?.Invoke(screen));
            }
        }

        //Hidden behind the title and the intro, they're full screen moments
        public void SetVisible(bool visible)
        {
            CanvasGroup group = GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();
            group.alpha = visible ? 1f : 0f;
            group.blocksRaycasts = visible;
            group.interactable = visible;
        }

        //The open screen's tab turns vermilion and glows, glow means "you are here"
        public void Select(MenuScreen screen)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                bool on = (int)screen == i;
                icons[i].sprite = on ? onIcon : offIcon;
                icons[i].color = on ? InkColors.Vermilion : InkColors.WithAlpha(InkColors.Gold, 0.4f);
                glyphs[i].color = on ? InkColors.OnVermilion : InkColors.MoonSoft;
                labels[i].color = on ? InkColors.Moon : InkColors.MoonFaint;
                if (glows != null && i < glows.Length && glows[i] != null) glows[i].enabled = on;
            }
        }
    }
}
