
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SkyMavis.Waypoint.Types
{
    public enum SDKState
    {
        NotInitialized,
        Initializing,
        Initialized
    }

    public enum ResponseType
    {
        [System.Runtime.Serialization.EnumMember(Value = "success")]
        Success,
        [System.Runtime.Serialization.EnumMember(Value = "fail")]
        Fail
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class ProviderRequest
    {
        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("params")]
        public object Params { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class InteractWithContractRequest
    {
        [JsonProperty("abi")]
        public JArray Abi { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("functionName")]
        public string FunctionName { get; set; }

        [JsonProperty("args")]
        public object[] Args { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; } = "0x0";

        public string gas;
        public string gasPrice;
        public string maxFeePerGas;
        public string maxPriorityFeePerGas;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="abiString"></param>
        /// <param name="functionName"></param>
        /// <param name="args"></param>
        /// <param name="value">The value with decimal</param>
        /// <param name="gas">The gas limit the sender is willing to pay to miners in wei.</param>
        /// <param name="gasPrice">The gas price the sender is willing to pay to miners in wei.</param>
        /// <param name="maxFeePerGas">Maximum fee per gas the sender is willing to pay to miners in wei.</param>
        /// <param name="maxPriorityFeePerGas">The maximum total fee per gas the sender is willing to pay (includes the network / base fee and miner / priority fee) in wei.</param>
        /// <returns></returns>
        public static InteractWithContractRequest New(
            string address,
            string abiString,
            string functionName,
            object[] args = null,
            string value = "0x0",
            string gas = null,
            string gasPrice = null,
            string maxFeePerGas = null,
            string maxPriorityFeePerGas = null
        )
        {
            var abi = JArray.Parse(abiString);
            return new InteractWithContractRequest()
            {
                Address = address,
                Abi = abi,
                FunctionName = functionName,
                Args = args,
                Value = value,
                gas = gas,
                gasPrice = gasPrice,
                maxFeePerGas = maxFeePerGas,
                maxPriorityFeePerGas = maxPriorityFeePerGas,
            };
        }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class ProviderResponse
    {
        [JsonProperty("data")]
        public object Data { get; set; }
        [JsonProperty("type")]
        public ResponseType? Type { get; set; }
        [JsonProperty("error")]
        public OverlayError Error { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    internal class UnsignedTransaction
    {
        public string to;
        public string value = "0x0";
        public string gas;
        public string data;
        public string gasPrice;
        public string maxFeePerGas;
        public string maxPriorityFeePerGas;
    }

    internal class GetTokenResponse
    {
        public string idToken;
        public AccountWallet accountWallet;
    }

    internal struct AccountWallet
    {
        public string identity;
        public string secondary;
    }
}
