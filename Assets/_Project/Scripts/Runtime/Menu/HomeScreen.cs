using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //Where the app lands: the companion, a big Continue into the next floor, and a peek at the Codex
    public class HomeScreen : MenuPanel
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_Text continueFloor;
        [SerializeField] private TMP_Text continueTitle;
        [SerializeField] private Button sealsButton;
        [SerializeField] private TMP_Text sealsCount;

        private StageData next;

        public override void Init(MenuRouter router)
        {
            base.Init(router);
            continueButton.onClick.AddListener(() => Router.PreviewFloor(next));
            sealsButton.onClick.AddListener(() => Router.Show(MenuScreen.Codex));
        }

        public override void Open()
        {
            base.Open();

            //Always floor 1 for now, it'll point at the first uncleared floor once progress is saved
            List<StageData> floors = Router.Floors();
            next = floors.Count > 0 ? floors[0] : null;
            continueButton.interactable = next != null;
            continueFloor.text = next == null ? "No floors yet" : "Realm " + next.realm + "  ·  Floor " + next.floor;
            continueTitle.text = next == null ? "Sync the content first" : next.title;
            sealsCount.text = Router.Seals().Count + " to find";
        }
    }
}
