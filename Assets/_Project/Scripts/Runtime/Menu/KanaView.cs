using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    /*
        The Kana tab in the Codex. A reference chart of both scripts with romaji under every square,
        so it's the one place outside Realm 0 where romaji shows up. Tap a square and the bar at the
        bottom shows it in hiragana and katakana side by side.
    */
    public class KanaView : MonoBehaviour
    {
        [SerializeField] private Button hiraganaButton;
        [SerializeField] private Button katakanaButton;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RectTransform list;
        [SerializeField] private TMP_Text headingTemplate; //Kept switched off
        [SerializeField] private RectTransform rowTemplate; //Kept switched off, needs a LayoutElement for its height
        [SerializeField] private KanaCell cellTemplate; //Kept switched off
        [SerializeField] private TMP_Text noteTemplate; //Kept switched off

        [SerializeField] private TMP_Text detailPair;
        [SerializeField] private TMP_Text detailRomaji;
        [SerializeField] private TMP_Text detailCaption;

        [SerializeField] private float squareHeight = 168f; //Rows of 5
        [SerializeField] private float comboHeight = 150f; //Rows of 3, the squares get wide so they can be a bit shorter

        private readonly List<GameObject> built = new List<GameObject>();
        private Action<string> showSummary;
        private KanaScript script;
        private KanaCell picked;
        private string pickedKana; //Remembered so the same square stays lit when you switch scripts

        public void Init(Action<string> showSummary)
        {
            this.showSummary = showSummary;
            headingTemplate.gameObject.SetActive(false);
            rowTemplate.gameObject.SetActive(false);
            cellTemplate.gameObject.SetActive(false);
            noteTemplate.gameObject.SetActive(false);
            hiraganaButton.onClick.AddListener(() => Pick(KanaScript.Hiragana));
            katakanaButton.onClick.AddListener(() => Pick(KanaScript.Katakana));
            ClearDetail();
        }

        public void Show()
        {
            Pick(script);
        }

        private void Pick(KanaScript next)
        {
            script = next;
            CodexScreen.Paint(hiraganaButton, script == KanaScript.Hiragana);
            CodexScreen.Paint(katakanaButton, script == KanaScript.Katakana);
            Build();
            scroll.verticalNormalizedPosition = 1f;

            string name = script == KanaScript.Katakana ? "katakana" : "hiragana";
            showSummary?.Invoke(KanaChart.Count(script) + " " + name + ", tap one to compare");
        }

        private void Build()
        {
            foreach (GameObject item in built) Destroy(item);
            built.Clear();
            picked = null;

            foreach (KanaSection section in KanaChart.For(script))
            {
                TMP_Text heading = Instantiate(headingTemplate, list);
                heading.gameObject.SetActive(true);
                heading.text = section.Title(script);
                built.Add(heading.gameObject);

                float height = section.columns >= 5 ? squareHeight : comboHeight;
                for (int r = 0; r < section.Rows; r++)
                {
                    RectTransform row = Instantiate(rowTemplate, list);
                    row.gameObject.SetActive(true);
                    row.GetComponent<LayoutElement>().preferredHeight = height;
                    built.Add(row.gameObject);

                    for (int c = 0; c < section.columns; c++)
                    {
                        KanaEntry entry = section.entries[r * section.columns + c];
                        KanaSection owner = section;
                        KanaCell cell = Instantiate(cellTemplate, row);
                        cell.gameObject.SetActive(true);
                        cell.Show(entry.In(script), entry.romaji, () => Picked(cell, entry, owner));

                        if (!entry.IsBlank && entry.kana == pickedKana)
                        {
                            cell.SetPicked(true);
                            picked = cell;
                        }
                    }
                }

                string note = section.Note(script);
                if (string.IsNullOrEmpty(note)) continue;
                TMP_Text noteText = Instantiate(noteTemplate, list);
                noteText.gameObject.SetActive(true);
                noteText.text = note;
                built.Add(noteText.gameObject);
            }
        }

        private void Picked(KanaCell cell, KanaEntry entry, KanaSection section)
        {
            if (picked != null) picked.SetPicked(false);
            picked = cell;
            pickedKana = entry.kana;
            cell.SetPicked(true);

            detailPair.gameObject.SetActive(true);
            detailPair.text = section.katakanaOnly ? entry.Katakana : entry.kana + "　" + entry.Katakana;
            detailRomaji.text = entry.romaji;
            detailCaption.text = section.katakanaOnly ? "Katakana only, for loanwords" : "Hiragana and katakana";
        }

        private void ClearDetail()
        {
            detailPair.gameObject.SetActive(false);
            detailRomaji.text = "Tap a kana";
            detailCaption.text = "to see it in both scripts";
        }
    }
}
