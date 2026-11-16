using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JLPTQuest
{
    /*
        Glue between the BattleSession rules and the views.
        Runs one stage from the Stages tab: its waves in order, strike questions from its lessons.
        Parry, hints and results come in the next steps of Phase 3.
    */
    public class BattleController : MonoBehaviour
    {
        [SerializeField] private ContentDatabase database;
        [SerializeField] private BattleConfig config;
        [SerializeField] private string stageId = "R1-F1";

        [SerializeField] private TMP_Text topLabel;
        [SerializeField] private TMP_Text waveLabel;
        [SerializeField] private EnemyView enemyView;
        [SerializeField] private WaveBanner waveBanner;
        [SerializeField] private QuestionCardView questionCard;
        [SerializeField] private AnswerTilesView answerTiles;
        [SerializeField] private TeachCardView teachCard;
        [SerializeField] private VictoryView victory;

        private BattleSession session;
        private StagePlan plan;

        void Start()
        {
            answerTiles.Picked += OnPicked;
            StartStage();
        }

        void OnDestroy()
        {
            answerTiles.Picked -= OnPicked;
            Unhook();
        }

        void Update()
        {
            session?.Tick(Time.deltaTime);
        }

        public void StartStage()
        {
            StopAllCoroutines();
            Unhook();

            plan = StagePlan.Build(database, stageId, out string problem);
            if (config == null || plan == null)
            {
                Debug.LogError("[Battle] Can't start: " + (config == null ? "there's no BattleConfig, rebuild the greybox." : problem + " Sync the content first."));
                enabled = false;
                return;
            }

            session = new BattleSession(plan.waves, plan.questions, config.rules, Environment.TickCount);
            session.WaveStarted += OnWaveStarted;
            session.RoundStarted += OnRoundStarted;
            session.RoundResolved += OnRoundResolved;
            session.EnemyAttacked += OnEnemyAttacked;

            topLabel.text = "Floor " + plan.stage.floor + "  ·  " + plan.stage.title;
            teachCard.Hide();
            victory.Hide();
            waveBanner.Hide();
            session.Start();
        }

        private void OnPicked(int index)
        {
            session?.Answer(index);
        }

        private void OnWaveStarted(int index)
        {
            int count = session.WaveCount;
            waveLabel.text = "Wave " + (index + 1) + "/" + count;
            questionCard.Clear();
            answerTiles.Clear();
            enemyView.Show(session.CurrentEnemy, session.Enemy);

            string title = count > 1 && session.IsLastWave ? "Final wave" : "Wave " + (index + 1) + "/" + count;
            waveBanner.Play(title, EnemyView.NameLine(session.CurrentEnemy), config.waveIntroSeconds);
            StartCoroutine(AfterDelay(config.waveIntroSeconds, session.NextRound));
        }

        private void OnRoundStarted(QuestionRound round)
        {
            questionCard.Show(round);
            answerTiles.Show(round.options);
        }

        private void OnRoundResolved(RoundResult result)
        {
            answerTiles.ShowResult(result);
            questionCard.Reveal(result.round.CorrectOption);
            enemyView.Refresh(session.Enemy);
            if (result.correct) enemyView.ShowHit(result.damage);

            if (result.enemyDefeated)
                StartCoroutine(DefeatThen(result.stageCleared ? (Action)ShowStageCleared : session.NextWave));
            else if (result.correct)
                StartCoroutine(AfterDelay(config.revealSeconds, session.NextRound));
            else
                teachCard.Show(result, config.teachMinSeconds, config.teachAutoSeconds, session.NextRound);
        }

        private void OnEnemyAttacked()
        {
            enemyView.ShowAttack();
        }

        private void ShowStageCleared()
        {
            victory.Show(plan.stage.title + " cleared!", WordSeals(), StartStage);
        }

        //One of each, in the order the enemies were beaten
        private List<string> WordSeals()
        {
            var seals = new List<string>();
            foreach (EnemyData enemy in plan.waves)
                if (!string.IsNullOrEmpty(enemy.wordSeal) && !seals.Contains(enemy.wordSeal)) seals.Add(enemy.wordSeal);
            return seals;
        }

        //Let the last hit land, fade the enemy out, then move on
        private IEnumerator DefeatThen(Action then)
        {
            yield return new WaitForSeconds(config.revealSeconds);
            enemyView.ShowDefeated(config.defeatSeconds);
            yield return new WaitForSeconds(config.defeatSeconds);
            then();
        }

        private IEnumerator AfterDelay(float seconds, Action then)
        {
            yield return new WaitForSeconds(seconds);
            then();
        }

        private void Unhook()
        {
            if (session == null) return;
            session.WaveStarted -= OnWaveStarted;
            session.RoundStarted -= OnRoundStarted;
            session.RoundResolved -= OnRoundResolved;
            session.EnemyAttacked -= OnEnemyAttacked;
            session = null;
        }
    }
}
