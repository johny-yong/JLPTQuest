using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JLPTQuest
{
    /*
        Glue between the BattleSession rules and the views.
        Phase 2 runs one enemy on the Strike questions of one lesson. Waves, parry and hints come in Phase 3.
    */
    public class BattleController : MonoBehaviour
    {
        [SerializeField] private ContentDatabase database;
        [SerializeField] private BattleConfig config;
        [SerializeField] private int lesson = 1;
        [SerializeField] private string enemyId = "kuchi-he";

        [SerializeField] private TMP_Text topLabel;
        [SerializeField] private EnemyView enemyView;
        [SerializeField] private QuestionCardView questionCard;
        [SerializeField] private AnswerTilesView answerTiles;
        [SerializeField] private TeachCardView teachCard;
        [SerializeField] private VictoryView victory;

        private BattleSession session;
        private EnemyData enemy;

        void Start()
        {
            answerTiles.Picked += OnPicked;
            StartBattle();
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

        public void StartBattle()
        {
            StopAllCoroutines();
            Unhook();

            enemy = database != null ? database.GetEnemy(enemyId) : null;
            List<QuestionData> pool = StrikePool();
            if (config == null || enemy == null || pool.Count == 0)
            {
                Debug.LogError("[Battle] Need a BattleConfig, the enemy '" + enemyId + "' and some strike questions in lesson " + lesson + ". Sync the content or rebuild the greybox.");
                enabled = false;
                return;
            }

            session = new BattleSession(enemy, pool, config.rules, Environment.TickCount);
            session.RoundStarted += OnRoundStarted;
            session.RoundResolved += OnRoundResolved;
            session.EnemyAttacked += OnEnemyAttacked;

            teachCard.Hide();
            victory.Hide();
            enemyView.Show(enemy, session.Enemy);
            session.Start();
        }

        private List<QuestionData> StrikePool()
        {
            var pool = new List<QuestionData>();
            QuestionSet set = database != null ? database.GetLesson(lesson) : null;
            if (set == null) return pool;

            foreach (QuestionData question in set.questions)
                if (question.format == QuestionFormat.Strike) pool.Add(question);
            return pool;
        }

        private void OnPicked(int index)
        {
            session?.Answer(index);
        }

        private void OnRoundStarted(QuestionRound round)
        {
            topLabel.text = "Lesson " + lesson + "  ·  Question " + round.number;
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
            {
                StartCoroutine(AfterDelay(config.revealSeconds, () => victory.Show(enemy, StartBattle)));
                return;
            }

            if (result.correct) StartCoroutine(AfterDelay(config.revealSeconds, session.NextRound));
            else teachCard.Show(result, config.teachMinSeconds, config.teachAutoSeconds, session.NextRound);
        }

        private void OnEnemyAttacked()
        {
            enemyView.ShowAttack();
        }

        private IEnumerator AfterDelay(float seconds, Action then)
        {
            yield return new WaitForSeconds(seconds);
            then();
        }

        private void Unhook()
        {
            if (session == null) return;
            session.RoundStarted -= OnRoundStarted;
            session.RoundResolved -= OnRoundResolved;
            session.EnemyAttacked -= OnEnemyAttacked;
            session = null;
        }
    }
}
