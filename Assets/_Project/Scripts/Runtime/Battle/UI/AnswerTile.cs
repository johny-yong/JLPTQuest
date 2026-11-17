using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    public enum TileState { Idle, Locked, Right, Wrong, Missed }

    //One of the 2x2 answer tiles. The red ○ and × are how Japanese teachers mark work
    public class AnswerTile : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private RubyText label;
        [SerializeField] private TMP_Text mark;
        [SerializeField] private float maxFontSize = 72f;

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
            float width = ((RectTransform)LabelText.transform).rect.width;
            LabelText.fontSize = width > 0f ? Mathf.Min(maxSize, width / (length + 0.5f)) : maxSize;

            label.Markup = markup; //After the size, the furigana is measured at the new size
            SetState(TileState.Idle);
        }

        public void SetState(TileState state)
        {
            button.interactable = state == TileState.Idle;
            LabelText.color = state == TileState.Locked ? InkColors.InkFaint : InkColors.Ink;

            switch (state)
            {
                case TileState.Right:
                    background.color = InkColors.RightTint;
                    ShowMark("○");
                    break;
                case TileState.Missed: //The right answer the player didn't pick
                    background.color = InkColors.RightTint;
                    ShowMark(string.Empty);
                    break;
                case TileState.Wrong:
                    background.color = InkColors.WrongTint;
                    ShowMark("×");
                    break;
                case TileState.Locked: //Tiles nobody picked fade back
                    background.color = InkColors.Panel;
                    ShowMark(string.Empty);
                    break;
                default:
                    background.color = InkColors.Tile;
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
