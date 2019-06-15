namespace SBR {
    /// <summary>
    /// Used to store information about Damage being applied.
    /// This class can be extended to implement custom damage types, such as elemental damage.
    /// These damage types can then be processed via the ProcessDamage event in Health.
    /// </summary>
    public class Damage {
        /// <summary>
        /// Create a new Damage instance.
        /// </summary>
        /// <param name="amount">Amount of damage.</param>
        public Damage(float amount) {
            this.amount = amount;
        }

        /// <summary>
        /// Amount of damage that is applied.
        /// </summary>
        public float amount;
    }
}