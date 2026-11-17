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
        public event Action<HintKind> HintUsed;

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
        public int HintCharges { get; private set; }
        public StageStats Stats { get; } = new StageStats();

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
            HintCharges = rules.hintCharges;
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
            if (Round.removedOptions.Contains(optionIndex)) return; //50/50 took that one away
            Resolve(optionIndex);
        }

        //Each hint once per question, only while the question is up, and only where it makes sense
        public bool CanUseHint(HintKind kind)
        {
            if (Phase != BattlePhase.Asking || HintCharges <= 0 || Round.UsedHint(kind)) return false;
            if (!rules.HintWorks(kind, Round.question.format, Round.isParry)) return false;
            if (kind == HintKind.FiftyFifty && Round.options.Count < 4) return false; //Needs two wrong ones to take away
            return true;
        }

        public bool UseHint(HintKind kind)
        {
            if (!CanUseHint(kind)) return false;

            HintCharges--;
            Round.hintsUsed.Add(kind);
            Stats.hintsUsed++;

            if (kind == HintKind.FiftyFifty) RemoveTwoWrong();
            if (kind == HintKind.ExtraTime)
            {
                //The limit grows too, so the extra seconds can't pump up the speed bonus
                Round.timeLeft += rules.extraTimeSeconds;
                Round.timeLimit += rules.extraTimeSeconds;
            }

            HintUsed?.Invoke(kind); //Translate and first kana are just shown by the screen
            return true;
        }

        private void RemoveTwoWrong()
        {
            var wrong = new List<int>();
            for (int i = 0; i < Round.options.Count; i++)
                if (i != Round.correctIndex) wrong.Add(i);
            ListShuffle.Shuffle(wrong, random);
            Round.removedOptions.Add(wrong[0]);
            Round.removedOptions.Add(wrong[1]);
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

            roundCount++;
            Round = ChoiceBuilder.Build(question, random, rules.choiceCount, 0f, roundCount);
            Round.isParry = parry;
            Round.StartClock(rules.TimeLimit(question, Round.options, parry)); //Longer questions and replies get more time
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

            if (result.correct) RefillHint(result);
            Count(result);

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
                result.damage = Hit(Round.question.format);
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
                DamageBreakdown hit = Hit(QuestionFormat.Strike);
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

        //Hints that cost damage take their quarter, and any hint at all means no Perfect
        private DamageBreakdown Hit(QuestionFormat format)
        {
            DamageBreakdown hit = DamageCalculator.Calculate(rules, format, Round.timeLeft, Round.timeLimit, Streak, Round.DamageHints);
            if (Round.hintsUsed.Count > 0) hit.perfect = false;
            return hit;
        }

        private void RefillHint(RoundResult result)
        {
            if (rules.hintRefillEveryCombo <= 0 || Streak % rules.hintRefillEveryCombo != 0) return;
            if (HintCharges >= rules.hintMaxCharges) return;
            HintCharges++;
            result.hintRefilled = true;
        }

        private void Count(RoundResult result)
        {
            Stats.answered++;
            if (result.correct) Stats.correct++;
            if (result.damage.perfect) Stats.perfects++;
            Stats.bestStreak = Math.Max(Stats.bestStreak, Streak);
            if (Round.isParry)
            {
                if (result.blocked) Stats.parriesBlocked++;
                else Stats.parriesFailed++;
            }
        }
    }
}
