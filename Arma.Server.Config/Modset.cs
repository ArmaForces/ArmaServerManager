namespace Arma.Server.Config {
    public class Modset: IModset {
        private string _modsetName = "default";

        public string GetName() => _modsetName;
    }
}