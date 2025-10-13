using System;

namespace Dungeon2048.Core.Entities
{
    public abstract class EntityBase
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public int X;
        public int Y;
        public int Hp;
        public int Atk;

        protected EntityBase(int x, int y, int hp, int atk)
        {
            X = x; Y = y; Hp = hp; Atk = atk;
        }
    }
}
