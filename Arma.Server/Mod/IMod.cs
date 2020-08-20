namespace Arma.Server.Mod {
    public interface IMod {
        string GetName();
        ModSource GetModSource();
        ModType GetModType();
        int GetItemId();
        string GetDirectory();
    }
}