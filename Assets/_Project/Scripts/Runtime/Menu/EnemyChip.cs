using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    //A little enemy card on the floor preview. The face is just the first kana until enemy art exists
    public class EnemyChip : MonoBehaviour
    {
        [SerializeField] private Image face;
        [SerializeField] private TMP_Text glyph;
        [SerializeField] private TMP_Text nameLabel;

        public void Show(EnemyData enemy)
        {
            glyph.text = string.IsNullOrEmpty(enemy.nameJp) ? "?" : enemy.nameJp.Substring(0, 1);
            nameLabel.text = enemy.displayName;

            //Elites and bosses stand out in vermilion
            bool tough = enemy.kind != EnemyKind.Grunt;
            face.color = tough ? InkColors.Vermilion : InkColors.Lacquer;
            glyph.color = tough ? InkColors.OnVermilion : InkColors.MoonSoft;
        }
    }
}
