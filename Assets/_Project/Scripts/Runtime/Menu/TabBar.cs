using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //Home 家, Tower 塔, Codex 辞, Companions 友 along the bottom. Arrays go in that same order
    public class TabBar : MonoBehaviour
    {
        [SerializeField] private Button[] tabs;
        [SerializeField] private Image[] icons;
        [SerializeField] private TMP_Text[] glyphs;
        [SerializeField] private TMP_Text[] labels;

        public event Action<MenuScreen> Picked;

        void Awake()
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                MenuScreen screen = (MenuScreen)i; //Own copy per tab, or every button would open the last one
                tabs[i].onClick.AddListener(() => Picked?.Invoke(screen));
            }
        }

        //Settings isn't a tab, so nothing lights up while it's open
        public void Select(MenuScreen screen)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                bool on = (int)screen == i;
                icons[i].color = on ? InkColors.Vermilion : InkColors.Tile;
                glyphs[i].color = on ? InkColors.Paper : InkColors.InkSoft;
                labels[i].color = on ? InkColors.Ink : InkColors.InkSoft;
            }
        }
    }
}
