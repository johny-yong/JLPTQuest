using TMPro;
using UnityEngine;

namespace JLPTQuest
{
    /*
        Put this next to a TextMeshPro text and write into markup instead of text.
        {先生|せんせい} gets its reading drawn on top. Give the text some line spacing so the readings have room.
        Auto Size on the TMP text isn't supported, keep a fixed font size.
    */
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class RubyText : MonoBehaviour
    {
        [SerializeField, TextArea(2, 6)] private string markup = "わたしの {先生|せんせい}";
        [SerializeField, Range(0.3f, 0.8f)] private float rubyScale = 0.5f;
        [SerializeField] private float rubyOffset = 1f; //How high the reading floats, in em

        private TMP_Text label;
        private float lastFontSize = -1f;
        private bool dirty = true;

        public string Markup
        {
            get => markup;
            set
            {
                markup = value;
                Refresh();
            }
        }

        void OnEnable()
        {
            FuriganaSettings.Changed += MarkDirty;
            dirty = true;
        }

        void OnDisable()
        {
            FuriganaSettings.Changed -= MarkDirty;
        }

        void OnValidate()
        {
            dirty = true; //TMP doesn't like being measured mid OnValidate, so wait for the next Update
        }

        void Update()
        {
            if (label == null) label = GetComponent<TMP_Text>();
            if (dirty || !Mathf.Approximately(label.fontSize, lastFontSize)) Refresh();
        }

        public void Refresh()
        {
            if (label == null) label = GetComponent<TMP_Text>();
            dirty = false;
            lastFontSize = label.fontSize;

            if (!FuriganaSettings.ShowFurigana)
            {
                label.text = RubyMarkup.StripReadings(markup);
                return;
            }

            float size = Mathf.Max(1f, label.fontSize);
            label.text = RubyMarkup.ToRichText(markup, part => label.GetPreferredValues(part).x / size, rubyScale, rubyOffset);
        }

        private void MarkDirty()
        {
            dirty = true;
        }
    }
}
