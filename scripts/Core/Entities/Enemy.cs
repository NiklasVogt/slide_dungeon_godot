namespace Dungeon2048.Core.Entities
{
    public enum EnemyType
    {
        Goblin,
        Orc,
        Dragon,
        Boss,
        Masochist,
        Thorns
    }

    public sealed class Enemy : EntityBase
    {
        public EnemyType Type;
        public int EnemyLevel;
        public bool IsBoss;

        public Enemy(int x, int y, EnemyType type, int enemyLevel, bool isBoss = false)
            : base(x, y, CalcHp(type, enemyLevel, isBoss), CalcAtk(type, enemyLevel, isBoss))
        {
            Type = type;
            EnemyLevel = enemyLevel;
            IsBoss = isBoss;
        }

        static int CalcHp(EnemyType type, int level, bool isBoss)
        {
            int baseHp = type switch
            {
                EnemyType.Goblin    => 8,
                EnemyType.Orc       => 15,
                EnemyType.Dragon    => 25,
                EnemyType.Boss      => 50,
                EnemyType.Masochist => 18,
                EnemyType.Thorns    => 16,
                _ => 10
            };
            double levelMul = System.Math.Pow(1 + level * 0.4, 1.2);
            double bossMul = isBoss ? 2.5 : 1.0;
            return (int)System.Math.Round(baseHp * levelMul * bossMul);
        }

        static int CalcAtk(EnemyType type, int level, bool isBoss)
        {
            int baseAtk = type switch
            {
                EnemyType.Goblin    => 2,
                EnemyType.Orc       => 4,
                EnemyType.Dragon    => 7,
                EnemyType.Boss      => 12,
                EnemyType.Masochist => 1,
                EnemyType.Thorns    => 0,
                _ => 1
            };
            double mult = type switch
            {
                EnemyType.Goblin    => 1.2,
                EnemyType.Orc       => 1.8,
                EnemyType.Dragon    => 2.5,
                EnemyType.Boss      => 3.0,
                EnemyType.Masochist => 1.0,
                EnemyType.Thorns    => 0.5,
                _ => 1.0
            };
            double levelBonus = level * mult;
            double bossMul = isBoss ? 1.8 : 1.0;
            return (int)System.Math.Round((baseAtk + levelBonus) * bossMul);
        }

        public int XpReward => Player.CalculateXpReward(Type, EnemyLevel, IsBoss);
        public bool IsElite => IsBoss || EnemyLevel >= 5;

        public string DisplayName => IsBoss
            ? "Boss"
            : Type switch
            {
                EnemyType.Goblin    => "Goblin",
                EnemyType.Orc       => "Orc",
                EnemyType.Dragon    => "Dragon",
                EnemyType.Masochist => "Masochist",
                EnemyType.Thorns    => "Thorns",
                _ => "Enemy"
            };
    }
}
