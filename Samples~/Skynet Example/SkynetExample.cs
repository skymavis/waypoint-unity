using System;
using System.Collections;
using System.Text;
using SkyMavis;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Skynet example using Waypoint authorization to get owner address and callback to handle response
/// </summary>
public class SkynetExample : MonoBehaviour
{
    private const string SkynetNftUrl = "https://api-gateway.skymavis.com/skynet/ronin/nfts/search";
    private const string SkynetTokenBalanceUrl = "https://api-gateway.skymavis.com/skynet/ronin/tokens/balances/search";
    private static readonly string[] NftContractAddresses = new[] {
        "0x32950db2a7164ae833121501c797d79e7b79d74c", // Axie
    };
    private static readonly string[] Erc20ContractAddresses = new[] {
        "0x97a9107c1793bc407d6f527b77e7fff4d812bece", // AXS
        "0xa8754b9fa15fc18bb59458815510e40a12cd2014", // SLP
        "0xe514d9deb7966c8be0ca922de8a064264ea6bcd4", // WRON
        "0xc99a6a985ed2cac1ef41640596c5a5f9f4e19ef5", // WETH
        "0x7eae20d11ef8c779433eb24503def900b9d28ad7", // PIXEL
        "0x0b7007c13325c48911f73a2dad5fa5dcbf808adc", // USDC
    };

    [Header("Setup Waypoint")]
    public WaypointSettings waypointSettings = new WaypointSettings()
    {
        network = WaypointSettings.Network.Testnet,
    };
    [Min(1f)]
    public float uiScale = 1.5f;

    [Header("Setup Skynet")]
    public string skynetApiKey;

    private bool _hasMavisHubArgs;
    private int _step = 1;
    private string _ownerAddress;
    private string _lastResponse;

    void Awake()
    {
        if (WaypointSettings.TryGetMavisHubArgs(out var sessionID, out var port))
        {
            _hasMavisHubArgs = true;
            waypointSettings.mavisHubSessionID = sessionID;
            waypointSettings.mavisHubPort = port;
        }
    }

    void OnDestroy()
    {
        Waypoint.CleanUp();
    }

    private void OnGUI()
    {
        GUI.matrix = Matrix4x4.Scale(uiScale * Vector3.one);

        using (new GUILayout.AreaScope(new Rect(20f, 20f, Screen.width / uiScale - 40f, Screen.height / uiScale - 40f)))
        {
            GUI.enabled = _step == 1;
            GUILayout.Label("Step 1: Configure Waypoint and Skynet in Skynet Example GameObject");
            if (_hasMavisHubArgs) GUILayout.Label("Mavis Hub related properties are configured programmatically from command line arguments.");
            GUILayout.Label($"Mavis Hub Session ID: {waypointSettings.mavisHubSessionID}");
            GUILayout.Label($"Mavis Hub Port: {waypointSettings.mavisHubPort}");
            GUILayout.Label($"OAuth Client ID: {waypointSettings.clientID}");
            GUILayout.Label($"Deep Link Callback URL: {waypointSettings.deepLinkCallbackURL}");
            GUILayout.Label($"Endpoint: {waypointSettings.endpoint}");
            GUILayout.Label($"Chain ID: {waypointSettings.network.chainID}");
            GUILayout.Label($"RPC URL: {waypointSettings.network.rpcURL}");

            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Mainnet")) UseMainnet();
                if (GUILayout.Button("Use Testnet")) UseTestnet();
            }

            if (GUILayout.Button("Waypoint: Initialize")) InitializeWaypoint();

            GUI.enabled = true;
            GUILayout.Space(10);
            GUILayout.Label($"Waypoint.IsConnected == {Waypoint.IsConnected}");
            GUILayout.Label($"The Waypoint API can be called only when Waypoint is connected.");
            GUILayout.Space(10);

            GUI.enabled = _step == 2 && Waypoint.IsConnected;
            GUILayout.Label("Step 2: Authorize using Waypoint SDK");
            if (GUILayout.Button("Waypoint: Authorize")) AuthorizeWaypoint();

            GUI.enabled = _step == 3;
            GUILayout.Label("Step 3: Wait for Waypoint response");

            GUI.enabled = _step == 4;
            GUILayout.Label("Step 4: Extract owner address from response JSON");
            GUILayout.Label($"Owner address: {_ownerAddress ?? "<empty>"}");
            if (GUILayout.Button("Skynet: Get NFTs")) StartCoroutine(GetNfts());
            if (GUILayout.Button("Skynet: Get ERC-20 token balances")) StartCoroutine(GetErc20TokenBalances());
            GUILayout.Label("Last response");
            GUILayout.TextArea(_lastResponse, GUILayout.ExpandHeight(true));
        }
    }

    private void UseMainnet() => waypointSettings.network = WaypointSettings.Network.Mainnet;

    private void UseTestnet() => waypointSettings.network = WaypointSettings.Network.Testnet;

    private void InitializeWaypoint()
    {
        if (WaypointSettings.TryGetMavisHubArgs(out var sessionID, out var port))
        {
            waypointSettings.mavisHubSessionID = sessionID;
            waypointSettings.mavisHubPort = port;
        }

        Waypoint.SetUp(waypointSettings);
        _step = 2;
    }

    private void AuthorizeWaypoint()
    {
        _step = 3;

        ExecuteOnWaypointResponse(
            Waypoint.Authorize(),
            data =>
            {
                _lastResponse = $"Waypoint authorize response: {data}";
                Debug.Log(_lastResponse);
                var response = JsonUtility.FromJson<WaypointAuthorizeResponse>(data);

                if (response.type == "success")
                {
                    _ownerAddress = response.data.accountWallet.identity;
                    _step = 4;
                }
            }
        );
    }

    void ExecuteOnWaypointResponse(string requestId, Action<string> callback)
    {
        Waypoint.ResponseReceived += OnResponseReceived;

        void OnResponseReceived(string state, string data)
        {
            if (state == requestId)
            {
                Waypoint.ResponseReceived -= OnResponseReceived;
                callback(data);
            }
        }
    }

    private IEnumerator GetNfts() => SendAndLogSkynetRequest(SkynetNftUrl, new SkynetRequestBody
    {
        ownerAddress = _ownerAddress,
        contractAddresses = NftContractAddresses,
        paging = SkynetPaging.Default,
    });

    private IEnumerator GetErc20TokenBalances() => SendAndLogSkynetRequest(SkynetTokenBalanceUrl, new SkynetRequestBody
    {
        ownerAddress = _ownerAddress,
        contractAddresses = Erc20ContractAddresses,
        includes = new[] { "RON" },
        tokenStandards = new[] { "ERC20" },
        paging = SkynetPaging.Default,
    });

    private IEnumerator SendAndLogSkynetRequest(string url, SkynetRequestBody body)
    {
        using var request = CreateSkynetRequest(url, JsonUtility.ToJson(body));
        yield return request.SendWebRequest();
        _lastResponse = Encoding.UTF8.GetString(request.downloadHandler.data);
        Debug.Log(_lastResponse);
    }

    private UnityWebRequest CreateSkynetRequest(string url, string json)
    {
        var request = new UnityWebRequest(
            url,
            UnityWebRequest.kHttpVerbPOST,
            new DownloadHandlerBuffer(),
            new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)) { contentType = "application/json" }
        );
        request.SetRequestHeader("X-API-KEY", skynetApiKey);
        return request;
    }

    [Serializable]
    private struct WaypointAuthorizeResponse
    {
        public string type;
        public Data data;

        [Serializable]
        public struct Data
        {
            public AccountWallet accountWallet;

            [Serializable]
            public struct AccountWallet
            {
                public string identity;
            }
        }
    }

    [Serializable]
    private struct SkynetRequestBody
    {
        public string[] contractAddresses;
        public string[] includes;
        public string ownerAddress;
        public string[] tokenIds;
        public string[] tokenStandards;
        public SkynetPaging paging;
    }

    [Serializable]
    private struct SkynetPaging
    {
        public static readonly SkynetPaging Default = new SkynetPaging
        {
            offset = 0,
            limit = 200,
        };

        public int offset;
        public int limit;
    }
}
