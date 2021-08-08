using System;

namespace ArmaForces.Arma.Server.Exceptions
{
    public class ModNotFoundException : Exception
    {
        public string ModName { get; }

        public ModNotFoundException(string modName, string message) : base(message)
        {
            ModName = modName;
        }
    }
}
