using UnityEngine;

namespace CleanEnergy.Economy
{
    /// <summary>
    /// Simple money wallet for placement costs.
    /// </summary>
    public sealed class Wallet
    {
        public float Money { get; private set; }

        public Wallet(float startingMoney)
        {
            Money = Mathf.Max(0f, startingMoney);
        }

        public bool CanAfford(float cost) => Money >= cost;

        public bool TrySpend(float cost)
        {
            if (!CanAfford(cost))
            {
                return false;
            }

            Money -= cost;
            return true;
        }

        public void Add(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            Money += amount;
        }

        public void SetMoney(float amount)
        {
            Money = Mathf.Max(0f, amount);
        }
    }
}
