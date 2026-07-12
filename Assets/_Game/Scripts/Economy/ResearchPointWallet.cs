using UnityEngine;

namespace CleanEnergy.Economy
{
    /// <summary>
    /// Research point wallet separate from money.
    /// </summary>
    public sealed class ResearchPointWallet
    {
        public float Points { get; private set; }

        public ResearchPointWallet(float startingPoints = 0f)
        {
            Points = Mathf.Max(0f, startingPoints);
        }

        public bool CanAfford(float cost) => Points + 0.0001f >= cost;

        public bool TrySpend(float cost)
        {
            if (cost <= 0f)
            {
                return true;
            }

            if (!CanAfford(cost))
            {
                return false;
            }

            Points -= cost;
            return true;
        }

        public void Add(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            Points += amount;
        }

        public void SetPoints(float amount)
        {
            Points = Mathf.Max(0f, amount);
        }
    }
}
