using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //The realm's floors stacked up like a tower, boss on top. Tapping one opens its preview
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

            List<StageData> floors = Router.Floors();
            for (int i = floors.Count - 1; i >= 0; i--)
            {
                StageData stage = floors[i];
                FloorTile tile = Instantiate(tileTemplate, floorList);
                tile.gameObject.SetActive(true);
                tile.Show(stage, Router.IsBoss(stage), () => Router.PreviewFloor(stage));
                tiles.Add(tile);
            }

            realmLabel.text = "Realm " + Router.Realm + "  ·  " + floors.Count + " floors";
        }
    }
}
