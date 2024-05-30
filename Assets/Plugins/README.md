# Mavis ID Unity SDK

The [Mavis ID](https://id.skymavis.com) lets developers use features such as player authorization, account creation, and in-app wallet interaction in mobile and desktop games.

#### Features:

- Creating a wallet without requiring web3/crypto knowledge or downloading an external app
- Recover & access the wallet on different devices with a simple passphrase
- Perform on-chain actions like minting NFTs, sending transactions & signing messages
- Gas sponsoring

Integration guide: [Integrate Mavis ID into a Unity game](https://docs.skymavis.com/mavis/mavis-id/guides/integrate-unity).

## Installation

1. Download the latest `ID.unitypackage` release from this repository.
2. Import the file by selecting the Unity menu option **Assets > Import package > Custom Package** and importing `ID.unitypackage`.

## Prerequisites

### Mobile

- Register your application with Sky Mavis to get `YOUR_APP_ID` and `YOUR_DEEP_LINK_SCHEMA`
- Request permission to use Mavis ID
- Go to Developer Console > your app > App Permission > Mavis ID > Request Access

[Head to the detail guide](https://docs.skymavis.com/comming-soon) to acquired `YOUR_APP_ID` and `YOUR_DEEP_LINK_SCHEMA`

`Android` : Settings in Unity as described in the [Customize platform-specific settings](https://docs.skymavis.com/mavis/mavis-id/guides/integrate-unity#customize-platform-specific-settings) section in the integration guide.

### Initialization

- Initialize SDK for desktop and mobile

```csharp
    public async void OnInitialized()
    {
#if UNITY_STANDALONE
        string sessionId = GetArg("-sessionId");
        string idPort = GetArg("-hubPort");
        if (!string.IsNullOrEmpty(idPort) && int.TryParse(idPort, out var port))
        {
            SM.MavisId.Init(sessionId, port);
            float startSec = Time.realtimeSinceStartup;
            while (!SM.MavisId.IsConnected && Time.realtimeSinceStartup - startSec <= 15) await Task.Yield();
            if (SM.MavisId.IsConnected)
            {
                Debug.Log("Connected to MH Overlay");
            }
            else
            {
                Debug.LogError("Can't connect to MH Overlay");
            }
        }
#else
        SM.MavisId.Init(AppId, DeeplinkSchema);
#endif
    }
```

### Authorize

- This example demonstrates how to authorize a user when an authorization button is clicked for `Mobile` only.

```csharp
public async void OnAuthorizeClicked()
    {
        _responseId = SM.MavisId.OnAuthorize();
        string responseData = await WaitForMavisIdResponse(_responseId);
        Debug.Log(responseData);
    }
```

The response consists of a deep link with appended parameters that contain the authorization details. Successful authorization provides details such as `method`, `version`, `type` of response, and a JSON Web Token (JWT) containing user data.

## Usage examples

### Sign a message

This example demonstrates how to sign a simple message using the user's MPC wallet.

```csharp
public async void OnPersonalSignClicked()
{
    string message = "Hello world";

    _responseId = SM.MavisId.OnPersonalSign(message);
    string responseData = await WaitForMavisIdResponse(_responseId);
    Debug.Log(responseData);
}
```

The response consists of a deep link with appended parameters that indicate the outcome of the signing request. For successful signings, the response includes a signed message hash.

### Send a transaction

This example demonstrates how to send a transaction to transfer 1 RON to the specified address.

```csharp
public async void OnSendTransactionClicked()
{
    string receiverAddress = "0xD36deD8E1927dCDD76Bfe0CC95a5C1D65c0a807a";
    string value = "100000000000000000";

    _responseId = SM.MavisId.SendTransaction(receiverAddress, value);
    string responseData = await WaitForMavisIdResponse(_responseId);
    Debug.Log(responseData);
}
```

The response consists of a deep link with appended parameters that indicate the outcome of the transaction. Successful transactions provide a transaction hash, which can be verified on the blockchain for additional details.

### Call a contract

This example demonstrates how to send a transaction to approve the specified contract's right to spend 0.1 AXS.

```csharp
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
            _responseId = SM.MavisId.OnCallContract(contractAddress, data);
            string responseData = await WaitForMavisIdResponse(_responseId);
            Debug.Log(responseData);

        }
        catch (System.Exception e)
        {
            Debug.Log("Error in call contract : " + e.Message);
        }

    }
```

The response consists of a deep link with appended parameters that indicate the outcome of the transaction. Successful transactions provide a transaction hash, which can be verified on the blockchain for additional details.

[Head to the playground source code](https://github.com/axieinfinity/mavis-id-unity/blob/main/Assets/Example/ID.cs) for full use-cases

## Documentation

For information on configuration, usage, and response handling, see [Integrate Mavis ID into a Unity game](https://docs.skymavis.com/mavis/mavis-id/guides/integrate-unity).
