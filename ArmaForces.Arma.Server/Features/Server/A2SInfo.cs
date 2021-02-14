using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArmaForces.Arma.Server.Features.Server
{
    /// <summary>
    /// Adopted from Valve description: https://developer.valvesoftware.com/wiki/Server_queries#A2S_INFO
    /// Source of code: https://www.techpowerup.com/forums/threads/snippet-c-net-steam-a2s_info-query.229199/#post-3579408
    /// </summary>
    public class A2SInfo
    {
        public byte Header { get; protected set; } // I
        public byte Protocol { get; protected set; }
        public string Name { get; protected set; }
        public string Map { get; protected set; }
        public string Folder { get; protected set; }
        public string Game { get; protected set; }
        public short Id { get; protected set; }
        public byte Players { get; protected set; }
        public byte MaxPlayers { get; protected set; }
        public byte Bots { get; protected set; }
        public ServerTypeFlags ServerType { get; protected set; }
        public EnvironmentFlags Environment { get; protected set; }
        public VisibilityFlags Visibility { get; protected set; }
        public VACFlags VAC { get; protected set; }
        public string Version { get; protected set; }

        public ExtraDataFlags ExtraDataFlag { get; set; }

        // \xFF\xFF\xFF\xFFTSource Engine Query\x00 because UTF-8 doesn't like to encode 0xFF
        public static readonly byte[] Request =
        {
            0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65,
            0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00
        };

        public static async Task<A2SInfo> GetServerInfoAsync(IPEndPoint ipEndPoint, CancellationToken cancellationToken)
        {
            using var udpClient = new UdpClient();

            try
            {
                return await Task.Run(
                    () => ReadServerInfo(udpClient, ipEndPoint, cancellationToken),
                    cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Connection timed out
                return new A2SInfo();
            }
            catch (SocketException)
            {
                // Connection error
                return null;
            }
        }

        private static async Task<A2SInfo> ReadServerInfo(UdpClient udpClient, IPEndPoint ipEndPoint, CancellationToken cancellationToken)
        {
            var serverInfo = new A2SInfo();

            // TODO: use logger
            udpClient.Connect(ipEndPoint);

            Console.WriteLine($"{DateTime.Now:s} Sending UDP request to {ipEndPoint}.");

            await udpClient.SendAsync(
                Request,
                Request.Length);

            Console.WriteLine($"{DateTime.Now:s} UDP request sent to {ipEndPoint}.");

            var receiveTask = udpClient.ReceiveAsync();
            while (!receiveTask.IsCompleted)
            {
                var delayTask = Task.Delay(1000, cancellationToken);
                await Task.WhenAny(receiveTask, delayTask);
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"{DateTime.Now:s} Connection to {ipEndPoint} timed out.");
                    throw new TaskCanceledException($"Connection to {ipEndPoint} timed out");
                }
            }

            var udpReceiveResult = await receiveTask;
            Console.WriteLine($"{DateTime.Now:s} UDP request received from {ipEndPoint}. Reading started.");

            await using var memoryStream = new MemoryStream(udpReceiveResult.Buffer); // Saves the received data in a memory buffer
            using var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8); // A binary reader that treats characters as Unicode 8-bit
            memoryStream.Seek(4, SeekOrigin.Begin); // skip the 4 0xFFs

            serverInfo.Header = binaryReader.ReadByte();
            serverInfo.Protocol = binaryReader.ReadByte();
            serverInfo.Name = ReadNullTerminatedString(binaryReader);
            serverInfo.Map = ReadNullTerminatedString(binaryReader);
            serverInfo.Folder = ReadNullTerminatedString(binaryReader);
            serverInfo.Game = ReadNullTerminatedString(binaryReader);
            serverInfo.Id = binaryReader.ReadInt16();
            serverInfo.Players = binaryReader.ReadByte();
            serverInfo.MaxPlayers = binaryReader.ReadByte();
            serverInfo.Bots = binaryReader.ReadByte();
            serverInfo.ServerType = (ServerTypeFlags)binaryReader.ReadByte();
            serverInfo.Environment = (EnvironmentFlags)binaryReader.ReadByte();
            serverInfo.Visibility = (VisibilityFlags)binaryReader.ReadByte();
            serverInfo.VAC = (VACFlags)binaryReader.ReadByte();
            serverInfo.Version = ReadNullTerminatedString(binaryReader);
            serverInfo.ExtraDataFlag = (ExtraDataFlags)binaryReader.ReadByte();

            #region These EDF readers have to be in this order because that's the way they are reported

            if (serverInfo.ExtraDataFlag.HasFlag(ExtraDataFlags.Port))
                serverInfo.Port = binaryReader.ReadInt16();
            if (serverInfo.ExtraDataFlag.HasFlag(ExtraDataFlags.SteamId))
                serverInfo.SteamId = binaryReader.ReadUInt64();
            if (serverInfo.ExtraDataFlag.HasFlag(ExtraDataFlags.Spectator))
            {
                serverInfo.SpectatorPort = binaryReader.ReadInt16();
                serverInfo.Spectator = ReadNullTerminatedString(binaryReader);
            }

            if (serverInfo.ExtraDataFlag.HasFlag(ExtraDataFlags.Keywords))
                serverInfo.Keywords = ReadNullTerminatedString(binaryReader);
            if (serverInfo.ExtraDataFlag.HasFlag(ExtraDataFlags.GameId))
                serverInfo.GameId = binaryReader.ReadUInt64();

            #endregion

            Console.WriteLine($"{DateTime.Now:s} Successfully read message from {ipEndPoint}.");

            return serverInfo;
        }
        
        /// <summary>Reads a null-terminated string into a .NET Framework compatible string.</summary>
        /// <param name="input">
        ///     Binary reader to pull the null-terminated string from.  Make sure it is correctly positioned in the
        ///     stream before calling.
        /// </param>
        /// <returns>String of the same encoding as the input BinaryReader.</returns>
        private static string ReadNullTerminatedString(BinaryReader input)
        {
            var stringBuilder = new StringBuilder();
            var readChar = input.ReadChar();
            while (readChar != '\x00')
            {
                stringBuilder.Append(readChar);
                readChar = input.ReadChar();
            }

            return stringBuilder.ToString();
        }

        #region Strong Typing Enumerators

        [Flags]
        public enum ExtraDataFlags : byte
        {
            GameId = 0x01,
            SteamId = 0x10,
            Keywords = 0x20,
            Spectator = 0x40,
            Port = 0x80
        }

        public enum VACFlags : byte
        {
            Unsecured = 0,
            Secured = 1
        }

        public enum VisibilityFlags : byte
        {
            Public = 0,
            Private = 1
        }

        public enum EnvironmentFlags : byte
        {
            Linux = 0x6C, //l
            Windows = 0x77, //w
            Mac = 0x6D, //m
            MacOsX = 0x6F //o
        }

        public enum ServerTypeFlags : byte
        {
            Dedicated = 0x64, //d
            Nondedicated = 0x6C, //l
            SourceTV = 0x70 //p
        }

        #endregion

        #region Extra Data Flag Members

        public ulong GameId { get; protected set; } //0x01
        public ulong SteamId { get; protected set; } //0x10
        public string Keywords { get; protected set; } //0x20
        public string Spectator { get; protected set; } //0x40
        public short SpectatorPort { get; protected set; } //0x40
        public short Port { get; protected set; } //0x80

        #endregion
    }
}
