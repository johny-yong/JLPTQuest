using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //One floor on the tower map
    public class FloorTile : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Image badge;
        [SerializeField] private TMP_Text badgeLabel;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private TMP_Text state;

        private Action onPicked;

        void Awake()
        {
            button.onClick.AddListener(() => onPicked?.Invoke());
        }

        //Every floor is open and starless for now, locks and stars come with progress saving
        public void Show(StageData stage, bool boss, Action onPicked)
        {
            this.onPicked = onPicked;

            badgeLabel.text = boss ? "鬼" : "F" + stage.floor;
            title.text = boss ? "Boss floor" : "Floor " + stage.floor;
            subtitle.text = stage.title;
            state.text = "☆☆☆";

            background.color = boss ? InkColors.Ink : InkColors.Card;
            badge.color = boss ? InkColors.Vermilion : InkColors.Tile;
            badgeLabel.color = boss ? InkColors.Paper : InkColors.Ink;
            title.color = boss ? InkColors.Paper : InkColors.Ink;
            subtitle.color = boss ? InkColors.Tile : InkColors.InkSoft;
            state.color = InkColors.InkFaint;
        }
    }
}
