using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        Three tabs. Word Seals: every seal in the realm with furigana, the ones not found yet stay as ？.
        Grammar: every grammar point by lesson, met ones open their floor's scroll so you can reread before a rematch.
        Kana: the hiragana and katakana charts with romaji, always fully open since it's a reference.
    */
    public class CodexScreen : MenuPanel
    {
        private enum CodexTab { Seals, Grammar, Kana }

        [SerializeField] private TMP_Text countLabel;
        [SerializeField] private Button sealsTab;
        [SerializeField] private Button grammarTab;
        [SerializeField] private Button kanaTab;
        [SerializeField] private GameObject sealsView;
        [SerializeField] private GameObject grammarView;
        [SerializeField] private KanaView kanaView;

        [SerializeField] private RectTransform grid;
        [SerializeField] private GameObject sealTemplate; //Kept switched off, needs a RubyText somewhere inside

        [SerializeField] private RectTransform grammarList;
        [SerializeField] private TMP_Text lessonTemplate; //Kept switched off
        [SerializeField] private GrammarRow rowTemplate; //Kept switched off

        private readonly List<GameObject> built = new List<GameObject>();
        private CodexTab tab;

        public override void Init(MenuRouter router)
        {
            base.Init(router);
            sealTemplate.SetActive(false);
            lessonTemplate.gameObject.SetActive(false);
            rowTemplate.gameObject.SetActive(false);
            kanaView.Init(summary => countLabel.text = summary);
            sealsTab.onClick.AddListener(() => ShowTab(CodexTab.Seals));
            grammarTab.onClick.AddListener(() => ShowTab(CodexTab.Grammar));
            kanaTab.onClick.AddListener(() => ShowTab(CodexTab.Kana));
        }

        public override void Open()
        {
            base.Open();
            ShowTab(tab);
        }

        private void ShowTab(CodexTab next)
        {
            tab = next;
            Paint(sealsTab, tab == CodexTab.Seals);
            Paint(grammarTab, tab == CodexTab.Grammar);
            Paint(kanaTab, tab == CodexTab.Kana);
            sealsView.SetActive(tab == CodexTab.Seals);
            grammarView.SetActive(tab == CodexTab.Grammar);
            kanaView.gameObject.SetActive(tab == CodexTab.Kana);

            foreach (GameObject item in built) Destroy(item);
            built.Clear();
            if (tab == CodexTab.Grammar) BuildGrammar();
            else if (tab == CodexTab.Kana) kanaView.Show();
            else BuildSeals();
        }

        private void BuildSeals()
        {
            Progress progress = Router.Progress;
            List<string> seals = Router.Seals();
            int found = 0;
            foreach (string seal in seals)
            {
                bool have = progress.HasSeal(seal);
                if (have) found++;

                GameObject tile = Instantiate(sealTemplate, grid);
                tile.SetActive(true);
                tile.GetComponent<Image>().color = have ? InkColors.Card : InkColors.Panel;
                RubyText word = tile.GetComponentInChildren<RubyText>();
                word.Markup = have ? seal : "？";
                word.GetComponent<TMP_Text>().color = have ? InkColors.Ink : InkColors.InkFaint;
                built.Add(tile);
            }
            countLabel.text = found + " / " + seals.Count + " Word Seals found";
        }

        private void BuildGrammar()
        {
            Progress progress = Router.Progress;
            var lessons = new List<int>();
            foreach (StageData floor in Router.Floors())
                foreach (int lesson in floor.lessons)
                    if (!lessons.Contains(lesson)) lessons.Add(lesson);
            lessons.Sort();

            int met = 0;
            int total = 0;
            foreach (int lesson in lessons)
            {
                List<GrammarData> points = Router.Database.GrammarFor(lesson);
                if (points.Count == 0) continue;

                TMP_Text heading = Instantiate(lessonTemplate, grammarList);
                heading.gameObject.SetActive(true);
                heading.text = "LESSON " + lesson;
                built.Add(heading.gameObject);

                foreach (GrammarData point in points)
                {
                    bool seen = progress.HasSeenGrammar(point.id);
                    if (seen) met++;
                    total++;

                    GrammarRow row = Instantiate(rowTemplate, grammarList);
                    row.gameObject.SetActive(true);
                    row.Show(point, seen, () => Router.OpenScrollAt(point));
                    built.Add(row.gameObject);
                }
            }
            countLabel.text = met + " / " + total + " grammar points met";
        }

        //Also used by the Kana tab's script switch so the pills look the same
        internal static void Paint(Button tab, bool on)
        {
            tab.GetComponent<Image>().color = on ? InkColors.Ink : InkColors.Panel;
            tab.GetComponentInChildren<TMP_Text>().color = on ? InkColors.Paper : InkColors.InkSoft;
        }
    }
}
