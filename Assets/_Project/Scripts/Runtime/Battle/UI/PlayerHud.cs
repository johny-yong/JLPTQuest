using System.Collections;
using TMPro;
using UnityEngine;

namespace JLPTQuest
{
    //Your HP, in the band between the enemy and the question. Hint buttons join it in step 4
    public class PlayerHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text hpLabel;
        [SerializeField] private BarFill hpBar;
        [SerializeField] private float flashSeconds = 0.6f;

        private Coroutine flashRoutine;

        public void Refresh(PlayerState player)
        {
            hpBar.Value = player.HpFraction;
            hpLabel.text = "HP " + player.Hp;
        }

        public void ShowHit()
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(Flash());
        }

        private IEnumerator Flash()
        {
            hpLabel.color = InkColors.Vermilion;
            yield return new WaitForSeconds(flashSeconds);
            hpLabel.color = InkColors.Ink;
            flashRoutine = null;
        }
    }
}
