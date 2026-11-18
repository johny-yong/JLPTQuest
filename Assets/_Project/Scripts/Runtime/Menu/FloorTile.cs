using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    public enum FloorState { Locked, Next, Cleared }

    //One floor on the tower map: green badge and stars once cleared, red for the floor you're on, faded while locked
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

            //The locked look is done by hand below, the button's own grey-out would fade it twice
            ColorBlock colors = button.colors;
            colors.disabledColor = Color.white;
            button.colors = colors;
        }

        public void Show(StageData stage, bool boss, FloorState floorState, bool[] stars, Action onPicked)
        {
            this.onPicked = onPicked;
            bool locked = floorState == FloorState.Locked;
            bool cleared = floorState == FloorState.Cleared;
            button.interactable = !locked;

            badgeLabel.text = boss ? "鬼" : "F" + stage.floor;
            title.text = boss ? "Boss floor" : "Floor " + stage.floor;
            subtitle.text = stage.title;
            state.text = cleared ? StarText.Line(stars) : locked ? "Locked" : "Next";

            background.color = boss ? InkColors.Ink : locked ? InkColors.Panel : InkColors.Card;
            if (locked) badge.color = boss ? InkColors.InkSoft : InkColors.Tile;
            else badge.color = cleared && !boss ? InkColors.Matcha : InkColors.Vermilion;
            badgeLabel.color = locked && !boss ? InkColors.InkFaint : InkColors.Paper;
            title.color = boss ? InkColors.Paper : locked ? InkColors.InkFaint : InkColors.Ink;
            subtitle.color = boss ? InkColors.Tile : locked ? InkColors.InkFaint : InkColors.InkSoft;
            state.color = floorState == FloorState.Next ? InkColors.Vermilion : InkColors.InkFaint;
        }
    }
}
