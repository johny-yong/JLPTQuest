using System;
using System.Collections.Generic;
using UnityEngine;

namespace JLPTQuest
{
    //The 2x2 grid. Tells the controller which tile got tapped and paints the result
    public class AnswerTilesView : MonoBehaviour
    {
        [SerializeField] private List<AnswerTile> tiles = new List<AnswerTile>();

        public event Action<int> Picked;

        void Awake()
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                int index = i; //Captured per tile, not the loop variable
                tiles[i].Button.onClick.AddListener(() => Picked?.Invoke(index));
            }
        }

        public void Show(IList<string> options)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                bool used = i < options.Count;
                tiles[i].gameObject.SetActive(used);
                if (used) tiles[i].SetOption(options[i]);
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
