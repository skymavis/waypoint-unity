using System;
using System.Threading.Tasks;
using SkyMavis;
using UnityEngine;

/// <summary>
/// Basic Waypoint example using asynchronous pattern to handle response
/// </summary>
public class WaypointExample : MonoBehaviour
{
    [Header("Setup Waypoint")]
    public WaypointSettings waypointSettings = new WaypointSettings()
    {
        network = WaypointSettings.Network.Testnet,
    };
    [Min(1f)]
    public float uiScale = 1.5f;

    private bool _hasMavisHubArgs;
    private int _step = 1;
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

    private void OnDestroy()
    {
        Waypoint.CleanUp();
    }

    private void OnGUI()
    {
        GUI.matrix = Matrix4x4.Scale(uiScale * Vector3.one);

        using (new GUILayout.AreaScope(new Rect(20f, 20f, Screen.width / uiScale - 40f, Screen.height / uiScale - 40f)))
        {
            GUI.enabled = _step == 1;
            GUILayout.Label("Step 1: Configure Waypoint settings in the inspector");
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

            if (GUILayout.Button("Initialize")) Initialize();

            GUI.enabled = _step == 2;
            GUILayout.Label("Step 2: Wait for Waypoint initialization");

            GUI.enabled = _step == 3;
            GUILayout.Label("Step 3: Call Waypoint API");
            if (GUILayout.Button("Authorize")) Authorize();
            if (GUILayout.Button("Personal Sign")) PersonalSign();
            if (GUILayout.Button("Sign Typed Data")) SignTypedData();
            if (GUILayout.Button("Send RON")) SendRon();
            if (GUILayout.Button("Approve ERC-20")) ApproveErc20();
            if (GUILayout.Button("Swap RON to AXS")) SwapRonToAxs();
            if (GUILayout.Button("Atia Blessing")) AtiaBlessing();
            if (GUILayout.Button("Auth As Guest")) AuthAsGuest();
            if (GUILayout.Button("Register Guest Account")) RegisterGuestAccount();
            if (GUILayout.Button("Create Keyless Wallet")) CreateKeylessWallet();
            GUILayout.Label("Last response");
            GUILayout.TextArea(_lastResponse, GUILayout.ExpandHeight(true));
        }
    }

    private void UseMainnet() => waypointSettings.network = WaypointSettings.Network.Mainnet;

    private void UseTestnet() => waypointSettings.network = WaypointSettings.Network.Testnet;

    private async void Initialize()
    {
        Waypoint.SetUp(waypointSettings);
        _step = 2;

        while (!Waypoint.IsConnected) await Task.Yield();

        _step = 3;
    }

    private async void Execute(string actionName, Func<string> action)
    {
        _lastResponse = $"{actionName} response: {await WaitForResponse(action())}";
        Debug.Log(_lastResponse);
    }

    private Task<string> WaitForResponse(string requestID)
    {
        var tcs = new TaskCompletionSource<string>();
        Waypoint.ResponseReceived += Callback;
        return tcs.Task;

        void Callback(string state, string data)
        {
            if (state == requestID)
            {
                Waypoint.ResponseReceived -= Callback;
                tcs.SetResult(data);
            }
        }
    }

    private void Authorize()
    {
        var scope = "profile openid email wallet";
        Execute("Authorize", () => Waypoint.Authorize(scope));
    }

    private void PersonalSign() => Execute("Personal Sign", () => Waypoint.PersonalSign("Hello Axie Infinity"));

    private void SignTypedData()
    {
        var typedData = @"{""types"":{""Asset"":[{""name"":""erc"",""type"":""uint8""},{""name"":""addr"",""type"":""address""},{""name"":""id"",""type"":""uint256""},{""name"":""quantity"",""type"":""uint256""}],""Order"":[{""name"":""maker"",""type"":""address""},{""name"":""kind"",""type"":""uint8""},{""name"":""assets"",""type"":""Asset[]""},{""name"":""expiredAt"",""type"":""uint256""},{""name"":""paymentToken"",""type"":""address""},{""name"":""startedAt"",""type"":""uint256""},{""name"":""basePrice"",""type"":""uint256""},{""name"":""endedAt"",""type"":""uint256""},{""name"":""endedPrice"",""type"":""uint256""},{""name"":""expectedState"",""type"":""uint256""},{""name"":""nonce"",""type"":""uint256""},{""name"":""marketFeePercentage"",""type"":""uint256""}],""EIP712Domain"":[{""name"":""name"",""type"":""string""},{""name"":""version"",""type"":""string""},{""name"":""chainId"",""type"":""uint256""},{""name"":""verifyingContract"",""type"":""address""}]}, ""domain"":{""name"":""MarketGateway"",""version"":""1"",""chainId"":2021,""verifyingContract"":""0xfff9ce5f71ca6178d3beecedb61e7eff1602950e""},""primaryType"":""Order"",""message"":{""maker"":""0xd761024b4ef3336becd6e802884d0b986c29b35a"",""kind"":""1"",""assets"":[{""erc"":""1"",""addr"":""0x32950db2a7164ae833121501c797d79e7b79d74c"",""id"":""2730069"",""quantity"":""0""}],""expiredAt"":""1721709637"",""paymentToken"":""0xc99a6a985ed2cac1ef41640596c5a5f9f4e19ef5"",""startedAt"":""1705984837"",""basePrice"":""500000000000000000"",""endedAt"":""0"",""endedPrice"":""0"",""expectedState"":""0"",""nonce"":""0"",""marketFeePercentage"":""425""}}";
        Execute("Sign Typed Data", () => Waypoint.SignTypedData(typedData));
    }

    private void SendRon()
    {
        var receiverAddress = "0xD36deD8E1927dCDD76Bfe0CC95a5C1D65c0a807a";
        var value = "100000000000000000";
        Execute("Send RON", () => Waypoint.SendNativeToken(receiverAddress, value));
    }

    private void ApproveErc20()
    {
        var contractAddress = "0x3c4e17b9056272ce1b49f6900d8cfd6171a1869d";
        var readableAbi = "function approve(address _spender, uint256 _value)";
        // 1 AXS
        var approveParams = new { _spender = "0x6B190089ed7F75Fe17B3b0A17F6ebd69f72c3F63", _value = 1000000000000000000 };
        Execute("Approve ERC-20", () => Waypoint.WriteContract(contractAddress, readableAbi, approveParams));
    }


    private void SwapRonToAxs()
    {
        var katanaAddress = "0xDa44546C0715ae78D454fE8B84f0235081584Fe0";
        var ronAddress = "0xa959726154953bae111746e265e6d754f48570e6";
        var axsAddress = "0x3c4e17b9056272ce1b49f6900d8cfd6171a1869d";
        var walletAddress = "0x6B190089ed7F75Fe17B3b0A17F6ebd69f72c3F63";
        var readableAbi = "function swapExactRONForTokens(uint256 _amountOutMin, address[] _path, address _to, uint256 _deadline) payable";
        // 0.1 RON
        var value = "100000000000000000";
        object swapParams = new
        {
            _amountOutMin = 0,
            _path = new[] { ronAddress, axsAddress },
            _to = walletAddress,
            _deadline = 19140313050
        };
        Execute("Swap RON to AXS", () => Waypoint.WriteContract(katanaAddress, readableAbi, swapParams, value));
    }

    private void AtiaBlessing()
    {
        var walletAddress = "0x6b190089ed7f75fe17b3b0a17f6ebd69f72c3f63";
        var atiaShrineContractAddress = "0xd5c5afefad9ea288acbaaebeacec5225dd3d6d2b";
        var readableAbi = "function activateStreak(address to)";
        var values = new[] { walletAddress };
        Execute("Atia Blessing", () => Waypoint.WriteContract(atiaShrineContractAddress, readableAbi, values));
    }

    private void AuthAsGuest()
    {
        var credential = "credentialk2";
        var authDate = "1727254379";
        var hash = "4e4fe3085e191bd03c54b7a35508682815899bb7c46300290558eb6cb9029f4c";
        var scope = "wallet";
        Execute("Auth as guest", () => Waypoint.AuthAsGuest(credential, authDate, hash, scope));
    }

    private void RegisterGuestAccount()
    {
        Execute("Register guest account", () => Waypoint.RegisterGuestAccount());
    }

    private void CreateKeylessWallet()
    {
        Execute("Create keyless wallet", () => Waypoint.CreateKeylessWallet());
    }
}
