using System;
using System.Collections.Generic;
using UnityEngine;

namespace JLPTQuest
{
    /*
        The answer grid. Tells the controller which tile got tapped and paints the result.
        Normal is 2x2, Hard and Very hard add rows in the same zone: 2x3 and 2x4. An odd tile out sits centred on the last row.
        Parry replies are whole sentences, so for those the tiles stack into full width bars.
    */
    public class AnswerTilesView : MonoBehaviour
    {
        [SerializeField] private List<AnswerTile> tiles = new List<AnswerTile>(); //Enough for Very hard, the spare ones hide
        [SerializeField] private float replyFontSize = 48f;
        [SerializeField] private float replyGap = 0.02f; //Between bars, as a share of the zone height
        [SerializeField] private float columnGap = 0.04f; //Between the two columns, as a share of the zone width
        [SerializeField] private float rowGap = 0.04f; //Between rows, as a share of the zone height

        public event Action<int> Picked;

        void Awake()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                int index = i; //Captured per tile, not the loop variable
                tiles[i].Button.onClick.AddListener(() => Picked?.Invoke(index));
            }
        }

        public void Show(IList<string> options, bool asReplies = false)
        {
            int count = Mathf.Min(options.Count, tiles.Count);
            Arrange(count, asReplies);
            for (int i = 0; i < tiles.Count; i++)
            {
                bool used = i < count;
                tiles[i].gameObject.SetActive(used);
                if (!used) continue;
                if (asReplies) tiles[i].SetOption(options[i], replyFontSize);
                else tiles[i].SetOption(options[i]);
            }
        }

        private void Arrange(int count, bool asReplies)
        {
            if (count == 0) return;

            int rows = (count + 1) / 2;
            float rowHeight = (1f - (rows - 1) * rowGap) / rows;
            float columnWidth = (1f - columnGap) * 0.5f;
            float barStep = 1f / count;

            for (int i = 0; i < count; i++)
            {
                var rect = (RectTransform)tiles[i].transform;
                if (asReplies)
                {
                    float top = 1f - i * barStep;
                    rect.anchorMin = new Vector2(0f, top - barStep + replyGap * 0.5f);
                    rect.anchorMax = new Vector2(1f, top - replyGap * 0.5f);
                }
                else
                {
                    int row = i / 2;
                    bool alone = count % 2 == 1 && i == count - 1;
                    float left = alone ? (1f - columnWidth) * 0.5f : (i % 2) * (columnWidth + columnGap);
                    float top = 1f - row * (rowHeight + rowGap);
                    rect.anchorMin = new Vector2(left, top - rowHeight);
                    rect.anchorMax = new Vector2(left + columnWidth, top);
                }
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        public void Clear()
        {
            Show(Array.Empty<string>());
        }

        //50/50: those tiles fade out and can't be tapped
        public void Remove(IList<int> indices)
        {
            foreach (int index in indices)
                if (index >= 0 && index < tiles.Count) tiles[index].SetState(TileState.Removed);
        }

        public void ShowResult(RoundResult result)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                bool isRight = i == result.round.correctIndex;
                bool isChosen = i == result.chosenIndex;

                TileState state = result.round.removedOptions.Contains(i) ? TileState.Removed : TileState.Locked;
                if (isRight) state = isChosen ? TileState.Right : TileState.Missed;
                else if (isChosen) state = TileState.Wrong;
                tiles[i].SetState(state);
            }
        }
    }
}
