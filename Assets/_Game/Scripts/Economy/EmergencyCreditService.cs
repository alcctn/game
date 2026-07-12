using UnityEngine;

namespace CleanEnergy.Economy
{
    /// <summary>
    /// Emergency bailout when money hits zero after upkeep, with interest debt.
    /// Up to two grants per session (second only after full repay).
    /// </summary>
    public sealed class EmergencyCreditService
    {
        public const float CreditAmount = 200f;
        public const float SecondCreditAmount = 150f;
        public const float InterestRate = 0.01f;
        public const int MaxCreditUses = 2;

        public int CreditUses { get; private set; }
        public bool HasBeenUsed => CreditUses > 0;
        public float RemainingDebt { get; private set; }

        public float NextGrantAmount =>
            CreditUses <= 0 ? CreditAmount : SecondCreditAmount;

        public void Reset()
        {
            CreditUses = 0;
            RemainingDebt = 0f;
        }

        public void Restore(bool used, float debt = 0f)
        {
            Restore(used ? 1 : 0, debt);
        }

        public void Restore(int creditUses, float debt = 0f)
        {
            CreditUses = Mathf.Clamp(creditUses, 0, MaxCreditUses);
            RemainingDebt = CreditUses > 0 ? Mathf.Max(0f, debt) : 0f;
        }

        /// <summary>
        /// If money is depleted, debt is clear, and uses remain, grants the next credit amount.
        /// </summary>
        public bool TryGrant(Wallet wallet)
        {
            if (wallet == null
                || CreditUses >= MaxCreditUses
                || RemainingDebt > 0.0001f
                || wallet.Money > 0.0001f)
            {
                return false;
            }

            var amount = NextGrantAmount;
            wallet.Add(amount);
            CreditUses++;
            RemainingDebt = amount;
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
