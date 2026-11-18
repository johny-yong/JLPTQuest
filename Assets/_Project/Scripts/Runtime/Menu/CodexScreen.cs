using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JLPTQuest
{
    //Every Word Seal in the realm, with furigana
    public class CodexScreen : MenuPanel
    {
        [SerializeField] private TMP_Text countLabel;
        [SerializeField] private RectTransform grid;
        [SerializeField] private GameObject sealTemplate; //Kept switched off, needs a RubyText somewhere inside

        private readonly List<GameObject> tiles = new List<GameObject>();

        public override void Init(MenuRouter router)
        {
            base.Init(router);
            sealTemplate.SetActive(false);
        }

        public override void Open()
        {
            base.Open();

            foreach (GameObject tile in tiles) Destroy(tile);
            tiles.Clear();

            //Shows every seal for now, found vs still hidden needs progress saving first
            List<string> seals = Router.Seals();
            foreach (string seal in seals)
            {
                GameObject tile = Instantiate(sealTemplate, grid);
                tile.SetActive(true);
                tile.GetComponentInChildren<RubyText>().Markup = seal;
                tiles.Add(tile);
            }
            countLabel.text = seals.Count + " Word Seals in Realm " + Router.Realm;
        }
    }
}
