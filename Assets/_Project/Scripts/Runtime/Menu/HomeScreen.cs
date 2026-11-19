using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        Where the app lands: the companion standing in tonight's scene, their line for the moment,
        the stars and seals so far, and one big Continue into the next floor.
        Everything the companion says comes from their CompanionData, so it's all editable in the Inspector.
    */
    public class HomeScreen : MenuPanel
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_Text continueLabel; //Continue, or Rematch once the realm is done
        [SerializeField] private TMP_Text continueDetail; //"Floor 3  ·  Where is it?"
        [SerializeField] private Button sealsButton;
        [SerializeField] private TMP_Text sealsCount;
        [SerializeField] private TMP_Text starsCount;
        [SerializeField] private TMP_Text kicker; //"REALM 1  ·  LESSONS 1–6"
        [SerializeField] private TMP_Text speech;
        [SerializeField] private Image companionArt;
        [SerializeField] private Image companionGlow; //Rim light from the scene behind them
        [SerializeField] private SceneryView scenery;

        private StageData next;
        private Vector2 artRest;
        private bool artRestKnown;

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
            bool allCleared = next != null && progress.AllCleared(floors);

            continueLabel.text = allCleared ? "Rematch" : "Continue";
            if (next == null) continueDetail.text = "Sync the content first";
            else if (allCleared) continueDetail.text = "Realm " + Router.Realm + " cleared  ·  chase the missing stars";
            else continueDetail.text = (Router.IsBoss(next) ? "Boss floor" : "Floor " + next.floor) + "  ·  " + next.title;

            kicker.text = "REALM " + Router.Realm + Lessons(floors);
            starsCount.text = "<color=" + InkColors.ToTag(InkColors.GoldLight) + ">★</color> " + progress.TotalStars(floors) + " / " + floors.Count * 3;

            int found = 0;
            foreach (string seal in Router.Seals())
                if (progress.HasSeal(seal)) found++;
            sealsCount.text = "<color=" + InkColors.ToTag(InkColors.VermilionGlow) + ">封</color> " + found + (found == 1 ? " seal" : " seals");

            ShowCompanion(Moment(progress, floors, allCleared));
        }

        private void ShowCompanion(HomeMoment moment)
        {
            CompanionData companion = Router.Companion;
            if (companion == null)
            {
                speech.text = string.Empty;
                return;
            }

            if (companionArt != null)
            {
                RectTransform rect = companionArt.rectTransform;
                if (!artRestKnown) { artRest = rect.anchoredPosition; artRestKnown = true; }
                companionArt.sprite = companion.portrait;
                companionArt.enabled = companion.portrait != null;
                companionArt.preserveAspect = true;
                rect.localScale = new Vector3(companion.artFacesLeft ? -companion.homeScale : companion.homeScale, companion.homeScale, 1f);
                rect.anchoredPosition = artRest + companion.homeOffset;
            }
            if (companionGlow != null && scenery != null && scenery.Tint != null) companionGlow.color = scenery.Tint.companionGlow;

            int floor = next != null ? next.floor : 1;
            string title = next != null ? next.title : string.Empty;
            speech.text = CompanionTalk.Home(companion.lines, moment, TimeOfDayClock.Now, floor, title, companion.displayName, CompanionTalk.DaySeed(DateTime.Now));
        }

        private HomeMoment Moment(Progress progress, List<StageData> floors, bool allCleared)
        {
            if (next == null) return HomeMoment.NoFloors;
            if (allCleared) return HomeMoment.RealmCleared;
            if (Router.IsBoss(next)) return HomeMoment.BossNext;
            if (next.floor == 1 && !progress.IsCleared(next.id)) return HomeMoment.FirstFloor;
            return HomeMoment.Floor;
        }

        //"  ·  LESSONS 1–6" from the lessons this realm's floors cover
        private static string Lessons(List<StageData> floors)
        {
            int low = int.MaxValue, high = int.MinValue;
            foreach (StageData floor in floors)
                foreach (int lesson in floor.lessons)
                {
                    low = Mathf.Min(low, lesson);
                    high = Mathf.Max(high, lesson);
                }
            if (low == int.MaxValue) return string.Empty;
            return low == high ? "  ·  LESSON " + low : "  ·  LESSONS " + low + "–" + high;
        }
    }
}
