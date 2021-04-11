namespace ArmaForces.Arma.Server.Config
{
    public class ConfigSimpleModel
    {
        public BasicConfigSimpleModel? Basic { get; set; }

        public ServerConfigSimpleModel? Server { get; set; }
    }

    public class BasicConfigSimpleModel
    {
    }

    public class ServerConfigSimpleModel
    {
        public string? Password { get; set; }
    }
}
