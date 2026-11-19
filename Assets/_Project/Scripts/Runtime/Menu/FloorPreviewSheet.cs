using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        The sheet that slides over the menu when you pick a floor: who you'll fight, the difficulty, then Start battle.
        Hard and Very hard stay greyed out until the floor is cleared on Normal.
        The last difficulty you picked sticks, so grinding a boss on Very hard doesn't mean picking it every time.
    */
    public class FloorPreviewSheet : MenuPanel
    {
        [SerializeField] private Button dimButton; //Tapping outside the sheet closes it too
        [SerializeField] private Button closeButton;
        [SerializeField] private Button startButton;
        [SerializeField] private Button scrollButton;
        [SerializeField] private TMP_Text kicker;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text details;
        [SerializeField] private EnemyChip chipTemplate; //Kept switched off, one copy per wave
        [SerializeField] private RectTransform enemyRow;
        [SerializeField] private TMP_Text best;
        [SerializeField] private BattleConfig config; //For each difficulty's choices and extra seconds
        [SerializeField] private Button[] difficultyButtons = new Button[3]; //Normal, Hard, Very hard, in that order
        [SerializeField] private TMP_Text difficultyNote;

        private static Difficulty lastPicked;

        private StageData stage;
        private Difficulty picked;
        private readonly List<EnemyChip> chips = new List<EnemyChip>();

        public override void Init(MenuRouter router)
        {
            base.Init(router);
            chipTemplate.gameObject.SetActive(false);
            dimButton.onClick.AddListener(Close);
            closeButton.onClick.AddListener(Close);
            startButton.onClick.AddListener(() => Router.StartFloor(stage, picked));
            scrollButton.onClick.AddListener(() => Router.OpenScroll(stage));

            for (int i = 0; i < difficultyButtons.Length; i++)
            {
                Difficulty difficulty = (Difficulty)i;
                difficultyButtons[i].onClick.AddListener(() => Pick(difficulty));
                ColorBlock colors = difficultyButtons[i].colors;
                colors.disabledColor = Color.white; //The locked look is painted by hand
                difficultyButtons[i].colors = colors;
            }
        }

        public void Show(StageData stage)
        {
            this.stage = stage;
            kicker.text = Router.IsBoss(stage) ? "BOSS FLOOR" : "FLOOR " + stage.floor;
            title.text = stage.title;
            details.text = Lessons(stage) + "  ·  " + stage.waves.Count + (stage.waves.Count == 1 ? " wave" : " waves");

            foreach (EnemyChip chip in chips) Destroy(chip.gameObject);
            chips.Clear();
            foreach (string id in stage.waves)
            {
                EnemyData enemy = Router.Database.GetEnemy(id);
                if (enemy == null) continue;
                EnemyChip chip = Instantiate(chipTemplate, enemyRow);
                chip.gameObject.SetActive(true);
                chip.Show(enemy);
                chips.Add(chip);
            }

            Select(Router.Progress.IsOpen(stage, lastPicked, Router.Floors()) ? lastPicked : Difficulty.Normal);
            Open();
        }

        //A tap on a pill, the only thing that changes what sticks for the next floor
        private void Pick(Difficulty difficulty)
        {
            lastPicked = difficulty;
            Select(difficulty);
        }

        private void Select(Difficulty difficulty)
        {
            picked = difficulty;
            List<StageData> floors = Router.Floors();

            for (int i = 0; i < difficultyButtons.Length; i++)
            {
                Difficulty each = (Difficulty)i;
                Button button = difficultyButtons[i];
                bool open = Router.Progress.IsOpen(stage, each, floors);
                button.interactable = open;
                if (open)
                {
                    CodexScreen.Paint(button, each == picked);
                    continue;
                }
                button.GetComponent<Image>().color = InkColors.LacquerLow;
                Color faded = InkColors.MoonFaint;
                faded.a = 0.5f;
                button.GetComponentInChildren<TMP_Text>().color = faded;
            }

            DifficultyRules level = config.rules.For(picked);
            string note = level.choices + " choices per blank";
            if (level.extraSeconds > 0f) note += "  ·  +" + level.extraSeconds + " s on the clock";
            if (!Router.Progress.IsCleared(stage.id)) note += "  ·  clear it once to open Hard";
            difficultyNote.text = note;

            StageRecord record = Router.Progress.Record(stage.id, picked);
            if (record == null || record.clears == 0)
                best.text = picked == Difficulty.Normal ? "Not cleared yet" : "Not cleared on " + DifficultyText.Name(picked) + " yet";
            else best.text = StarText.Line(record.stars) + "  best  ·  cleared " + record.clears + (record.clears == 1 ? " time" : " times");
        }

        //"Lesson 3" or "Lessons 1–6"
        private static string Lessons(StageData stage)
        {
            if (stage.lessons.Count == 0) return "Mixed";
            if (stage.lessons.Count == 1) return "Lesson " + stage.lessons[0];
            return "Lessons " + stage.lessons[0] + "–" + stage.lessons[stage.lessons.Count - 1];
        }
    }
}
