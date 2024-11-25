# Ronin Waypoint Unity SDK

The Ronin Waypoint Unity SDK lets developers integrate the account and wallet features of the Ronin Waypoint service into Unity games deployed to mobile platforms (Android and iOS) and desktop platforms (Windows and macOS). After the integration, users can sign in to your game with their Ronin Waypoint account and connect their keyless wallet for instant in-game transactions.

On mobile platforms, users interact with Ronin Waypoint through a native WebView. As for desktop platforms, Ronin Waypoint is accessed through an overlay window provided by the Mavis Hub client. Whenever the implementation differs between platforms, this guide specifies the platform-specific steps.

## Features

- Authorize users: let users sign in to your app with Ronin Waypoint to connect their keyless wallet and an optional EOA wallet.
- Send transactions: transfer RON, ERC-20 tokens, and make contract calls for in-game transactions.
- Sign messages and typed data: prove ownership of a wallet or sign structured data.

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
- A redirect URI registered in **Developer Console > Products > Wayoint Service > Redirect URI**.

- To deploy to Android, [Android API level 24](https://developer.android.com/about/versions/nougat) or later.
- To deploy to iOS, [iOS 13.0](https://developer.apple.com/ios/) or later.

## Installation

1. [Install the package using the following Git URL:](https://docs.unity3d.com/Manual/upm-ui-giturl.html) `https://github.com/skymavis/waypoint-unity.git#v0.4.1`
2. Configure [platform-specific settings](https://docs.skymavis.com/mavis/ronin-waypoint/reference/unity-sdk#installation).

## Initialization

### Mobile

```csharp
void Start()
{
    WaypointSettings settings = new WaypointSettings()
    {
        endpoint = "https://waypoint.roninchain.com",
        clientID = "${YOUR_CLIENT_ID}",
        deepLinkCallbackURL = "mydapp://open",
        network = new WaypointSettings.Network()
        {
            rpcURL = "https://saigon-testnet.roninchain.com/rpc",
            chainID = 2021
        }
        // or
        {/* network = WaypointSettings.Network.Testnet */}
        {/* network = WaypointSettings.Network.Mainnet */}
    };
    Waypoint.SetUp(settings)
}
```

### Desktop

```csharp
void Start()
{
    WaypointSettings settings = new WaypointSettings()
    {
        mavisHubSessionID = "${SESSION_ID}",
        mavisHubPort = "${PORT_NUMBER}"
    };
    Waypoint.SetUp(settings);
}
```

## Usage example

### Create a response listener

Ronin Waypoint returns responses to your app through a redirect URI that you registered in the **Developer Console**.

```csharp
using System.Threading.Tasks;

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
```

### Authorize a user

**Note:** Not required for Windows and macOS games distributed through Mavis Hub.

Initializes the authorization process, allowing a user to sign in or sign up for a Ronin Waypoint account, and connect their wallet. Returns an authorization response containing an ID token and the user's keyless wallet address.

```csharp
public async void OnAuthorizeClicked()
{
    _responseId = Waypoint.Authorize();
    string responseData = await WaitForResponse(_responseId);
    Debug.Log("Get ID Token response : " + responseData);
}
```

### Ron transfer

Transfers 0.1 RON to another address, returning a transaction hash.

```csharp
public async void OnSendRonClicked()
{
    string receiverAddress = "0xD36deD8E1927dCDD76Bfe0CC95a5C1D65c0a807a";
    string value = "100000000000000000";

    _responseId = Waypoint.SendNativeToken(receiverAddress, value);
    string responseData = await WaitForResponse(_responseId);
    Debug.Log("Send Ron response data in Unity : " + responseData);
}
```

### Sign a message

Signs a plain text message, returning a signature in hex format.

```csharp
public async void OnPersonalSignClicked()
{
    string message = "Hello Axie Infinity";

    _responseId = Waypoint.PersonalSign(message);
    string responseData = await WaitForResponse(_responseId);
    Debug.Log("Personal sign response: " + responseData);
}
```

### Sign typed data

Signs [EIP-712](https://eips.ethereum.org/EIPS/eip-712) typed data for an order on Axie Marketplace, returning a signature in hex format.

```csharp
public async void OnSignTypedDataClicked()
{

    string typedData = @"{""types"":{""Asset"":[{""name"":""erc"",""type"":""uint8""},{""name"":""addr"",""type"":""address""},{""name"":""id"",""type"":""uint256""},{""name"":""quantity"",""type"":""uint256""}],""Order"":[{""name"":""maker"",""type"":""address""},{""name"":""kind"",""type"":""uint8""},{""name"":""assets"",""type"":""Asset[]""},{""name"":""expiredAt"",""type"":""uint256""},{""name"":""paymentToken"",""type"":""address""},{""name"":""startedAt"",""type"":""uint256""},{""name"":""basePrice"",""type"":""uint256""},{""name"":""endedAt"",""type"":""uint256""},{""name"":""endedPrice"",""type"":""uint256""},{""name"":""expectedState"",""type"":""uint256""},{""name"":""nonce"",""type"":""uint256""},{""name"":""marketFeePercentage"",""type"":""uint256""}],""EIP712Domain"":[{""name"":""name"",""type"":""string""},{""name"":""version"",""type"":""string""},{""name"":""chainId"",""type"":""uint256""},{""name"":""verifyingContract"",""type"":""address""}]}, ""domain"":{""name"":""MarketGateway"",""version"":""1"",""chainId"":2021,""verifyingContract"":""0xfff9ce5f71ca6178d3beecedb61e7eff1602950e""},""primaryType"":""Order"",""message"":{""maker"":""0xd761024b4ef3336becd6e802884d0b986c29b35a"",""kind"":""1"",""assets"":[{""erc"":""1"",""addr"":""0x32950db2a7164ae833121501c797d79e7b79d74c"",""id"":""2730069"",""quantity"":""0""}],""expiredAt"":""1721709637"",""paymentToken"":""0xc99a6a985ed2cac1ef41640596c5a5f9f4e19ef5"",""startedAt"":""1705984837"",""basePrice"":""500000000000000000"",""endedAt"":""0"",""endedPrice"":""0"",""expectedState"":""0"",""nonce"":""0"",""marketFeePercentage"":""425""}}";

    _responseId = Waypoint.SignTypedData(typedData);
    string responseData = await WaitForResponse(_responseId);
    Debug.Log("Sign typed data response " + responseData);
}
```

### Contract function calls

Allows another contract to spend 1 AXS on user's behalf, returning a transaction hash.

```csharp
public async void OnApproveErc20Clicked()
{
    // Contract address
    string contractAddress = "0x3c4e17b9056272ce1b49f6900d8cfd6171a1869d";
    // Readable ABI string for the function
    string readableAbi = "function approve(address _spender, uint256 _value)";
    // Approve 1 AXS
    var approveParams = new { _spender = "0x6B190089ed7F75Fe17B3b0A17F6ebd69f72c3F63", _value = 1000000000000000000 };
    try
    {
        _responseId = Waypoint.WriteContract(contractAddress, readableAbi, approveParams);
        string responseData = await WaitForResponse(_responseId);
        Debug.Log(responseData);
    }
    catch (System.Exception e)
    {
        Debug.Log("Error in call contract: " + e.Message);
    }

}
```

## Documentation

- For more information, see the [Ronin Waypoint Unity SDK](https://docs.skymavis.com/mavis/ronin-waypoint/reference/unity-sdk) integration guide.
