namespace Arma.Modset {
    public class Modset : IModset {
        private string _modsetName = "default";

        public string GetName() => _modsetName;
    }
}