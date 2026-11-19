using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //The equipped companion's card: art, name, what they do and their skill, all read from their CompanionData
    public class CompanionsScreen : MenuPanel
    {
        [SerializeField] private Image art;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text role;
        [SerializeField] private TMP_Text about;
        [SerializeField] private TMP_Text skillGlyph;
        [SerializeField] private TMP_Text skillName;
        [SerializeField] private TMP_Text skillText;

        public override void Open()
        {
            base.Open();
            CompanionData companion = Router.Companion;
            if (companion == null) return;

            if (art != null)
            {
                art.sprite = companion.portrait;
                art.enabled = companion.portrait != null;
                art.preserveAspect = true;
                art.rectTransform.localScale = new Vector3(companion.artFacesLeft ? -1f : 1f, 1f, 1f);
            }
            nameLabel.text = companion.displayName + "  <size=70%><color=" + InkColors.ToTag(InkColors.MoonSoft) + ">" + companion.kana + "  ·  " + companion.glyph + "</color></size>";
            if (role != null) role.text = companion.role.ToUpperInvariant();
            about.text = companion.blurb;
            if (skillGlyph != null) skillGlyph.text = companion.skillGlyph;
            if (skillName != null) skillName.text = companion.skillName;
            if (skillText != null) skillText.text = companion.skillText;
        }
    }
}
