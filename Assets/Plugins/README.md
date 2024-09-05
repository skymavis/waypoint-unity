# Waypoint Unity SDK

The Waypoint Unity SDK lets developers integrate Waypoint into Unity games deployed to mobile platforms (Android and iOS) and desktop platforms (Windows and macOS). After the integration, users can sign in to your game with Waypoint and set up a Web3 wallet to interact with the blockchain to send and receive tokens, sign messages, and more.

Mobile games use the Waypoint SDK to interact with the Waypoint service through a native WebView. Desktop games distributed through Mavis Hub use the Waypoint SDK to interact with the Waypoint service through the Mavis Hub client.

## Features

- Authorize users: sign in to your app with Waypoint.
- Send transactions: transfer tokens to other addresses.
- Sign messages: sign plain text messages.
- Sign typed data: sign structured data according to the EIP-712 standard.
- Call contracts: execute custom transactions on smart contracts.

## Prerequisites

General requirements:

- [Unity v2020.3.48f1 or later](https://unity.com/download)

Desktop requirements:

- A game distributed through [Mavis Hub](https://hub.skymavis.com).
- [.Net v3.1](https://dotnet.microsoft.com/en-us/download/dotnet/3.1) or later.

Mobile requirements:

- An app created in the [Developer Console](https://developers.skymavis.com/console/applications/).
- Permission to use Waypoint. Request in **Developer Console > your app > App Permission > Sky Mavis Account (OAuth 2.0) > Request Access**.
- A client ID that you can find in **Developer Console > Products > Waypoint Service > Client ID**.
- A redirect URI registered in **Developer Console > Products > ID Service > Redirect URI**.

- To deploy to Android, [Android API level 24](https://developer.android.com/about/versions/nougat) or later.
- To deploy to iOS, [iOS 13.0](https://developer.apple.com/ios/) or later.

## Installation

1. Download the latest `Waypoint.unitypackage` release from this repository.
2. Import the file by selecting the Unity menu option **Assets > Import package > Custom Package** and importing `Waypoint.unitypackage`.
3. In Unity Editor, go to **Build Settings**, then choose the platform you are deploying, then click **Switch Platform**.
4. Configure [platform-specific settings](https://docs.skymavis.com/mavis/mavis-id/guides/unity-sdk#step-2-install-the-sdk).

## Initialization

### Mobile

```csharp
void Start()
{
    // Client ID registered in the Waypoint settings in the Developer Console
    string clientId = "${YOUR_CLIENT_ID}";
    // Redirect URI registered in the Waypoint settings in the Developer Console
    string deeplinkSchema = "${YOUR_DEEPLINK_SCHEMA}";
    // Initializion on the Ronin mainnet
    SkyMavis.Waypoint.Init(ClientId, DeeplinkSchema);
}
```

### Desktop

```csharp
void Start()
{
    // Session ID and port number provided by Mavis Hub
    string sessionId = "${SESSION_ID}";
    int port = ${PORT_NUMBER};
    // Initialize the Waypoint Unity SDK
    SkyMavis.Waypoint.Init(sessionId, port);
}
```

## Usage

### Authorize users

**Note:** Not required for Windows and macOS games distributed through Mavis Hub.

Authorizes a user with an existing Waypoint account, returning an ID token and the user's wallet address. If the user does not have an account, they will be prompted to create one.

```csharp
public async void OnAuthorizeClicked()
{
    _responseId = SkyMavis.Waypoint.OnAuthorize();
    string responseData = await WaitForMavisIdResponse(_responseId);
    Debug.Log(responseData);
}
```

### Send transactions

Transfers 0.1 RON to another address, returning a transaction hash.

```csharp
public async void OnSendTransactionClicked()
{
    // Recipient address
    string receiverAddress = "0xD36deD8E1927dCDD76Bfe0CC95a5C1D65c0a807a";
    // Value in wei (1 RON = 10^18 wei)
    string value = "100000000000000000";

    _responseId = SkyMavis.Waypoint.SendTransaction(receiverAddress, value);
    string responseData = await WaitForMavisIdResponse(_responseId);
    Debug.Log(responseData);
}
```

### Sign messages

Signs a plain text message, returning a signature in hex format.

```csharp
public async void OnPersonalSignClicked()
{
    // Message to sign
    string message = "I accept the terms and conditions.";

    _responseId = SkyMavis.Waypoint.OnPersonalSign(message);
    string responseData = await WaitForMavisIdResponse(_responseId);
    Debug.Log(responseData);
}
```

### Sign typed data

Signs [EIP-712](https://eips.ethereum.org/EIPS/eip-712) typed data for an order on Axie Marketplace, returning a signature in hex format.

```csharp
public void OnSignTypedDataClicked()
// JSON struct that specifies the EIP-712 typed structured data
{
    string typedData = @"{""types"":{""Asset"":[{""name"":""erc"",""type"":""uint8""},{""name"":""addr"",""type"":""address""},{""name"":""id"",""type"":""uint256""},{""name"":""quantity"",""type"":""uint256""}],""Order"":[{""name"":""maker"",""type"":""address""},{""name"":""kind"",""type"":""uint8""},{""name"":""assets"",""type"":""Asset[]""},{""name"":""expiredAt"",""type"":""uint256""},{""name"":""paymentToken"",""type"":""address""},{""name"":""startedAt"",""type"":""uint256""},{""name"":""basePrice"",""type"":""uint256""},{""name"":""endedAt"",""type"":""uint256""},{""name"":""endedPrice"",""type"":""uint256""},{""name"":""expectedState"",""type"":""uint256""},{""name"":""nonce"",""type"":""uint256""},{""name"":""marketFeePercentage"",""type"":""uint256""}],""EIP712Domain"":[{""name"":""name"",""type"":""string""},{""name"":""version"",""type"":""string""},{""name"":""chainId"",""type"":""uint256""},{""name"":""verifyingContract"",""type"":""address""}]}, ""domain"":{""name"":""MarketGateway"",""version"":""1"",""chainId"":2021,""verifyingContract"":""0xfff9ce5f71ca6178d3beecedb61e7eff1602950e""},""primaryType"":""Order"",""message"":{""maker"":""0xd761024b4ef3336becd6e802884d0b986c29b35a"",""kind"":""1"",""assets"":[{""erc"":""1"",""addr"":""0x32950db2a7164ae833121501c797d79e7b79d74c"",""id"":""2730069"",""quantity"":""0""}],""expiredAt"":""1721709637"",""paymentToken"":""0xc99a6a985ed2cac1ef41640596c5a5f9f4e19ef5"",""startedAt"":""1705984837"",""basePrice"":""500000000000000000"",""endedAt"":""0"",""endedPrice"":""0"",""expectedState"":""0"",""nonce"":""0"",""marketFeePercentage"":""425""}}";
    _responseId = SkyMavis.Waypoint.OnSignTypedData(typedData);
    string responseData = await WaitForMavisIdResponse(_responseId);
    Debug.Log(responseData);
}
```

### Call contracts

Allows another contract to spend 1 AXS on user's behalf, returning a transaction hash.

```csharp
public async void OnApproveErc20Clicked()
{
    // Address of the smart contract to interact with
    string contractAddress = "0x3c4e17b9056272ce1b49f6900d8cfd6171a1869d";

    string readableAbi = "function approve(address _spender, uint256 _value)";
    // Approve 1 AXS
    var approveParams = new { _spender = "0x6B190089ed7F75Fe17B3b0A17F6ebd69f72c3F63", _value = 1000000000000000000 };
    try
    {
        // Transaction data to be sent to the smart contract
        var data = ABI.EncodeFunctionData(readableAbi, approveParams);
        Debug.Log("Approve data: " + data);
        _responseId = SkyMavis.Waypoint.OnCallContract(contractAddress, data);
        string responseData = await WaitForMavisIdResponse(_responseId);
        Debug.Log(responseData);

    }
    catch (System.Exception e)
    {
        Debug.Log("Error in call contract: " + e.Message);
    }
}
```

## Utilities

### Encode function data

Swaps tokens on the [Katana](https://app.roninchain.com/swap) decentralized exchange:

```csharp
using SkyMavis.Utils;

public async void OnSwapRonToAxsClicked()
{
    // Contract addresses
    string katanaAddress = "0xDa44546C0715ae78D454fE8B84f0235081584Fe0";
    string ronAddress = "0xa959726154953bae111746e265e6d754f48570e6";
    string axsAddress = "0x3c4e17b9056272ce1b49f6900d8cfd6171a1869d";
    // Wallet address
    string walletAddress = "0x6B190089ed7F75Fe17B3b0A17F6ebd69f72c3F63";
    // Readable ABI string for the function
    string readableAbi = "function swapExactRONForTokens(uint256 _amountOutMin, address[] _path, address _to, uint256 _deadline)";

    // Values to pass as parameters to the called function
    var value = "1000000000000000000";

    var swapParams = new
    {
        // 0.1 RON
        _amountOutMin = "0",
        _path = new string[] { ronAddress, axsAddress },
        _to = walletAddress,
        _deadline = "1814031305"
    };

    try
    {
        // Encode the function data using the ABI utility in SkyMavis.Utils
        var data = ABI.EncodeFunctionData(readableAbi, swapParams);
        // Send the transaction to the Katana contract using SkyMavis.Waypoint namespace
        _responseId = SkyMavis.Waypoint.OnCallContract(katanaAddress, data, value);
        // Wait for the response
        string responseData = await WaitForMavisIdResponse(_responseId);
        Debug.Log(responseData);
    }
    catch (System.Exception e)
    {
        Debug.Log("Error in call contract: " + e.Message);
    }
}
```

### Read smart contract data

Fetches the total NFTs of the specified address using the [Skynet REST API](https://docs.skymavis.com/api/ronin-rest/skynet-rest-api):

```csharp
var skynet = new Skynet(SKYNET_API_KEY, RONIN_WALLET_ADDRESS, new string[] { Constants.Mainnet.ERC721.AxieContractAddress }, new string[] { Constants.Mainnet.ERC20.AXSContractAddress });
var response = await skynet.GetTotalNFTs();
Debug.Log("Total NFTs: " + response);
```

### Make RPC calls

Checks how many AXS tokens the user has allowed the Katana contract to spend, using the [Skynet REST API](https://docs.skymavis.com/api/ronin-rest/skynet-rest-api):

```csharp
using SkyMavis.Utils;

// Initialize Skynet
var skynet = InitializeSkynet();
// ABI for the allowance function
var allowanceOfABI = @"{""constant"":true,""inputs"":[{""internalType"":""address"",""name"":""_owner"",""type"":""address""},{""internalType"":""address"",""name"":""_spender"",""type"":""address""}],""name"":""allowance"",""outputs"":[{""internalType"":""uint256"",""name"":""_value"",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""}";
// Assign values for the ABI parameters
var args = new object[] { "0x2d62c27ce2e9e66bb8a667ce1b60f7cb02fa9810", Constants.Mainnet.KatanaAddress };
// Encode the function data using the EncodeFunctionData utility in SkyMavis.Utils
var data = ABI.EncodeFunctionData(allowanceOfABI, args);
// Execute the RPC call to check how many AXS tokens the user has allowed the Katana contract to spend
var result = await skynet.CallRPC(Constants.Mainnet.ERC20.AXSContractAddress, data);
// Process the result: remove the "0x" prefix, parse the hex string to a BigInteger, and format the value by dividing by 10^18 to get the Ether balance
result = result.StartsWith("0x") ? result.Substring(2) : result;
BigInteger weiValue = BigInteger.Parse(result, System.Globalization.NumberStyles.HexNumber);
BigInteger divisor = BigInteger.Pow(10, 18);
decimal formatedValue = (decimal)weiValue / (decimal)divisor;

Debug.Log("Formatted Ether balance: " + formatedValue);
```

## Documentation

- For more information, see the [Waypoint Unity SDK](https://docs.skymavis.com/mavis/mavis-id/guides/unity-sdk) integration guide.
- For detailed examples, see the [playground source code](https://github.com/axieinfinity/mavis-id-unity/blob/main/Assets/Example/ID.cs).
