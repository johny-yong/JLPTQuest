using System;
using System.Collections.Generic;
using UnityEngine;

namespace JLPTQuest
{
    /*
        The 2x2 grid. Tells the controller which tile got tapped and paints the result.
        Parry replies are whole sentences, so for those the same 4 tiles stack into full width bars.
    */
    public class AnswerTilesView : MonoBehaviour
    {
        [SerializeField] private List<AnswerTile> tiles = new List<AnswerTile>();
        [SerializeField] private float replyFontSize = 48f;
        [SerializeField] private float replyGap = 0.02f; //Between bars, as a share of the zone height

        public event Action<int> Picked;

        private Vector2[] gridMin;
        private Vector2[] gridMax;

        void Awake()
        {
            //The grid is however the tiles were laid out in the prefab
            gridMin = new Vector2[tiles.Count];
            gridMax = new Vector2[tiles.Count];
            for (int i = 0; i < tiles.Count; i++)
            {
                var rect = (RectTransform)tiles[i].transform;
                gridMin[i] = rect.anchorMin;
                gridMax[i] = rect.anchorMax;
            }

            for (int i = 0; i < tiles.Count; i++)
            {
                int index = i; //Captured per tile, not the loop variable
                tiles[i].Button.onClick.AddListener(() => Picked?.Invoke(index));
            }
        }

        public void Show(IList<string> options, bool asReplies = false)
        {
            Arrange(asReplies);
            for (int i = 0; i < tiles.Count; i++)
            {
                bool used = i < options.Count;
                tiles[i].gameObject.SetActive(used);
                if (!used) continue;
                if (asReplies) tiles[i].SetOption(options[i], replyFontSize);
                else tiles[i].SetOption(options[i]);
            }
        }

        private void Arrange(bool asReplies)
        {
            float step = 1f / Mathf.Max(1, tiles.Count);
            for (int i = 0; i < tiles.Count; i++)
            {
                var rect = (RectTransform)tiles[i].transform;
                if (asReplies)
                {
                    float top = 1f - i * step;
                    rect.anchorMin = new Vector2(0f, top - step + replyGap * 0.5f);
                    rect.anchorMax = new Vector2(1f, top - replyGap * 0.5f);
                }
                else
                {
                    rect.anchorMin = gridMin[i];
                    rect.anchorMax = gridMax[i];
                }
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        public void Clear()
        {
            Show(Array.Empty<string>());
        }

        public void ShowResult(RoundResult result)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                bool isRight = i == result.round.correctIndex;
                bool isChosen = i == result.chosenIndex;

                TileState state = TileState.Locked;
                if (isRight) state = isChosen ? TileState.Right : TileState.Missed;
                else if (isChosen) state = TileState.Wrong;
                tiles[i].SetState(state);
            }
        }
    }
}
