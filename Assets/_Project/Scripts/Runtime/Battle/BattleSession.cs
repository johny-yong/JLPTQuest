using System;
using System.Collections.Generic;

namespace JLPTQuest
{
    public enum BattlePhase { NotStarted, WaveIntro, Asking, Resolved, WaveCleared, StageCleared, Defeated }

    /*
        A whole stage as plain C#, no Unity in here so it's easy to test.
        Each wave: WaveIntro -> Asking -> Resolved -> Asking ... -> WaveCleared -> the next wave's WaveIntro
        Beating the last enemy ends in StageCleared, running out of HP ends in Defeated.
        The screen decides how long each step stays up, then calls NextRound or NextWave.

        Offense turn: a right answer hits and ticks the enemy's countdown by 1, a wrong one by 2.
        When the countdown hits 0 the next turn is a parry: the enemy asks something and you pick the reply.
        Right blocks it and counters for half a strike, wrong costs HP equal to the enemy's attack.
        The streak and the question decks carry over between waves, each enemy brings its own HP and CD.
    */
    public class BattleSession
    {
        public event Action<int> WaveStarted; //Wave index, 0 is the first
        public event Action<QuestionRound> RoundStarted;
        public event Action<RoundResult> RoundResolved;
        public event Action ParryIncoming; //The countdown hit 0, the next turn is a parry
        public event Action<EnemyData> EnemyDefeated;
        public event Action StageCleared;
        public event Action PlayerDefeated;

        public BattlePhase Phase { get; private set; } = BattlePhase.NotStarted;
        public IReadOnlyList<EnemyData> Waves => waves;
        public int WaveIndex { get; private set; } = -1;
        public int WaveCount => waves.Count;
        public bool IsLastWave => WaveIndex == waves.Count - 1;
        public EnemyData CurrentEnemy => WaveIndex >= 0 ? waves[WaveIndex] : null;
        public EnemyState Enemy { get; private set; }
        public PlayerState Player { get; }
        public QuestionRound Round { get; private set; }
        public int Streak { get; private set; }
        public bool ParryPending { get; private set; }

        private readonly List<EnemyData> waves;
        private readonly BattleRules rules;
        private readonly QuestionDeck deck;
        private readonly QuestionDeck parryDeck; //Null when the stage has no parry questions, the offense deck stands in
        private readonly Random random;
        private int roundCount;

        public BattleSession(IList<EnemyData> waves, IList<QuestionData> questions, BattleRules rules, int seed, IList<QuestionData> parryQuestions = null)
        {
            if (waves == null || waves.Count == 0) throw new ArgumentException("A stage needs at least one wave.");
            this.waves = new List<EnemyData>(waves);
            this.rules = rules;
            random = new Random(seed);
            deck = new QuestionDeck(questions, random);
            if (parryQuestions != null && parryQuestions.Count > 0) parryDeck = new QuestionDeck(parryQuestions, random);
            Player = new PlayerState(rules.playerMaxHp);
        }

        public void Start()
        {
            if (Phase != BattlePhase.NotStarted) return;
            EnterWave(0);
        }

        //After a wave intro or a resolved round. Gives a parry turn when one is pending
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
            ParryPending = false;
            Phase = BattlePhase.WaveIntro;
            WaveStarted?.Invoke(index);
        }

        private void BeginRound()
        {
            bool parry = ParryPending;
            QuestionData question = parry && parryDeck != null ? parryDeck.Draw() : deck.Draw();
            float timeLimit = parry ? rules.parrySeconds : rules.TimeLimit(question);

            roundCount++;
            Round = ChoiceBuilder.Build(question, random, rules.choiceCount, timeLimit, roundCount);
            Round.isParry = parry;
            Phase = BattlePhase.Asking;
            RoundStarted?.Invoke(Round);
        }

        private void Resolve(int chosenIndex)
        {
            var result = new RoundResult
            {
                round = Round,
                chosenIndex = chosenIndex,
                correct = chosenIndex == Round.correctIndex,
                timedOut = chosenIndex < 0
            };

            if (Round.isParry) ResolveParry(result);
            else ResolveOffense(result);

            result.streak = Streak;
            result.enemyDefeated = Enemy.IsDefeated;
            result.stageCleared = result.enemyDefeated && IsLastWave;
            result.playerDefeated = Player.IsDefeated;
            result.parryNext = !Round.isParry && !Enemy.IsDefeated && !Player.IsDefeated && Enemy.IsReadyToAttack;
            ParryPending = result.parryNext;

            if (result.playerDefeated) Phase = BattlePhase.Defeated;
            else if (result.stageCleared) Phase = BattlePhase.StageCleared;
            else if (result.enemyDefeated) Phase = BattlePhase.WaveCleared;
            else Phase = BattlePhase.Resolved;

            RoundResolved?.Invoke(result);
            if (result.parryNext) ParryIncoming?.Invoke();
            if (result.enemyDefeated) EnemyDefeated?.Invoke(CurrentEnemy);
            if (result.stageCleared) StageCleared?.Invoke();
            if (result.playerDefeated) PlayerDefeated?.Invoke();
        }

        private void ResolveOffense(RoundResult result)
        {
            if (result.correct)
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
        }

        private void ResolveParry(RoundResult result)
        {
            if (result.correct)
            {
                //Blocked, and the counter is half of what the same answer would do as a strike
                Streak++;
                DamageBreakdown hit = DamageCalculator.Calculate(rules, QuestionFormat.Strike, Round.timeLeft, Round.timeLimit, Streak, 0);
                hit.total = (int)Math.Round(hit.total * (double)rules.parryCounterShare, MidpointRounding.AwayFromZero);
                result.damage = hit;
                result.blocked = true;
                Enemy.TakeDamage(hit.total);
            }
            else
            {
                Streak = 0;
                result.playerDamage = CurrentEnemy.attack;
                Player.TakeDamage(result.playerDamage);
            }

            Enemy.ResetCooldown(); //Either way the attack is spent
        }
    }
}
