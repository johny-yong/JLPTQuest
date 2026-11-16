using System;
using System.Collections.Generic;

namespace JLPTQuest
{
    public enum BattlePhase { NotStarted, WaveIntro, Asking, Resolved, WaveCleared, StageCleared }

    /*
        A whole stage as plain C#, no Unity in here so it's easy to test.
        Each wave: WaveIntro -> Asking -> Resolved -> Asking ... -> WaveCleared -> the next wave's WaveIntro
        Beating the last enemy ends in StageCleared instead of WaveCleared.
        The screen decides how long each step stays up, then calls NextRound or NextWave.
        The streak and the question deck carry over between waves, each enemy brings its own HP and CD.
    */
    public class BattleSession
    {
        public event Action<int> WaveStarted; //Wave index, 0 is the first
        public event Action<QuestionRound> RoundStarted;
        public event Action<RoundResult> RoundResolved;
        public event Action EnemyAttacked; //Placeholder until parry turns land
        public event Action<EnemyData> EnemyDefeated;
        public event Action StageCleared;

        public BattlePhase Phase { get; private set; } = BattlePhase.NotStarted;
        public IReadOnlyList<EnemyData> Waves => waves;
        public int WaveIndex { get; private set; } = -1;
        public int WaveCount => waves.Count;
        public bool IsLastWave => WaveIndex == waves.Count - 1;
        public EnemyData CurrentEnemy => WaveIndex >= 0 ? waves[WaveIndex] : null;
        public EnemyState Enemy { get; private set; }
        public QuestionRound Round { get; private set; }
        public int Streak { get; private set; }

        private readonly List<EnemyData> waves;
        private readonly BattleRules rules;
        private readonly QuestionDeck deck;
        private readonly Random random;
        private int roundCount;

        public BattleSession(IList<EnemyData> waves, IList<QuestionData> questions, BattleRules rules, int seed)
        {
            if (waves == null || waves.Count == 0) throw new ArgumentException("A stage needs at least one wave.");
            this.waves = new List<EnemyData>(waves);
            this.rules = rules;
            random = new Random(seed);
            deck = new QuestionDeck(questions, random);
        }

        public void Start()
        {
            if (Phase != BattlePhase.NotStarted) return;
            EnterWave(0);
        }

        //After a wave intro or a resolved round
        public void NextRound()
        {
            if (Phase != BattlePhase.WaveIntro && Phase != BattlePhase.Resolved) return;
            BeginRound();
        }

        public void NextWave()
        {
            if (Phase != BattlePhase.WaveCleared) return;
            EnterWave(WaveIndex + 1);
        }

        public void Tick(float deltaTime)
        {
            if (Phase != BattlePhase.Asking) return;

            Round.timeLeft -= deltaTime;
            if (Round.timeLeft > 0f) return;

            Round.timeLeft = 0f;
            Resolve(-1);
        }

        public void Answer(int optionIndex)
        {
            if (Phase != BattlePhase.Asking) return;
            if (optionIndex < 0 || optionIndex >= Round.options.Count) return;
            Resolve(optionIndex);
        }

        private void EnterWave(int index)
        {
            WaveIndex = index;
            EnemyData data = waves[index];
            Enemy = new EnemyState(DamageCalculator.EnemyHp(rules, data.hits), data.attackCd);
            Phase = BattlePhase.WaveIntro;
            WaveStarted?.Invoke(index);
        }

        private void BeginRound()
        {
            QuestionData question = deck.Draw();
            roundCount++;
            Round = ChoiceBuilder.Build(question, random, rules.choiceCount, rules.TimeLimit(question), roundCount);
            Phase = BattlePhase.Asking;
            RoundStarted?.Invoke(Round);
        }

        private void Resolve(int chosenIndex)
        {
            bool correct = chosenIndex == Round.correctIndex;
            var result = new RoundResult
            {
                round = Round,
                chosenIndex = chosenIndex,
                correct = correct,
                timedOut = chosenIndex < 0
            };

            if (correct)
            {
                Streak++;
                result.damage = DamageCalculator.Calculate(rules, Round.question.format, Round.timeLeft, Round.timeLimit, Streak, 0);
                Enemy.TakeDamage(result.damage.total);
                Enemy.TickCooldown(rules.cooldownTickOnCorrect);
            }
            else
            {
                Streak = 0;
                Enemy.TickCooldown(rules.cooldownTickOnWrong); //A miss brings the enemy's attack closer
            }

            result.streak = Streak;
            result.enemyDefeated = Enemy.IsDefeated;
            result.stageCleared = result.enemyDefeated && IsLastWave;
            result.enemyAttacked = !Enemy.IsDefeated && Enemy.CooldownLeft <= 0;
            if (result.enemyAttacked) Enemy.ResetCooldown();

            if (result.stageCleared) Phase = BattlePhase.StageCleared;
            else if (result.enemyDefeated) Phase = BattlePhase.WaveCleared;
            else Phase = BattlePhase.Resolved;

            RoundResolved?.Invoke(result);
            if (result.enemyAttacked) EnemyAttacked?.Invoke();
            if (result.enemyDefeated) EnemyDefeated?.Invoke(CurrentEnemy);
            if (result.stageCleared) StageCleared?.Invoke();
        }
    }
}
