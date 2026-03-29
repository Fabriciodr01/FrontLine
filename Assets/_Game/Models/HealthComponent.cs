using System;

namespace FrontLine.Models
{
    public class HealthComponent
    {
        public int Current { get; private set; }
        public int Max     { get; private set; }
        public bool IsAlive => Current > 0;

        public event Action<int, int> OnDamaged;   // (current, max) after damage
        public event Action            OnDied;

        public HealthComponent(int maxHealth)
        {
            Max = maxHealth;
            Current = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            Current = Math.Max(0, Current - amount);
            OnDamaged?.Invoke(Current, Max);
            if (Current <= 0) OnDied?.Invoke();
        }
    }
}
