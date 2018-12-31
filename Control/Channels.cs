namespace SBR {
    public abstract class Channels {
        public Channels() {
            ClearInput(true);
        }

        public abstract void ClearInput(bool force = false);
    }
}