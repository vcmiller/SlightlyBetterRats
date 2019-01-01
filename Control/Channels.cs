namespace SBR {
    public abstract class Channels {
        public Channels() {
            ClearInput(true);
        }

        public virtual void ClearInput(bool force = false) { }
    }
}