using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    public enum TileState { Idle, Locked, Right, Wrong, Missed, Removed }

    //One of the answer tiles, lacquer on night. The ○ and × are how Japanese teachers mark work
    public class AnswerTile : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private RubyText label;
        [SerializeField] private TMP_Text mark;
        [SerializeField] private float maxFontSize = 72f;
        [SerializeField] private float heightShare = 0.6f; //Font can't go past this much of the label's height, matters on the short Very hard tiles

        private TMP_Text labelText;

        public Button Button => button;
        private TMP_Text LabelText => labelText != null ? labelText : labelText = label.GetComponent<TMP_Text>();

        public void SetOption(string markup)
        {
            SetOption(markup, maxFontSize);
        }

        public void SetOption(string markup, float maxSize)
        {
            //Long answers like じゃありません shrink to stay on one line. Kana and kanji are about 1 em wide
            int length = Mathf.Max(1, RubyMarkup.StripReadings(markup).Length);
            Rect area = ((RectTransform)LabelText.transform).rect;
            float size = area.width > 0f ? Mathf.Min(maxSize, area.width / (length + 0.5f)) : maxSize;
            if (area.height > 0f) size = Mathf.Min(size, area.height * heightShare); //Leaves room for furigana on short tiles
            LabelText.fontSize = size;

            label.Markup = markup; //After the size, the furigana is measured at the new size
            SetState(TileState.Idle);
        }

        public void SetState(TileState state)
        {
            button.interactable = state == TileState.Idle;
            Color text = state == TileState.Locked ? InkColors.MoonDim : InkColors.Moon;
            if (state == TileState.Removed) text.a = 0.2f; //50/50 took it, nearly gone
            LabelText.color = text;

            switch (state)
            {
                case TileState.Right:
                    background.color = InkColors.TileRight;
                    ShowMark("○");
                    break;
                case TileState.Missed: //The right answer the player didn't pick
                    background.color = InkColors.TileRight;
                    ShowMark(string.Empty);
                    break;
                case TileState.Wrong:
                    background.color = InkColors.TileWrong;
                    ShowMark("×");
                    break;
                case TileState.Removed:
                case TileState.Locked: //Tiles nobody picked fade back
                    background.color = InkColors.LacquerLow;
                    ShowMark(string.Empty);
                    break;
                default:
                    background.color = InkColors.Lacquer;
                    ShowMark(string.Empty);
                    break;
            }
        }

        private void ShowMark(string text)
        {
            mark.text = text;
            mark.enabled = text.Length > 0;
        }
    }
}
