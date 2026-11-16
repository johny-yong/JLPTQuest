using System;

namespace JLPTQuest
{
    //The live side of an enemy during a fight. EnemyData is the sheet row, this is what changes
    public class EnemyState
    {
        public int MaxHp { get; }
        public int Hp { get; private set; }
        public int AttackCd { get; }
        public int CooldownLeft { get; private set; }

        public bool IsDefeated => Hp <= 0;
        public float HpFraction => MaxHp > 0 ? (float)Hp / MaxHp : 0f;

        public EnemyState(int maxHp, int attackCd)
        {
            MaxHp = Math.Max(1, maxHp);
            Hp = MaxHp;
            AttackCd = Math.Max(1, attackCd);
            CooldownLeft = AttackCd;
        }

        public void TakeDamage(int amount)
        {
            Hp = Math.Max(0, Hp - Math.Max(0, amount));
        }

        public void TickCooldown(int amount)
        {
            CooldownLeft -= Math.Max(0, amount);
        }

        public void ResetCooldown()
        {
            CooldownLeft = AttackCd;
        }
    }
}
