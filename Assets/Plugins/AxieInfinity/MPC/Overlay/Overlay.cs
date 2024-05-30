#if UNITY_STANDALONE
using SM.ID.Types;
using UnityEngine.Events;

namespace SM.ID
{
    internal static partial class Overlay
    {
        private static bool _verboseLog = false;
        private static bool _initialized = false;
        public static UnityAction<string> OnDataResponsed;



        /// <summary>
        /// Initialize the SDK, this function must be called before other functions.
        /// </summary>
        /// <param name="sessionID">Session ID</param>
        /// <param name="port">Port number for overlay connection</param>
        /// <param name="verboseLog">Enable verbose logging or not</param>
        /// <returns></returns>
        public static void Initialize(
            string sessionID,
            int port,
            bool verboseLog = false
        )
        {
            // Initialize SDK params
            _verboseLog = verboseLog;

            Log("Initializing Overlay SDK...");
            InitializeSocket(port, sessionID);

            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">A unique identifier</param>
        /// <returns></returns>
        public static void GetChainID(
            string state
        )
        {
            ThrowIfNotInitialized();

            RequestToOverlay(
                "provider:request",
                state,
                new ProviderRequest()
                {
                    Method = "eth_chainId",
                }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">A unique identifier</param>
        /// <param name="addressList"></param>
        /// <returns></returns>
        public static void GetAccounts(
            string state
        )
        {
            ThrowIfNotInitialized();

            RequestToOverlay(
                "provider:request",
                state,
                new ProviderRequest()
                {
                    Method = "eth_accounts",
                }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">A unique identifier</param>
        /// <returns></returns>
        public static void GetRequestAccounts(
            string state
        )
        {
            ThrowIfNotInitialized();

            RequestToOverlay(
                "provider:request",
                state,
                new ProviderRequest()
                {
                    Method = "eth_accounts",
                }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">A unique identifier</param>
        /// <param name="message">The message to sign</param>
        /// <returns></returns>
        public static void PersonalSign(
            string state,
            string message
        )
        {
            ThrowIfNotInitialized();

            RequestToOverlay(
                "provider:request",
                state,
                new ProviderRequest()
                {
                    Method = "personal_sign",
                    Params = message,
                }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">A unique identifier</param>
        /// <param name="typedData">The JSON string of the object that specifies the data to be signed.</param>
        /// <returns></returns>
        public static void SignTypedData(
            string state,
            string typedData
        )
        {
            ThrowIfNotInitialized();

            RequestToOverlay(
                "provider:request",
                state,
                new ProviderRequest()
                {
                    Method = "eth_signTypedData_v4",
                    Params = typedData,
                }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">A unique identifier</param>
        /// <param name="receiverAddress">Address of receiver</param>
        /// <param name="value">The value with decimal</param>
        /// <param name="gas">The gas limit the sender is willing to pay to miners in wei.</param>
        /// <param name="gasPrice">The gas price the sender is willing to pay to miners in wei.</param>
        /// <param name="maxFeePerGas">Maximum fee per gas the sender is willing to pay to miners in wei.</param>
        /// <param name="maxPriorityFeePerGas">The maximum total fee per gas the sender is willing to pay (includes the network / base fee and miner / priority fee) in wei.</param>
        /// <returns></returns>
        public static void SendNativeToken(
            string state,
            string receiverAddress,
            string value,
            string gas = null,
            string gasPrice = null,
            string maxFeePerGas = null,
            string maxPriorityFeePerGas = null
        )
        {
            SendTransaction(
                state,
                receiverAddress,
                value,
                "",
                gas,
                gasPrice,
                maxFeePerGas,
                maxPriorityFeePerGas
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">A unique identifier</param>
        /// <param name="receiverAddress">Address of receiver</param>
        /// <param name="value">The value with decimal</param>
        /// <param name="calldata">The data passed along with a transaction</param>
        /// <param name="gas">The gas limit the sender is willing to pay to miners in wei.</param>
        /// <param name="gasPrice">The gas price the sender is willing to pay to miners in wei.</param>
        /// <param name="maxFeePerGas">Maximum fee per gas the sender is willing to pay to miners in wei.</param>
        /// <param name="maxPriorityFeePerGas">The maximum total fee per gas the sender is willing to pay (includes the network / base fee and miner / priority fee) in wei.</param>
        /// <returns></returns>
        public static void SendTransaction(
            string state,
            string receiverAddress,
            string value,
            string calldata,
            string gas = null,
            string gasPrice = null,
            string maxFeePerGas = null,
            string maxPriorityFeePerGas = null
        )
        {
            ThrowIfNotInitialized();

            RequestToOverlay(
                "provider:request",
                state,
                new ProviderRequest()
                {
                    Method = "eth_sendTransaction",
                    Params = new UnsignedTransaction()
                    {
                        to = receiverAddress,
                        value = value,
                        gas = gas,
                        data = calldata,
                        gasPrice = gasPrice,
                        maxFeePerGas = maxFeePerGas,
                        maxPriorityFeePerGas = maxPriorityFeePerGas
                    }
                }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">A unique identifier</param>
        /// <param name="request">Request data for reading contract</param>
        /// <returns></returns>
        public static void ReadContract(
            string state,
            InteractWithContractRequest request
        )
        {
            ThrowIfNotInitialized();

            RequestToOverlay(
                "provider:contract:read:request",
                state,
                request
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">A unique identifier</param>
        /// <param name="request">Request data for writing contract</param>
        /// <param name="onComplete">Action for handling response if not using Async</param>
        /// <returns></returns>
        public static void WriteContract(
            string state,
            string data
        )
        {
            ThrowIfNotInitialized();
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<InteractWithContractRequest>(data);
            RequestToOverlay(
                "provider:contract:write:request",
                state,
                request
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">A unique identifier</param>
        /// <returns></returns>
        public static void GetIDToken(string state)
        {
            ThrowIfNotInitialized();

            RequestToOverlay(
                "id:token:request",
                state
            );
        }
    }
}
#endif