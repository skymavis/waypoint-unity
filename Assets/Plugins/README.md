# Mavis ID Unity SDK

The Mavis ID Unity SDK lets developers integrate Mavis ID into Unity games for Android, iOS, and desktop platforms (games distributed through Mavis Hub). After the integration, users can sign in to your game with Mavis ID and set up a Web3 wallet to interact with the blockchain to send and receive tokens, sign messages, and more. 

Players on mobile interact with Mavis ID through a WebView within the game environment. Desktop users playing games through Mavis Hub see an overlay within the Mavis Hub client.

## Features

* User authorization: sign in to your app with Mavis ID.
* Transaction sending: transfer tokens to other addresses.
* Message signing: sign plain text messages.
* Typed data signing: sign structured data according to the EIP-712 standard.
* Contract calls: execute custom transactions on smart contracts.

## Prerequisites

General requirements:

* [Unity v2020.3.48f1 or later](https://unity.com/download)

Desktop requirements:

* A game distributed through [Mavis Hub](https://hub.skymavis.com).
* [.Net v3.1](https://dotnet.microsoft.com/en-us/download/dotnet/3.1) or later.

Mobile requirements:

* An app created in the [Developer Console](https://developers.skymavis.com/console/applications/).
* Permission to use Mavis ID. Request in **Developer Console > your app > App Permission > Sky Mavis Account (OAuth 2.0) > Request Access**.
* A client ID that you can find in **Developer Console > Products > ID Service > Client ID**.
* A redirect URI registered in **Developer Console > Products > ID Service > Redirect URI**.

* To deploy to Android, [Android API level 24](https://developer.android.com/about/versions/nougat) or later.
* To deploy to iOS, [iOS 12.0](https://developer.apple.com/ios/) or later.

## Installation

1. Download the latest `ID.unitypackage` release from this repository.
2. Import the file by selecting the Unity menu option **Assets > Import package > Custom Package** and importing `ID.unitypackage`.
3. In Unity Editor, go to **Build Settings**, then choose the platform you are deploying, then click **Switch Platform**.

After that, configure [platform-specific settings](https://docs.skymavis.com/mavis/mavis-id/guides/unity-sdk#step-2-install-the-sdk).

## Initialization

### Mobile

```csharp
void Start()
{
    string appId = "mydapp";
    // Use the redirect URI registered in app's settings in the Developer Console
    string deeplinkSchema = "mydapp";
    // Initialize the Mavis ID Unity SDK
    SM.MavisId.Init(AppId, deeplinkSchema, isTestnet: false);
}
```

### Desktop

```csharp
void Start()
{
    // Use the session ID and port number provided by Mavis Hub
    string sessionId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
    int port = 4001;
    // Initialize the Mavis ID Unity SDK
    SM.MavisId.Init(sessionId, port);
}
```

## Usage

### Authorize users

#### Mobile

Authorizes a user with an existing Mavis ID account, returning an ID token and the user's wallet address. If the user does not have an account, they will be prompted to create one.

```csharp
public async void OnAuthorizeClicked()
{
    _responseId = SM.MavisId.OnAuthorize();
    string responseData = await WaitForMavisIdResponse(_responseId);
    Debug.Log(responseData);
}
```

#### Desktop



### Send transactions

Transfers RON tokens to a recipient's address, returning a transaction hash.

```csharp
string SendTransaction(string receiverAddress, string value)
```

Parameters:

* `receiverAddress` (string): the recipient's address.
* `value` (string): the amount of RON to send, specified in wei (1 RON = 10^18 wei).

Example:

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

### Sign messages

Signs a plain text message, returning a signature in hex format.

Parameters:

* `message` (string): the message to sign.

Example:

```csharp
public async void OnPersonalSignClicked()
{
    string message = "I accept the terms and conditions.";

    _responseId = SM.MavisId.OnPersonalSign(message);
    string responseData = await WaitForMavisIdResponse(_responseId);
    Debug.Log(responseData);
}
```

### Sign typed data

Signs typed data structured according to the [EIP-712](https://eips.ethereum.org/EIPS/eip-712) standard, returning a signature in hex format.

```csharp
string OnSignTypeData(string typedData)
```

Parameters:

* `typedData` (string): a JSON string that specifies the EIP-712 typed structured data to be signed by the user. This includes type definitions and the values of the data itself.
* `data` (string): a raw hexadecimal string representing the signature.

Example:

```csharp
public void OnSignTypedDataClicked()
{
    string typedData = @"{""types"":{""Asset"":[{""name"":""erc"",""type"":""uint8""},{""name"":""addr"",""type"":""address""},{""name"":""id"",""type"":""uint256""},{""name"":""quantity"",""type"":""uint256""}],""Order"":[{""name"":""maker"",""type"":""address""},{""name"":""kind"",""type"":""uint8""},{""name"":""assets"",""type"":""Asset[]""},{""name"":""expiredAt"",""type"":""uint256""},{""name"":""paymentToken"",""type"":""address""},{""name"":""startedAt"",""type"":""uint256""},{""name"":""basePrice"",""type"":""uint256""},{""name"":""endedAt"",""type"":""uint256""},{""name"":""endedPrice"",""type"":""uint256""},{""name"":""expectedState"",""type"":""uint256""},{""name"":""nonce"",""type"":""uint256""},{""name"":""marketFeePercentage"",""type"":""uint256""}],""EIP712Domain"":[{""name"":""name"",""type"":""string""},{""name"":""version"",""type"":""string""},{""name"":""chainId"",""type"":""uint256""},{""name"":""verifyingContract"",""type"":""address""}]}, ""domain"":{""name"":""MarketGateway"",""version"":""1"",""chainId"":2021,""verifyingContract"":""0xfff9ce5f71ca6178d3beecedb61e7eff1602950e""},""primaryType"":""Order"",""message"":{""maker"":""0xd761024b4ef3336becd6e802884d0b986c29b35a"",""kind"":""1"",""assets"":[{""erc"":""1"",""addr"":""0x32950db2a7164ae833121501c797d79e7b79d74c"",""id"":""2730069"",""quantity"":""0""}],""expiredAt"":""1721709637"",""paymentToken"":""0xc99a6a985ed2cac1ef41640596c5a5f9f4e19ef5"",""startedAt"":""1705984837"",""basePrice"":""500000000000000000"",""endedAt"":""0"",""endedPrice"":""0"",""expectedState"":""0"",""nonce"":""0"",""marketFeePercentage"":""425""}}";
    _responseId = SM.MavisId.OnSignTypeData(typedData);
    string responseData = await WaitForMavisIdResponse(_responseId);
    Debug.Log(responseData);
}
```

### Call contracts

Executes a custom transaction on a specified smart contract, returning a transaction hash.

```csharp
string OnCallContract(string contractAddress, string data, string value = "0x0")
```

Parameters:

* `contractAddress` (string): the address of the smart contract on which the transaction is to be executed.
* `data` (string): the transaction data to be sent to the smart contract, encoded as a hex string. This typically includes the function selector and the arguments. For more information on encoding your function data, see [Encode function data](https://docs.skymavis.com/mavis/mavis-id/guides/unity-sdk#encode-function-data).
* `value` (string): the amount of RON (in wei) to send along with the transaction. For non-payable smart contracts, the value is `0x0`.

Example:

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
            Debug.Log("Approve data: " + data);
            _responseId = SM.MavisId.OnCallContract(contractAddress, data);
            string responseData = await WaitForMavisIdResponse(_responseId);
            Debug.Log(responseData);

        }
        catch (System.Exception e)
        {
            Debug.Log("Error in call contract: " + e.Message);
        }

    }
```

## See also

* Head to the [playground source code](https://github.com/axieinfinity/mavis-id-unity/blob/main/Assets/Example/ID.cs) for complete use cases.
* Unity SDK guide with response details, utilities, and troubleshooting information: [Mavis ID Unity SDK](https://docs.skymavis.com/mavis/mavis-id/guides/unity-sdk)
