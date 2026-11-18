using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //The sheet that slides over the menu when you pick a floor: who you'll fight, then Start battle
    public class FloorPreviewSheet : MenuPanel
    {
        [SerializeField] private Button dimButton; //Tapping outside the sheet closes it too
        [SerializeField] private Button closeButton;
        [SerializeField] private Button startButton;
        [SerializeField] private TMP_Text kicker;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text details;
        [SerializeField] private EnemyChip chipTemplate; //Kept switched off, one copy per wave
        [SerializeField] private RectTransform enemyRow;
        [SerializeField] private TMP_Text best;

        private StageData stage;
        private readonly List<EnemyChip> chips = new List<EnemyChip>();

        public override void Init(MenuRouter router)
        {
            base.Init(router);
            chipTemplate.gameObject.SetActive(false);
            dimButton.onClick.AddListener(Close);
            closeButton.onClick.AddListener(Close);
            startButton.onClick.AddListener(() =>
            {
                if (stage != null) GameFlow.PlayStage(stage.id);
            });
        }

        public void Show(StageData stage)
        {
            this.stage = stage;
            kicker.text = Router.IsBoss(stage) ? "BOSS FLOOR" : "FLOOR " + stage.floor;
            title.text = stage.title;
            details.text = Lessons(stage) + "  ·  " + stage.waves.Count + (stage.waves.Count == 1 ? " wave" : " waves");
            best.text = "Not cleared yet"; //Best stars come with progress saving

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

            Open();
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
