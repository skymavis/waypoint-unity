using System;
using System.Threading.Tasks;
using SkyMavis.Utils;
using TMPro;
using UnityEngine;

public class WaypointExample : MonoBehaviour
{
    // ClientId and DeeplinkSchema are registered with Sky Mavis
    static readonly string ClientId = "${YOUR_CLIENT_ID}";
    static readonly string DeeplinkSchema = "${YOUR_DEEPLINK_SCHEMA}";


    public GameObject popupPanel;
    public TMP_Text text;
    public string _responseId;

    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }

    public async Task<string> WaitForWaypointResponse(string id)
    {
        string responseData = null;
        string currentId = id;
        void dataCallback(string state, string data) { if (currentId == state) responseData = data; }
        SkyMavis.Waypoint.BindOnResponse(dataCallback);
        while (string.IsNullOrEmpty(responseData) && currentId == _responseId) await Task.Yield();
        SkyMavis.Waypoint.UnBindOnResponse(dataCallback);
        return responseData;
    }

    // Start is called before the first frame update
    void Start()
    {
        OnInitialized();
    }

    // Method to be called by the close button to hide the popup
    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }

    // Example method to show the popup
    public void ShowPopup(string message)
    {
        text.text = message;
        popupPanel.SetActive(true);
    }


    public async void OnAuthorizeClicked()
    {
        _responseId = SkyMavis.Waypoint.OnAuthorize();
        string responseData = await WaitForWaypointResponse(_responseId);
        Debug.Log("Authorize response : " + responseData);
    }

    public async void OnPersonalSignClicked()
    {
        string message = "Hello Axie Infinity";

        _responseId = SkyMavis.Waypoint.OnPersonalSign(message);
        string responseData = await WaitForWaypointResponse(_responseId);
        Debug.Log("Personal sign response: " + responseData);
    }

    public async void OnSignTypedDataClicked()
    {

        string typedData = @"{""types"":{""Asset"":[{""name"":""erc"",""type"":""uint8""},{""name"":""addr"",""type"":""address""},{""name"":""id"",""type"":""uint256""},{""name"":""quantity"",""type"":""uint256""}],""Order"":[{""name"":""maker"",""type"":""address""},{""name"":""kind"",""type"":""uint8""},{""name"":""assets"",""type"":""Asset[]""},{""name"":""expiredAt"",""type"":""uint256""},{""name"":""paymentToken"",""type"":""address""},{""name"":""startedAt"",""type"":""uint256""},{""name"":""basePrice"",""type"":""uint256""},{""name"":""endedAt"",""type"":""uint256""},{""name"":""endedPrice"",""type"":""uint256""},{""name"":""expectedState"",""type"":""uint256""},{""name"":""nonce"",""type"":""uint256""},{""name"":""marketFeePercentage"",""type"":""uint256""}],""EIP712Domain"":[{""name"":""name"",""type"":""string""},{""name"":""version"",""type"":""string""},{""name"":""chainId"",""type"":""uint256""},{""name"":""verifyingContract"",""type"":""address""}]}, ""domain"":{""name"":""MarketGateway"",""version"":""1"",""chainId"":2021,""verifyingContract"":""0xfff9ce5f71ca6178d3beecedb61e7eff1602950e""},""primaryType"":""Order"",""message"":{""maker"":""0xd761024b4ef3336becd6e802884d0b986c29b35a"",""kind"":""1"",""assets"":[{""erc"":""1"",""addr"":""0x32950db2a7164ae833121501c797d79e7b79d74c"",""id"":""2730069"",""quantity"":""0""}],""expiredAt"":""1721709637"",""paymentToken"":""0xc99a6a985ed2cac1ef41640596c5a5f9f4e19ef5"",""startedAt"":""1705984837"",""basePrice"":""500000000000000000"",""endedAt"":""0"",""endedPrice"":""0"",""expectedState"":""0"",""nonce"":""0"",""marketFeePercentage"":""425""}}";

        _responseId = SkyMavis.Waypoint.OnSignTypeData(typedData);
        string responseData = await WaitForWaypointResponse(_responseId);
        Debug.Log("Sign typed data response " + responseData);
    }
    public async void OnSendTransactionClicked()
    {
        string receiverAddress = "0xD36deD8E1927dCDD76Bfe0CC95a5C1D65c0a807a";
        string value = "100000000000000000";

        _responseId = SkyMavis.Waypoint.SendTransaction(receiverAddress, value);
        string responseData = await WaitForWaypointResponse(_responseId);
        Debug.Log("Send response data in Unity : " + responseData);
    }

    public async void OnApproveErc20Clicked()
    {
        string contractAddress = "0x3c4e17b9056272ce1b49f6900d8cfd6171a1869d";

        string readableAbi = "function approve(address _spender, uint256 _value)";
        // Approve 1 AXS
        var approveParams = new { _spender = "0x6B190089ed7F75Fe17B3b0A17F6ebd69f72c3F63", _value = 1000000000000000000 };
        try
        {
            var data = ABI.EncodeFunctionData(readableAbi, approveParams);
            Debug.Log("Approve data : " + data);
            _responseId = SkyMavis.Waypoint.OnCallContract(contractAddress, data);
            string responseData = await WaitForWaypointResponse(_responseId);
            Debug.Log("Approve AXS response data in Unity : " + responseData);

        }
        catch (System.Exception e)
        {
            Debug.Log("Error in call contract : " + e.Message);
        }

    }


    public async void OnSwapRonToAxs()
    {
        string katanaAddress = "0xDa44546C0715ae78D454fE8B84f0235081584Fe0";
        string ronAddress = "0xa959726154953bae111746e265e6d754f48570e6";
        string axsAddress = "0x3c4e17b9056272ce1b49f6900d8cfd6171a1869d";
        string walletAddress = "0x6B190089ed7F75Fe17B3b0A17F6ebd69f72c3F63";
        string readableAbi = "function swapExactRONForTokens(uint256 _amountOutMin, address[] _path, address _to, uint256 _deadline) payable";

        // 0.1 Ron
        string value = "100000000000000000";

        object swapParams = new
        {
            _amountOutMin = 0,
            _path = new string[] { ronAddress, axsAddress },
            _to = walletAddress,
            _deadline = 19140313050
        };

        try
        {
            string data = ABI.EncodeFunctionData(readableAbi, swapParams);
            _responseId = SkyMavis.Waypoint.OnCallContract(katanaAddress, data, value);
            var responseData = await WaitForWaypointResponse(_responseId);
            Debug.Log("Swap response data in Unity : " + responseData);
        }
        catch (System.Exception e)
        {
            Debug.Log("Error in call contract : " + e.Message);
        }

    }

    public async void onClickAtiaBlessing()
    {

        string walletAddress = "0x6b190089ed7f75fe17b3b0a17f6ebd69f72c3f63";
        string atiaShrineContractAddress = "0xd5c5afefad9ea288acbaaebeacec5225dd3d6d2b";

        string readableAbi = "function activateStreak(address to)";
        Debug.Log(readableAbi);
        var values = new string[] { walletAddress };

        try
        {
            var data = ABI.EncodeFunctionData(readableAbi, values);
            _responseId = SkyMavis.Waypoint.OnCallContract(atiaShrineContractAddress, data);
            string responseData = await WaitForWaypointResponse(_responseId);
            Debug.Log("Atia blessing response data in Unity " + responseData);

        }
        catch (System.Exception e)
        {
            Debug.Log("Error in Atita's blessing : " + e.Message);
        }

    }

    public async void onClickAuthAsGuest()
    {

        string credential = "{YOUR_CREDENTIAL}";
        string authDate = "{YOUR_AUTH_DATE}";
        string hash = "{YOUR_HASH}";
        string scope = "{YOUR_SCOPE}";
        try
        {
            _responseId = SkyMavis.Waypoint.OnAuthAsGuest(credential, authDate, hash, scope);
            string responseData = await WaitForWaypointResponse(_responseId);
            Debug.Log("Auth as guest response data in Unity " + responseData);

        }
        catch (System.Exception e)
        {
            Debug.Log("Error in Auth as guest: " + e.Message);
        }

    }

    public async void OnRegisterGuestAccount()
    {
        try
        {
            _responseId = SkyMavis.Waypoint.OnRegisterGuestAccount();
            Debug.Log("Response ID register : " + _responseId);
            string responseData = await WaitForWaypointResponse(_responseId);
            Debug.Log("Register guest account response data in Unity " + responseData);

        }
        catch (System.Exception e)
        {
            Debug.Log("Error in register guest account : " + e.Message);
        }

    }

    public async void OnInitialized()
    {
#if UNITY_STANDALONE
        string sessionId = GetArg("-sessionId");
        string hubPort = GetArg("-hubPort");
        if (!string.IsNullOrEmpty(hubPort) && int.TryParse(hubPort, out var port))
        {
            SkyMavis.Waypoint.Init(sessionId, port);
            float startSec = Time.realtimeSinceStartup;
            while (!SkyMavis.Waypoint.IsConnected && Time.realtimeSinceStartup - startSec <= 15) await Task.Yield();
            if (SkyMavis.Waypoint.IsConnected)
            {
                Debug.Log("Connected to MH Overlay");
            }
            else
            {
                Debug.LogError("Can't connect to MH Overlay");
            }
        }
#else

        string rpcUrl = "https://saigon-testnet.roninchain.com/rpc";
        int chainId = 2021;
        SkyMavis.Waypoint.Init(ClientId, DeeplinkSchema, rpcUrl, chainId);
#endif
    }

    public void OnGetIDTokenClicked()
    {
        SkyMavis.Waypoint.OnGetIDToken();
    }

#if UNITY_EDITOR
    static string[] SAMPLE_HUMANREABLE_ABIS = new string[] {
        "function abc(address maker, uint256 kind) nonpayable",
        "function abc(address[] maker, uint256 kind) nonpayable",
        "function abc(tuple(address maker, uint256 kind) order) nonpayable",
        "function abc(tuple(address maker, uint256 kind)[] order) nonpayable",
        "function settleOrder(tuple(address maker, uint256 kind)[] _order) nonpayable",
        "function order((address maker, uint8 kind, (uint8 erc, address addr, uint256 id, uint256 quantity)[] assets, uint256 expiredAt, address paymentToken, uint256 startedAt, uint256 basePrice, uint256 endedAt, uint256 endedPrice, uint256 expectedState, uint256 nonce, uint256 marketFeePercentage)) payable"
    };

    [UnityEditor.MenuItem("MyMenu/Flattern")]
    static void EncodeFlattern()
    {

        Debug.Log("Flattern");
        var orders = new
        {
            maker = "0x70275A9B6828F83cF710eE735F3E02f973353FF7",
            kind = 1000
        };
        Debug.Log(ABI.ParseAbi(SAMPLE_HUMANREABLE_ABIS[0]).ToString());
        var data = ABI.EncodeFunctionData(SAMPLE_HUMANREABLE_ABIS[0], orders);
        var expectedResult = "0xbdc5ea0b00000000000000000000000070275a9b6828f83cf710ee735f3e02f973353ff700000000000000000000000000000000000000000000000000000000000003e8";
        Debug.Log("Data : " + data);
        Debug.Log($"Is same with expected result : {data == expectedResult}");
    }

    [UnityEditor.MenuItem("MyMenu/Array")]
    static void EncodeArray()
    {

        Debug.Log("Array");
        var orders = new
        {
            maker = new string[] { "0x70275A9B6828F83cF710eE735F3E02f973353FF7" },
            kind = 1000
        };

        var data = ABI.EncodeFunctionData(SAMPLE_HUMANREABLE_ABIS[1], orders);
        Debug.Log("Data : " + data);
        string expectedResult = "0x16d08d95000000000000000000000000000000000000000000000000000000000000004000000000000000000000000000000000000000000000000000000000000003e8000000000000000000000000000000000000000000000000000000000000000100000000000000000000000070275a9b6828f83cf710ee735f3e02f973353ff7";
        Debug.Log($"Is same with expected result : {data == expectedResult}");

    }

    [UnityEditor.MenuItem("MyMenu/Tuple")]
    static void EncodeTuple()
    {

        Debug.Log("Encode Tupple");
        var orders = new
        {
            order = new
            {
                maker = "0x70275A9B6828F83cF710eE735F3E02f973353FF7",
                kind = 1000
            }
        };


        var data = ABI.EncodeFunctionData(SAMPLE_HUMANREABLE_ABIS[2], orders);
        string expectedResult = "0x3c26d85800000000000000000000000070275a9b6828f83cf710ee735f3e02f973353ff700000000000000000000000000000000000000000000000000000000000003e8";
        Debug.Log("Data : " + data);
        Debug.Log($"Is same with expected result : {data == expectedResult}");
    }

    [UnityEditor.MenuItem("MyMenu/Tupple array")]
    static void EncodeTuppleArray()
    {

        Debug.Log("Encode tuple array...");
        var orders = new
        {
            order = new object[] {
                new {
                    maker = "0x70275A9B6828F83cF710eE735F3E02f973353FF7",
                    kind = 1000
                }
            }
        };

        var data = ABI.EncodeFunctionData(SAMPLE_HUMANREABLE_ABIS[3], orders);
        string expectedResult = "0x4b3ed09e0000000000000000000000000000000000000000000000000000000000000020000000000000000000000000000000000000000000000000000000000000000100000000000000000000000070275a9b6828f83cf710ee735f3e02f973353ff700000000000000000000000000000000000000000000000000000000000003e8";
        Debug.Log("Data : " + data);
        Debug.Log($"Is same with expected result : {data == expectedResult}");
    }
    [UnityEditor.MenuItem("MyMenu/Encode Swap abis")]
    static void EncodeSwapAbi()
    {
        string ronAddress = "0xa959726154953bae111746e265e6d754f48570e6";
        string axsAddress = "0x3c4e17b9056272ce1b49f6900d8cfd6171a1869d";
        string walletAddress = "0x6B190089ed7F75Fe17B3b0A17F6ebd69f72c3F63";
        string readableAbi = "function swapExactRONForTokens(uint256 _amountOutMin, address[] _path, address _to, uint256 _deadline) payable";
        var swapParams = new
        {
            // 0.1 Ron
            _amountOutMin = 0,
            _path = new string[] { ronAddress, axsAddress },
            _to = walletAddress,
            _deadline = 19140313050
        };

        try
        {
            var data = ABI.EncodeFunctionData(readableAbi, swapParams);
            Debug.Log("Encoded Function Data " + data);
        }
        catch (System.Exception e)
        {
            Debug.Log("Error in call contract : " + e.Message);
        }

    }

    [UnityEditor.MenuItem("MyMenu/ParseABI")]
    static void ParseABI()
    {

        Debug.Log("Parse ABI...");
        foreach (var abiItem in SAMPLE_HUMANREABLE_ABIS)
        {
            var abi = ABI.ParseAbi(abiItem);
            Debug.Log(abi);
        }

    }
    static Skynet InitializeSkynet()
    {
        var SkynetAPIKey = "";
        var ownerAddress = "0x2d62c27ce2e9e66bb8a667ce1b60f7cb02fa9810";
        var NftContractAddresses = new string[] { Constants.Mainnet.ERC721.AxieContractAddress };
        var ERC20ContractAddresses = new string[] { Constants.Mainnet.ERC20.AXSContractAddress, Constants.Mainnet.ERC20.SLPContractAddress, Constants.Mainnet.ERC20.WRONContractAddress, Constants.Mainnet.ERC20.WETHContractAddress };
        Debug.Log("AXS Contract Address : " + Constants.Mainnet.ERC20.AXSContractAddress);
        Debug.Log("Katana Swap Contract Address : " + Constants.Mainnet.KatanaAddress);
        Debug.Log("Ronnin RPC Url : " + Constants.Mainnet.RPCUrl);
        return new Skynet(SkynetAPIKey, ownerAddress, NftContractAddresses, ERC20ContractAddresses);

    }
    [UnityEditor.MenuItem("MyMenu/Get total NFTs")]
    static async void GetTotalNFTs()
    {
        var skynet = InitializeSkynet();
        var response = await skynet.GetTotalNFTs();

        Debug.Log("Result get total NFTs of address : " + response);
    }

    [UnityEditor.MenuItem("MyMenu/Get NFTs Metadata Skynet")]
    static async void GetNFTsMetadata()
    {

        var skynet = InitializeSkynet();
        var tokenIds = new string[] { "11967144" };
        var response = await skynet.GetNFTsMetadata(tokenIds);
        Debug.Log("Result Get NFTs Metadata with tokenIds : " + response);
    }

    [UnityEditor.MenuItem("MyMenu/Get Total Balances")]
    static async void GetTotalBalances()
    {

        var skynet = InitializeSkynet();
        var response = await skynet.GetERC20TokenBalances();

        Debug.Log("Result get total token balances : " + response);
    }
#endif
}
