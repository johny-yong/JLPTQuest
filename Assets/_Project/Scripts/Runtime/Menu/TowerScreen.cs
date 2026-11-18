using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //The realm's floors stacked up like a tower, boss on top. Tapping an open floor opens its preview
    public class TowerScreen : MenuPanel
    {
        [SerializeField] private FloorTile tileTemplate; //Kept switched off, every floor is a copy of it
        [SerializeField] private RectTransform floorList;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private TMP_Text realmLabel;

        private readonly List<FloorTile> tiles = new List<FloorTile>();

        public override void Init(MenuRouter router)
        {
            base.Init(router);
            tileTemplate.gameObject.SetActive(false);
        }

        public override void Open()
        {
            base.Open();
            Rebuild();

            //Start at the bottom, you climb up
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 0f;
        }

        private void Rebuild()
        {
            foreach (FloorTile tile in tiles) Destroy(tile.gameObject);
            tiles.Clear();

            Progress progress = Router.Progress;
            List<StageData> floors = Router.Floors();
            for (int i = floors.Count - 1; i >= 0; i--)
            {
                StageData stage = floors[i];
                FloorState state = progress.IsCleared(stage.id) ? FloorState.Cleared : progress.IsUnlocked(stage, floors) ? FloorState.Next : FloorState.Locked;
                FloorTile tile = Instantiate(tileTemplate, floorList);
                tile.gameObject.SetActive(true);
                tile.Show(stage, Router.IsBoss(stage), state, progress.Stars(stage.id), progress.HardestCleared(stage.id), () => Router.PreviewFloor(stage));
                tiles.Add(tile);
            }

            realmLabel.text = "Realm " + Router.Realm + "  ·  ★ " + progress.TotalStars(floors) + " / " + floors.Count * 3;
        }
    }
}
