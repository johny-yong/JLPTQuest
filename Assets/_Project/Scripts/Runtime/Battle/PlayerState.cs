using System;

namespace JLPTQuest
{
    //Your side of the fight. Only failed parries hurt, heart turns heal in Phase 4
    public class PlayerState
    {
        public int MaxHp { get; }
        public int Hp { get; private set; }

        public bool IsDefeated => Hp <= 0;
        public float HpFraction => MaxHp > 0 ? (float)Hp / MaxHp : 0f;

        public PlayerState(int maxHp)
        {
            MaxHp = Math.Max(1, maxHp);
            Hp = MaxHp;
        }

        public void TakeDamage(int amount)
        {
            Hp = Math.Max(0, Hp - Math.Max(0, amount));
        }

        public void Heal(int amount)
        {
            Hp = Math.Min(MaxHp, Hp + Math.Max(0, amount));
        }
    }
}
