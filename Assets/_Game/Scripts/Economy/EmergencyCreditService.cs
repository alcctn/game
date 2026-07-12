namespace CleanEnergy.Economy
{
    /// <summary>
    /// One-time bailout when the money wallet hits zero after upkeep.
    /// </summary>
    public sealed class EmergencyCreditService
    {
        public const float CreditAmount = 200f;

        public bool HasBeenUsed { get; private set; }

        public void Reset()
        {
            HasBeenUsed = false;
        }

        public void Restore(bool used)
        {
            HasBeenUsed = used;
        }

        /// <summary>
        /// If money is depleted and credit unused, grants CreditAmount and returns true.
        /// </summary>
        public bool TryGrant(Wallet wallet)
        {
            if (wallet == null || HasBeenUsed || wallet.Money > 0.0001f)
            {
                return false;
            }

            wallet.Add(CreditAmount);
            HasBeenUsed = true;
            return true;
        }
    }
}
