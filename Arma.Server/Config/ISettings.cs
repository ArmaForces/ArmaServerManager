namespace Arma.Server.Config {
    public interface ISettings {
        object GetSettingsValue(string key);
        string GetServerPath();
        string GetServerExePath();
    }
}