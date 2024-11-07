using System;
using System.Threading.Tasks;
using SkyMavis.Waypoint;
using SkyMavis.Waypoint.Utils;
using UnityEngine;

/// <summary>
/// Basic Waypoint example using asynchronous pattern to handle response
/// </summary>
public class WaypointExample : MonoBehaviour
{
    [Header("Setup Standalone")]
    [Tooltip("Set empty string to detect from command line arguments")]
    public string mavisHubSessionId;
    [Tooltip("Set negative number to detect from command line arguments")]
    public int mavisHubPort = -1;

    [Header("Setup Mobile")]
    public string mobileClientId;
    public string mobileDeepLinkSchema;

    private int _step = 1;
    private string _lastResponse;

    private void OnGUI()
    {
        using (new GUILayout.AreaScope(new Rect(20, 20, Screen.width - 40, Screen.height - 40)))
        {
            GUI.enabled = _step == 1;
            GUILayout.Label("Step 1: Configure Waypoint in Waypoint Example GameObject");
            if (GUILayout.Button("Initialize")) Initialize();

            GUI.enabled = _step == 2;
            GUILayout.Label("Step 2: Wait for Waypoint initialization");

            GUI.enabled = _step == 3;
            GUILayout.Label("Step 3: Call Waypoint API");
            if (GUILayout.Button("Authorize")) Authorize();
            if (GUILayout.Button("Personal Sign")) PersonalSign();
            if (GUILayout.Button("Sign Typed Data")) SignTypedData();
            if (GUILayout.Button("Send Transaction")) SendTransaction();
            if (GUILayout.Button("Approve ERC-20")) ApproveErc20();
            if (GUILayout.Button("Swap RON to AXS")) SwapRonToAxs();
            if (GUILayout.Button("Atia Blessing")) AtiaBlessing();
            GUILayout.Label("Last response");
            GUILayout.TextArea(_lastResponse, GUILayout.ExpandHeight(true));
        }
    }

    private async void Initialize()
    {
#if UNITY_STANDALONE
        if (string.IsNullOrEmpty(mavisHubSessionId)) mavisHubSessionId = GetArg("-sessionId");
        if (mavisHubPort < 0 && int.TryParse(GetArg("-hubPort"), out var portArg)) mavisHubPort = portArg;

        if (!string.IsNullOrEmpty(mavisHubSessionId) && mavisHubPort > -1)
        {
            Waypoint.Init(mavisHubSessionId, mavisHubPort);
        }

        static string GetArg(string name)
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
#else
        SkyMavis.Waypoint.Init(mobileClientId, mobileDeepLinkSchema, true);
#endif

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
        Waypoint.BindOnResponse(Callback);
        return tcs.Task;

        void Callback(string state, string data)
        {
            if (state == requestID)
            {
                Waypoint.UnBindOnResponse(Callback);
                tcs.SetResult(data);
            }
        }
    }

    private void Authorize() => Execute("Authorize", Waypoint.OnAuthorize);

    private void PersonalSign() => Execute("Personal Sign", () => Waypoint.OnPersonalSign("Hello Axie Infinity"));

    private void SignTypedData()
    {
        var typedData = @"{""types"":{""Asset"":[{""name"":""erc"",""type"":""uint8""},{""name"":""addr"",""type"":""address""},{""name"":""id"",""type"":""uint256""},{""name"":""quantity"",""type"":""uint256""}],""Order"":[{""name"":""maker"",""type"":""address""},{""name"":""kind"",""type"":""uint8""},{""name"":""assets"",""type"":""Asset[]""},{""name"":""expiredAt"",""type"":""uint256""},{""name"":""paymentToken"",""type"":""address""},{""name"":""startedAt"",""type"":""uint256""},{""name"":""basePrice"",""type"":""uint256""},{""name"":""endedAt"",""type"":""uint256""},{""name"":""endedPrice"",""type"":""uint256""},{""name"":""expectedState"",""type"":""uint256""},{""name"":""nonce"",""type"":""uint256""},{""name"":""marketFeePercentage"",""type"":""uint256""}],""EIP712Domain"":[{""name"":""name"",""type"":""string""},{""name"":""version"",""type"":""string""},{""name"":""chainId"",""type"":""uint256""},{""name"":""verifyingContract"",""type"":""address""}]}, ""domain"":{""name"":""MarketGateway"",""version"":""1"",""chainId"":2021,""verifyingContract"":""0xfff9ce5f71ca6178d3beecedb61e7eff1602950e""},""primaryType"":""Order"",""message"":{""maker"":""0xd761024b4ef3336becd6e802884d0b986c29b35a"",""kind"":""1"",""assets"":[{""erc"":""1"",""addr"":""0x32950db2a7164ae833121501c797d79e7b79d74c"",""id"":""2730069"",""quantity"":""0""}],""expiredAt"":""1721709637"",""paymentToken"":""0xc99a6a985ed2cac1ef41640596c5a5f9f4e19ef5"",""startedAt"":""1705984837"",""basePrice"":""500000000000000000"",""endedAt"":""0"",""endedPrice"":""0"",""expectedState"":""0"",""nonce"":""0"",""marketFeePercentage"":""425""}}";
        Execute("Sign Typed Data", () => Waypoint.OnSignTypeData(typedData));
    }

    private void SendTransaction()
    {
        var receiverAddress = "0xD36deD8E1927dCDD76Bfe0CC95a5C1D65c0a807a";
        var value = "100000000000000000";
        Execute("Send Transaction", () => Waypoint.SendTransaction(receiverAddress, value));
    }

    private void ApproveErc20()
    {
        var contractAddress = "0x3c4e17b9056272ce1b49f6900d8cfd6171a1869d";
        var readableAbi = "function approve(address _spender, uint256 _value)";
        // 1 AXS
        var approveParams = new { _spender = "0x6B190089ed7F75Fe17B3b0A17F6ebd69f72c3F63", _value = 1000000000000000000 };
        var data = ABI.EncodeFunctionData(readableAbi, approveParams);
        Execute("Approve ERC-20", () => Waypoint.OnCallContract(contractAddress, data));
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
        var data = ABI.EncodeFunctionData(readableAbi, swapParams);
        Execute("Swap RON to AXS", () => Waypoint.OnCallContract(katanaAddress, data, value));
    }

    private void AtiaBlessing()
    {
        var walletAddress = "0x6b190089ed7f75fe17b3b0a17f6ebd69f72c3f63";
        var atiaShrineContractAddress = "0xd5c5afefad9ea288acbaaebeacec5225dd3d6d2b";
        var readableAbi = "function activateStreak(address to)";
        var values = new[] { walletAddress };
        var data = ABI.EncodeFunctionData(readableAbi, values);
        Execute("Atia Blessing", () => Waypoint.OnCallContract(atiaShrineContractAddress, data));
    }

    [UnityEditor.MenuItem("MyMenu/Encode Swap abis")]
    static void EncodeSwapAbi()
    {
        string ronAddress = "0xa959726154953bae111746e265e6d754f48570e6";
        string axsAddress = "0x3c4e17b9056272ce1b49f6900d8cfd6171a1869d";
        string walletAddress = "0x6B190089ed7F75Fe17B3b0A17F6ebd69f72c3F63";
        string readableAbi = "function swapExactTokensForTokens(uint256 _amountIn, uint256 _amountOutMin, address[] _path, address _to, uint256 _deadline)";
        var swapParams = new
        {
            // 0.1 Ron
            _amountIn = "1166912850812407680945",
            _amountOutMin = "1580474142",
            _path = new[] { "0xe514d9DEB7966c8BE0ca922de8a064264eA6bcd4", "0x0B7007c13325C48911F73A2daD5FA5dCBf808aDc" },
            _to = "0x1d3286A3348Fa99852d147C57A79045B41c4f713",
            _deadline = "1730877010"
        };

        var data = ABI.EncodeFunctionData(readableAbi, swapParams);
        var correctData = "0x38ed173900000000000000000000000000000000000000000000003f422b3f3e9ff48fb1000000000000000000000000000000000000000000000000000000005e341f1e00000000000000000000000000000000000000000000000000000000000000a00000000000000000000000001d3286a3348fa99852d147c57a79045b41c4f71300000000000000000000000000000000000000000000000000000000672b16520000000000000000000000000000000000000000000000000000000000000002000000000000000000000000e514d9deb7966c8be0ca922de8a064264ea6bcd40000000000000000000000000b7007c13325c48911f73a2dad5fa5dcbf808adc";
        Debug.Log("Encoded Function Data " + data);
        Debug.Log("Correct encode data  " + correctData);
        Debug.Log("Is correct " + (data == correctData));
    }
}
