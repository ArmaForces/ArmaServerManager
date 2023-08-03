using ArmaForces.Arma.Server.Exceptions;

namespace ArmaForces.Arma.Server.Features.Dlcs.Constants
{
    public static class DlcDirectoryName
    {
        public const string Csla = "csla";
        public const string Gm = "gm";
        public const string Sog = "vn";
        public const string Spe = "spe";

        /// <summary>
        /// Returns default directory name for given <paramref name="dlc"/>.
        /// </summary>
        /// <param name="dlc">AppId of a DLC.</param>
        /// <returns>Default directory name for DLC.</returns>
        /// <exception cref="UnsupportedDlcException">Thrown when default directory name is not available for given <paramref name="dlc"/>.</exception>
        public static string GetName(DlcAppId dlc)
        {
            return dlc switch
            {
                DlcAppId.Unknown => throw new UnsupportedDlcException(DlcAppId.Unknown),
                DlcAppId.Csla => Csla,
                DlcAppId.Gm => Gm,
                DlcAppId.Sog => Sog,
                DlcAppId.Spe => Spe,
                _ => throw new UnsupportedDlcException(dlc)
            };
        }
    }
}
