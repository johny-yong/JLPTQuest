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
        [SerializeField] private TMP_Text speech; //Ryū's bubble, follows how far you've got

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

            //Continue points at the lowest floor not cleared yet, and sits on the boss once the realm is done
            List<StageData> floors = Router.Floors();
            Progress progress = Router.Progress;
            next = progress.NextFloor(floors);
            continueButton.interactable = next != null;

            if (next == null)
            {
                continueFloor.text = "No floors yet";
                continueTitle.text = "Sync the content first";
            }
            else if (progress.AllCleared(floors))
            {
                continueFloor.text = "Realm " + Router.Realm + " cleared!";
                continueTitle.text = "Rematch the boss or chase missing stars";
            }
            else
            {
                continueFloor.text = "Realm " + next.realm + "  ·  " + (Router.IsBoss(next) ? "Boss floor" : "Floor " + next.floor);
                continueTitle.text = next.title;
            }
            speech.text = SpeechLine(progress, floors);

            List<string> seals = Router.Seals();
            int found = 0;
            foreach (string seal in seals)
                if (progress.HasSeal(seal)) found++;
            sealsCount.text = found + " / " + seals.Count + " found";
        }

        private string SpeechLine(Progress progress, List<StageData> floors)
        {
            if (next == null) return "Hmm, the tower's empty.";
            if (progress.AllCleared(floors)) return "We did it! Rematch for stars?";
            if (Router.IsBoss(next)) return "The boss is next. Stay sharp!";
            if (next.floor == 1) return "Ready when you are. Floor 1 first!";
            return "Floor " + next.floor + " next. Let's go!";
        }
    }
}
