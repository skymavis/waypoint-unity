using System;
using UnityEngine;

namespace SkyMavis.Waypoint
{
    [Serializable]
    public class WaypointSettings
    {
        [Tooltip("Leave empty to auto-detect from command line arguments.")]
        public string mavisHubSessionID;
        [Tooltip("Leave negative to auto-detect from command line arguments.")]
        public int mavisHubPort = -1;
        public string clientID = "<provided by Sky Mavis Dev Portal>";
        public string endpoint = "https://waypoint.roninchain.com";
        public string deepLinkCallbackURL = "schema://host";
        public Network network = Network.Mainnet;

        [Serializable]
        public struct Network
        {
            public static readonly Network Mainnet = new Network()
            {
                chainID = 2020,
                rpcURL = "https://api.roninchain.com/rpc",
            };
            public static readonly Network Testnet = new Network()
            {
                chainID = 2021,
                rpcURL = "https://saigon-testnet.roninchain.com/rpc",
            };

            public int chainID;
            public string rpcURL;
        }

        internal string MavisHubSessionID => string.IsNullOrEmpty(mavisHubSessionID) ? GetCommandLineArgs("-sessionId") : mavisHubSessionID;

        internal int MavisHubPort => mavisHubPort < 0 && int.TryParse(GetCommandLineArgs("-hubPort"), out var port) ? port : mavisHubPort;

        private static string GetCommandLineArgs(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

    }
}
