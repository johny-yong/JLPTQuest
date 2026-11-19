using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JLPTQuest
{
    public enum FloorState { Locked, Next, Cleared }

    /*
        One floor on the tower. Cleared floors are wooden plaques with their stars, the floor you're on is a lit plaque
        that glows (glow means "go here"), locked ones are dim lacquer. The boss sits on top in black and gold.
        A Hard clear adds a red 難 after the stars, Very hard a 極. Floor numbers are written the old way: 壱 弐 参...
    */
    public class FloorTile : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image background;
        [SerializeField] private Image rim;
        [SerializeField] private Image glow;
        [SerializeField] private Image node; //The dot on the path up the tower
        [SerializeField] private Image badge;
        [SerializeField] private TMP_Text badgeLabel;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private TMP_Text state;
        [SerializeField] private Sprite lacquer;
        [SerializeField] private Sprite wood;
        [SerializeField] private Sprite woodLit;

        private static readonly string[] numerals = { "壱", "弐", "参", "四", "五", "六", "七", "八", "九", "十" };
        private static readonly Color bossTile = new Color(0.12f, 0.09f, 0.15f);
        private static readonly Color bossBadge = new Color(0.16f, 0.08f, 0.08f);
        private static readonly Color lockedNode = new Color(0.16f, 0.15f, 0.21f);
        private static readonly Color litSubtitle = new Color(0.95f, 0.85f, 0.77f);
        private static readonly Color woodSubtitle = new Color(0.84f, 0.76f, 0.67f);

        private Action onPicked;

        void Awake()
        {
            button.onClick.AddListener(() => onPicked?.Invoke());

            //The locked look is done by hand below, the button's own grey-out would fade it twice
            ColorBlock colors = button.colors;
            colors.disabledColor = Color.white;
            button.colors = colors;
        }

        public void Show(StageData stage, bool boss, FloorState floorState, bool[] stars, Difficulty? hardest, Action onPicked)
        {
            this.onPicked = onPicked;
            bool locked = floorState == FloorState.Locked;
            bool cleared = floorState == FloorState.Cleared;
            bool next = floorState == FloorState.Next;
            button.interactable = !locked;

            badgeLabel.text = boss ? "鬼" : Numeral(stage.floor);
            title.text = boss ? "Boss floor" : "Floor " + stage.floor;
            subtitle.text = stage.title;
            state.text = cleared ? StarText.Line(stars) + Stamp(hardest) : next ? "Next" : "Locked";
            glow.enabled = next;

            if (boss)
            {
                Paint(lacquer, bossTile, InkColors.Gold, 1f);
                badge.color = bossBadge;
                badgeLabel.color = InkColors.VermilionGlow;
                title.color = InkColors.GoldLight;
                subtitle.color = InkColors.MoonFaint;
                state.color = next ? InkColors.GoldLight : InkColors.MoonFaint;
                node.color = next ? InkColors.VermilionGlow : cleared ? InkColors.Matcha : InkColors.Gold;
                SetAlpha(locked ? 0.75f : 1f);
                return;
            }

            SetAlpha(1f);
            if (next)
            {
                Paint(woodLit, Color.white, InkColors.VermilionGlow, 1f);
                badge.color = InkColors.Vermilion;
                badgeLabel.color = InkColors.OnVermilion;
                title.color = InkColors.OnVermilion;
                subtitle.color = litSubtitle;
                state.color = InkColors.GoldLight;
                node.color = InkColors.VermilionGlow;
            }
            else if (cleared)
            {
                Paint(wood, Color.white, InkColors.Gold, 0.5f);
                badge.color = InkColors.MatchaDeep;
                badgeLabel.color = InkColors.Moon;
                title.color = InkColors.Moon;
                subtitle.color = woodSubtitle;
                state.color = InkColors.GoldLight;
                node.color = InkColors.Matcha;
            }
            else
            {
                Paint(lacquer, InkColors.LacquerLow, InkColors.Gold, 0.15f);
                badge.color = InkColors.Lacquer;
                badgeLabel.color = InkColors.MoonDim;
                title.color = InkColors.MoonFaint;
                subtitle.color = InkColors.MoonDim;
                state.color = InkColors.MoonDim;
                node.color = lockedNode;
            }
        }

        private void Paint(Sprite sprite, Color tint, Color rimColor, float rimAlpha)
        {
            background.sprite = sprite;
            background.color = tint;
            rim.color = InkColors.WithAlpha(rimColor, rimAlpha);
        }

        private void SetAlpha(float alpha)
        {
            var group = GetComponent<CanvasGroup>();
            if (group != null) group.alpha = alpha;
        }

        public static string Numeral(int floor)
        {
            return floor >= 1 && floor <= numerals.Length ? numerals[floor - 1] : floor.ToString();
        }

        private static string Stamp(Difficulty? hardest)
        {
            string glyph = hardest.HasValue ? DifficultyText.Badge(hardest.Value) : string.Empty;
            return glyph.Length == 0 ? string.Empty : "  <color=" + InkColors.ToTag(InkColors.VermilionGlow) + ">" + glyph + "</color>";
        }
    }
}
