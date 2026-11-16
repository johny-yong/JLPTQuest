using System;
using System.Collections.Generic;

namespace JLPTQuest
{
    public enum BattlePhase { NotStarted, Asking, Resolved, Victory }

    /*
        The whole fight as plain C#, no Unity in here so it's easy to test.
        Flow: Start -> Asking -> (answer or time up) -> Resolved -> NextRound -> Asking ... -> Victory
        The screen decides how long to linger in Resolved, then calls NextRound.
    */
    public class BattleSession
    {
        public event Action<QuestionRound> RoundStarted;
        public event Action<RoundResult> RoundResolved;
        public event Action EnemyAttacked; //Placeholder until parry turns land in Phase 3
        public event Action EnemyDefeated;

        public BattlePhase Phase { get; private set; } = BattlePhase.NotStarted;
        public EnemyState Enemy { get; }
        public QuestionRound Round { get; private set; }
        public int Streak { get; private set; }

        private readonly BattleRules rules;
        private readonly QuestionDeck deck;
        private readonly Random random;
        private int roundCount;

        public BattleSession(EnemyData enemy, IList<QuestionData> questions, BattleRules rules, int seed)
        {
            this.rules = rules;
            random = new Random(seed);
            deck = new QuestionDeck(questions, random);
            Enemy = new EnemyState(DamageCalculator.EnemyHp(rules, enemy.hits), enemy.attackCd);
        }

        public void Start()
        {
            if (Phase != BattlePhase.NotStarted) return;
            BeginRound();
        }

        public void NextRound()
        {
            if (Phase != BattlePhase.Resolved) return;
            BeginRound();
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
            result.enemyAttacked = !Enemy.IsDefeated && Enemy.CooldownLeft <= 0;
            if (result.enemyAttacked) Enemy.ResetCooldown();

            Phase = result.enemyDefeated ? BattlePhase.Victory : BattlePhase.Resolved;
            RoundResolved?.Invoke(result);
            if (result.enemyAttacked) EnemyAttacked?.Invoke();
            if (result.enemyDefeated) EnemyDefeated?.Invoke();
        }
    }
}
