namespace ArmaForces.Arma.Server.Config
{
    public class ConfigSimpleModel
    {
        public BasicConfigSimpleModel Basic { get; set; } = null!;

        public ServerConfigSimpleModel Server { get; set; } = null!;
    }

    public class BasicConfigSimpleModel
    {
    }

    public class ServerConfigSimpleModel
    {
        public string Password { get; set; } = string.Empty;
    }
}
