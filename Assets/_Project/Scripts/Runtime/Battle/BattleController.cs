using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace JLPTQuest
{
    /*
        Glue between the BattleSession rules and the views.
        Runs one stage from the Stages tab: its waves in order, strike questions from its lessons,
        and parry questions from the same lessons whenever an enemy's countdown hits 0.
        Ends on the results screen with stars, which can go straight on to the next floor.
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
        [SerializeField] private CompanionView companion;
        [SerializeField] private CutInView cutIn;
        [SerializeField] private PlayerHud playerHud;
        [SerializeField] private HintBarView hintBar;
        [SerializeField] private ComboView combo;
        [SerializeField] private QuestionCardView questionCard;
        [SerializeField] private AnswerTilesView answerTiles;
        [SerializeField] private TeachCardView teachCard;
        [SerializeField] private VictoryView victory;
        [SerializeField] private DefeatView defeat;
        [SerializeField] private PauseView pauseView;
        [SerializeField] private float worriedSeconds = 3f; //Companion looks worried on the last seconds of the clock
        [SerializeField, Range(0f, 1f)] private float worriedHp = 0.3f; //...or below this much HP

        private BattleSession session;
        private StagePlan plan;
        private bool paused;

        void Start()
        {
            //Came from the menu? Play the floor it picked, otherwise whatever's set in the inspector
            string picked = GameFlow.TakeStage();
            if (!string.IsNullOrEmpty(picked)) stageId = picked;

            answerTiles.Picked += OnPicked;
            hintBar.Picked += OnHintPicked;
            pauseView.PauseRequested += Pause;
            pauseView.ResumeRequested += Resume;
            pauseView.QuitRequested += BackToTower;
            StartStage();
        }

        void OnDestroy()
        {
            answerTiles.Picked -= OnPicked;
            hintBar.Picked -= OnHintPicked;
            pauseView.PauseRequested -= Pause;
            pauseView.ResumeRequested -= Resume;
            pauseView.QuitRequested -= BackToTower;
            Unhook();
            Time.timeScale = 1f; //Never leave the next scene frozen
        }

        //Phone went to the background mid question, don't let the clock run out behind the player's back
        void OnApplicationPause(bool away)
        {
            if (away) Pause();
        }

        void Update()
        {
            if (session == null) return;
            session.Tick(Time.deltaTime);

            bool timeRunningOut = session.Phase == BattlePhase.Asking && session.Round.timeLeft <= worriedSeconds;
            companion.SetWorried(timeRunningOut || session.Player.HpFraction <= worriedHp);
        }

        public void StartStage()
        {
            StopAllCoroutines();
            Unhook();
            paused = false;
            Time.timeScale = 1f;
            pauseView.Hide();

            plan = StagePlan.Build(database, stageId, out string problem);
            if (config == null || plan == null)
            {
                Debug.LogError("[Battle] Can't start: " + (config == null ? "there's no BattleConfig on the BattleController." : problem + " Sync the content first."));
                enabled = false;
                return;
            }

            session = new BattleSession(plan.waves, plan.questions, config.rules, Environment.TickCount, plan.parryQuestions);
            session.WaveStarted += OnWaveStarted;
            session.RoundStarted += OnRoundStarted;
            session.RoundResolved += OnRoundResolved;

            topLabel.text = "Floor " + plan.stage.floor + "  ·  " + plan.stage.title;
            teachCard.Hide();
            victory.Hide();
            defeat.Hide();
            waveBanner.Hide();
            cutIn.Hide();
            companion.ResetPose();
            combo.Hide();
            playerHud.Refresh(session.Player);
            hintBar.Refresh(session);
            session.Start();
        }

        private void OnPicked(int index)
        {
            session?.Answer(index);
        }

        private void OnHintPicked(HintKind kind)
        {
            if (session == null || !session.UseHint(kind)) return;

            if (kind == HintKind.Translate) questionCard.ShowTranslation();
            if (kind == HintKind.FiftyFifty) answerTiles.Remove(session.Round.removedOptions);
            //+5 seconds shows on the timer bar by itself, first kana waits for Trace and Build
            hintBar.Refresh(session);
        }

        private void OnWaveStarted(int index)
        {
            int count = session.WaveCount;
            waveLabel.text = "Wave " + (index + 1) + "/" + count;
            questionCard.Clear();
            answerTiles.Clear();
            enemyView.Show(session.CurrentEnemy, session.Enemy);
            hintBar.Refresh(session);

            string title = count > 1 && session.IsLastWave ? "Final wave" : "Wave " + (index + 1) + "/" + count;
            waveBanner.Play(title, EnemyView.NameLine(session.CurrentEnemy), config.waveIntroSeconds);
            StartCoroutine(AfterDelay(config.waveIntroSeconds, session.NextRound));
        }

        private void OnRoundStarted(QuestionRound round)
        {
            questionCard.Show(round);
            answerTiles.Show(round.options, round.isParry);
            hintBar.Refresh(session);
        }

        private void OnRoundResolved(RoundResult result)
        {
            answerTiles.ShowResult(result);
            questionCard.Reveal(result.round.CorrectOption);
            enemyView.Refresh(session.Enemy);
            playerHud.Refresh(session.Player);
            combo.Show(result.streak);
            hintBar.Refresh(session);
            if (result.hintRefilled) hintBar.ShowRefill();

            float wait = config.revealSeconds;
            if (TryCutIn(result)) wait = Mathf.Max(wait, config.cutInSeconds);

            if (result.round.isParry)
            {
                if (result.blocked)
                {
                    companion.Block();
                    enemyView.ShowHit(result.damage, true);
                }
                else
                {
                    companion.Hurt(result.playerDamage);
                    playerHud.ShowHit();
                }
            }
            else if (result.correct)
            {
                companion.Attack();
                enemyView.ShowHit(result.damage);
            }

            if (result.enemyDefeated)
                StartCoroutine(DefeatThen(wait, result.stageCleared ? (Action)ShowStageCleared : session.NextWave));
            else if (result.correct)
                StartCoroutine(AfterDelay(wait, Continue));
            else
                teachCard.Show(result, config.teachMinSeconds, config.teachAutoSeconds, result.playerDefeated ? (Action)ShowDefeat : Continue); //Even the last miss gets taught
        }

        //Counters always get the cut-in, Perfects only on every few combos so it stays special
        private bool TryCutIn(RoundResult result)
        {
            string title = null;
            if (result.blocked && config.cutInOnCounter) title = "Counter!";
            else if (result.correct && result.damage.perfect && config.cutInEveryCombo > 0 && result.streak % config.cutInEveryCombo == 0) title = "Perfect!";
            if (title == null) return false;

            //The band covers the floating damage number, so the damage rides along in the subtitle
            cutIn.Play(companion.Art, companion.Glyph, title, result.damage.total + " damage  ·  " + result.streak + " combo", config.cutInSeconds);
            return true;
        }

        //Next turn, with a short warning first when it's the enemy's attack
        private void Continue()
        {
            if (session.ParryPending) StartCoroutine(ParryIntroThenAsk());
            else session.NextRound();
        }

        private IEnumerator ParryIntroThenAsk()
        {
            questionCard.Clear();
            answerTiles.Clear();
            waveBanner.Play("Enemy attack!", "Pick the reply that blocks it", config.parryIntroSeconds);
            yield return new WaitForSeconds(config.parryIntroSeconds);
            session.NextRound();
        }

        private void ShowStageCleared()
        {
            bool[] stars = StageStars.Earned(true, session.Stats, config.rules);
            string next = StagePlan.NextStageId(database, plan.stage);
            Action onNext = null;
            if (next != null) onNext = () => PlayStage(next);
            victory.Show(plan.stage.title + " cleared!", stars, StatsLine(), WordSeals(), StartStage, onNext, BackToTower);
        }

        private void ShowDefeat()
        {
            string detail = "Wave " + (session.WaveIndex + 1) + "/" + session.WaveCount + "  ·  " + session.CurrentEnemy.displayName + " won this time\n" + StatsLine();
            defeat.Show("Out of HP", detail, StartStage, BackToTower);
        }

        //Time scale 0 stops the question clock and every WaitForSeconds in here and the views in one go
        private void Pause()
        {
            if (session == null || paused || session.Phase == BattlePhase.StageCleared || session.Phase == BattlePhase.Defeated) return;
            paused = true;
            Time.timeScale = 0f;
            pauseView.Show();
        }

        private void Resume()
        {
            if (!paused) return;
            paused = false;
            Time.timeScale = 1f;
            pauseView.Hide();
        }

        private void BackToTower()
        {
            Time.timeScale = 1f;
            GameFlow.OpenMenu(MenuScreen.Tower);
        }

        public void PlayStage(string id)
        {
            stageId = id;
            StartStage();
        }

        //"11/13 right  ·  85%  ·  best combo 7"
        private string StatsLine()
        {
            StageStats stats = session.Stats;
            string line = stats.correct + "/" + stats.answered + " right  ·  " + Mathf.RoundToInt(stats.Accuracy * 100f) + "%  ·  best combo " + stats.bestStreak;
            if (stats.hintsUsed > 0) line += "  ·  " + stats.hintsUsed + (stats.hintsUsed == 1 ? " hint" : " hints");
            return line;
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
        private IEnumerator DefeatThen(float wait, Action then)
        {
            yield return new WaitForSeconds(wait);
            enemyView.ShowDefeated(config.defeatSeconds);
            companion.Cheer();
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
            session = null;
        }
    }
}
