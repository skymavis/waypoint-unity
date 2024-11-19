using System;

namespace SkyMavis
{
    [Serializable]
    public class WaypointSettings
    {
        public static bool TryGetMavisHubArgs(out string sessionID, out int port)
        {
            var args = Environment.GetCommandLineArgs();
            sessionID = null;
            port = -1;

            for (var i = 0; i < args.Length - 1; i++)
            {
                switch (args[i])
                {
                    case "-sessionId":
                        sessionID = args[i + 1];
                        break;
                    case "-hubPort":
                        if (!int.TryParse(args[i + 1], out port)) port = -1;
                        break;
                }
            }

            return sessionID != null && port >= 0;
        }

        /// <summary>
        /// Use <see cref="TryGetMavisHubArgs"/> to check if Mavis Hub session ID is provided via command line arguments.
        /// </summary>
        public string mavisHubSessionID;
        /// <summary>
        /// Use <see cref="TryGetMavisHubArgs"/> to check if Mavis Hub port is provided via command line arguments.
        /// </summary>
        public int mavisHubPort;
        /// <summary>
        /// Provided by <a href="https://developers.skymavis.com/console/id-service/">Sky Mavis Dev Portal</a>
        /// </summary>
        public string clientID = "<provided by Sky Mavis Dev Portal>";
        /// <summary>
        /// Reference: <a href="https://docs.unity3d.com/Manual/deep-linking.html">Unity Deep Linking Manual</a>
        /// </summary>
        public string deepLinkCallbackURL = "schema://host";
        public string endpoint = "https://waypoint.roninchain.com";
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
    }
}
