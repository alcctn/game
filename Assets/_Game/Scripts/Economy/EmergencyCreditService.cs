using UnityEngine;

namespace CleanEnergy.Economy
{
    /// <summary>
    /// One-time bailout when the money wallet hits zero after upkeep, plus interest debt.
    /// </summary>
    public sealed class EmergencyCreditService
    {
        public const float CreditAmount = 200f;
        public const float InterestRate = 0.01f;

        public bool HasBeenUsed { get; private set; }
        public float RemainingDebt { get; private set; }

        public void Reset()
        {
            HasBeenUsed = false;
            RemainingDebt = 0f;
        }

        public void Restore(bool used, float debt = 0f)
        {
            HasBeenUsed = used;
            RemainingDebt = used ? Mathf.Max(0f, debt) : 0f;
        }

        /// <summary>
        /// If money is depleted and credit unused, grants CreditAmount and opens debt.
        /// </summary>
        public bool TryGrant(Wallet wallet)
        {
            if (wallet == null || HasBeenUsed || wallet.Money > 0.0001f)
            {
                return false;
            }

            wallet.Add(CreditAmount);
            HasBeenUsed = true;
            RemainingDebt = CreditAmount;
            return true;
        }

        /// <summary>Interest for one tick: ceil(debt * 1%) with a minimum of 1.</summary>
        public static float CalculateInterest(float debt)
        {
            if (debt <= 0.0001f)
            {
                return 0f;
            }

            return Mathf.Max(1f, Mathf.Ceil(debt * InterestRate));
        }

        /// <summary>
        /// Charges interest from the wallet without changing principal debt.
        /// </summary>
        public float ProcessInterestTick(Wallet wallet)
        {
            if (RemainingDebt <= 0.0001f || wallet == null)
            {
                return 0f;
            }

            var interest = CalculateInterest(RemainingDebt);
            if (!wallet.TrySpend(interest))
            {
                wallet.SetMoney(0f);
            }

            return interest;
        }

        /// <summary>Spends the full remaining debt to clear it.</summary>
        public bool TryRepay(Wallet wallet)
        {
            if (RemainingDebt <= 0.0001f || wallet == null)
            {
                return false;
            }

            if (!wallet.TrySpend(RemainingDebt))
            {
                return false;
            }

            RemainingDebt = 0f;
            return true;
        }
    }
}
