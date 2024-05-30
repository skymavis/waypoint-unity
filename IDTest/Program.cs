using System;
using Newtonsoft.Json.Linq;
using SM.ID.Utils;
using System.Collections.Generic;
using System.Text.Json;

namespace IDTest
{
    class Program
    {
        

        static void Main(string[] args)
        {


            Console.Write("Encode ABI Flattern!");

            var flatternABI = @"{""name"":""abc"",""type"":""function"",""stateMutability"":""nonpayable"",""inputs"":[{""type"":""address"", ""name"":""maker""},{ ""type"":""uint256"",""name"":""kind""}],""outputs "":[]}";
            var flatternOrders = new
            {
                maker = "0x70275A9B6828F83cF710eE735F3E02f973353FF7",
                kind = 1000
            };
            var data = Encode.EncodeFunctionData(flatternABI, flatternOrders);
            var expectedResult = "0xbdc5ea0b00000000000000000000000070275a9b6828f83cf710ee735f3e02f973353ff700000000000000000000000000000000000000000000000000000000000003e8";
            Console.WriteLine("Data : " + data);
            Console.WriteLine("Data Length : " + data.Length);
            Console.WriteLine("Expected Length : " + expectedResult.Length);
            Console.WriteLine($"Is same with expected result : {data == expectedResult}");


            ApproveErc20Token();
            SwapRonToAxs();

            var orderData = GetOrderData();
            var settleOrder = SettleOrder(orderData);
            OrderExchange(settleOrder);


        }

        static string ApproveErc20Token()
        {
            var approveAbi = @"
                        {
                        ""constant"": false,
                        ""inputs"": [
                                {
                                ""internalType"": ""address"",
                                ""name"": ""_spender"",
                                ""type"": ""address""
                                },
                                {
                                ""internalType"": ""uint256"",
                                ""name"": ""_value"",
                                ""type"": ""uint256""
                                }
                        ],
                        ""name"": ""approve"",
                        ""outputs"": [
                                {
                                ""internalType"": ""bool"",
                                ""name"": ""_success"",
                                ""type"": ""bool""
                                }
                        ],
                        ""payable"": false,
                        ""stateMutability"": ""nonpayable"",
                        ""type"": ""function""
                        }";
            var approveParams = new { _spender = "0x6B190089ed7F75Fe17B3b0A17F6ebd69f72c3F63", _value = "1000000000000000000" };
            var expectedValue = "0x095ea7b30000000000000000000000006b190089ed7f75fe17b3b0a17f6ebd69f72c3f630000000000000000000000000000000000000000000000000de0b6b3a7640000";
            var data = Encode.EncodeFunctionData(approveAbi, approveParams);
            Console.WriteLine("Approve ERC20 Encoded data : " + data);
            Console.WriteLine("Approve ERC20 Encoded data Length : " + data.Length);
            Console.WriteLine("Approve expected data Length : " + data.Length);
            Console.WriteLine($"Is match with expected value : {data == expectedValue}");
            return data;
        }

        static string SwapRonToAxs ()
        {

            var swapABI = @"
                {
                    ""constant"": false,
                    ""inputs"": [
                        {
                            ""internalType"": ""uint256"",
                            ""name"": ""_amountOutMin"",
                            ""type"": ""uint256""
                        },
                        {
                            ""internalType"": ""address[]"",
                            ""name"": ""_path"",
                            ""type"": ""address[]""
                        },
                        {
                            ""internalType"": ""address"",
                            ""name"": ""_to"",
                            ""type"": ""address""
                        },
                        {
                            ""internalType"": ""uint256"",
                            ""name"": ""_deadline"",
                            ""type"": ""uint256""
                        }
                    ],
                    ""name"": ""swapExactRONForTokens"",
                    ""outputs"": [
                        {
                            ""internalType"": ""uint256[]"",
                            ""name"": ""_amounts"",
                            ""type"": ""uint256[]""
                        }
                    ],
                    ""payable"": true,
                    ""stateMutability"": ""payable"",
                    ""type"": ""function""
                }";

           
            var swapParams = new
            {
                // 0.1 Ron
                _amountOutMin = 0,
                _path = new string[] { "0xa959726154953bae111746e265e6d754f48570e6", "0x3c4e17b9056272ce1b49f6900d8cfd6171a1869d" },
                _to = "0x6B190089ed7F75Fe17B3b0A17F6ebd69f72c3F63",
                _deadline = 1814031305
            };

            try
            {
                var data = Encode.EncodeFunctionData(swapABI, swapParams);
                Console.WriteLine("____");
                Console.WriteLine("Swap Data : " + data);
                var expectedValue = "0x7da5cd66000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000800000000000000000000000006b190089ed7f75fe17b3b0a17f6ebd69f72c3f63000000000000000000000000000000000000000000000000000000006c1febc90000000000000000000000000000000000000000000000000000000000000002000000000000000000000000a959726154953bae111746e265e6d754f48570e60000000000000000000000003c4e17b9056272ce1b49f6900d8cfd6171a1869d";
                Console.WriteLine("Swap Ron Encoded data : " + data);
                Console.WriteLine("Swap Ron Encoded data Length : " + data.Length);
                Console.WriteLine("Swap Ron data Length : " + data.Length);
                Console.WriteLine($"Is match with expected value : {data == expectedValue}");
                Console.WriteLine("____");

                return data;

            }
            catch (System.Exception e)
            {
                Console.WriteLine("Error in call contract : " + e.Message);
                throw new Exception("Swapp error : " + e.Message);
            }
        }


        static string GetOrderData()
        {
            var orderObj = new
            {
                maker = "0x2d4cb4dcd4de54fe264142e56862558a91340949",
                kind = 1,
                assets = new object[]
                {
                    new
                    {
                        erc = 1,
                        addr = "0x32950db2a7164ae833121501c797d79e7b79d74c",
                        id = 3883265,
                        quantity = 0
                    }
                },
                expiredAt = 1729760513,
                paymentToken = "0xc99a6A985eD2Cac1ef41640596C5A5f9F4E19Ef5",
                startedAt = "1713949313",
                basePrice = "759999999999000",
                endedAt = "1714035713",
                endedPrice = "759999999999000",
                expectedState = 0,
                nonce = 0,
                marketFeePercentage = 425
            };
            var jOrderAbi = @"{""inputs"":[{""internalType"":""address"",""name"":""maker"",""type"":""address""},{""internalType"":""enum MarketOrder.OrderKind"",""name"":""kind"",""type"":""uint8""},{""components"":[{""internalType"":""enum MarketAsset.TokenStandard"",""name"":""erc"",""type"":""uint8""},{""internalType"":""address"",""name"":""addr"",""type"":""address""},{""internalType"":""uint256"",""name"":""id"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""quantity"",""type"":""uint256""}],""internalType"":""struct MarketAsset.Asset[]"",""name"":""assets"",""type"":""tuple[]""},{""internalType"":""uint256"",""name"":""expiredAt"",""type"":""uint256""},{""internalType"":""address"",""name"":""paymentToken"",""type"":""address""},{""internalType"":""uint256"",""name"":""startedAt"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""basePrice"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""endedAt"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""endedPrice"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""expectedState"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""nonce"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""marketFeePercentage"",""type"":""uint256""}],""name"":""order"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""}";
            var orderData = Encode.EncodeFunctionData(jOrderAbi, orderObj);
            var expected = "0xf81271220000000000000000000000002d4cb4dcd4de54fe264142e56862558a913409490000000000000000000000000000000000000000000000000000000000000001000000000000000000000000000000000000000000000000000000000000018000000000000000000000000000000000000000000000000000000000671a0d01000000000000000000000000c99a6a985ed2cac1ef41640596c5a5f9f4e19ef5000000000000000000000000000000000000000000000000000000006628ca810000000000000000000000000000000000000000000000000002b3374a077c1800000000000000000000000000000000000000000000000000000000662a1c010000000000000000000000000000000000000000000000000002b3374a077c180000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001a90000000000000000000000000000000000000000000000000000000000000001000000000000000000000000000000000000000000000000000000000000000100000000000000000000000032950db2a7164ae833121501c797d79e7b79d74c00000000000000000000000000000000000000000000000000000000003b41010000000000000000000000000000000000000000000000000000000000000000";
            Console.WriteLine("Order Data : " + orderData);
            Console.WriteLine("Is Order data match with expected: " + (orderData == expected));

            return expected;
        }

        static string SettleOrder(string orderData)
        {
            var payloadObj = new
            {
                settleInfo = new
                {
                    orderData = orderData,
                    signature = "0xd7ee18e8fd1d3882edd84ece8d60efd24930825f1791817cc670c005c44556357ad39c56fd100542b6810143db4399b16a58e8449ea0690c28b4f780793fc4ff1b",
                    referralAddr = "0x245db945c485b68fdc429e4f7085a1761aa4d45d",
                    expectedState = "0",
                    recipient = "0x63ade9e8257332928a6e0801c6e547e5523bb525",
                    refunder = "0x63ade9e8257332928a6e0801c6e547e5523bb525",
                },
                settlePrice = "759999999999000",

            };
            var jPayloadAbi = @"{""inputs"":[{""components"":[{""internalType"":""bytes"",""name"":""orderData"",""type"":""bytes""},{""internalType"":""bytes"",""name"":""signature"",""type"":""bytes""},{""internalType"":""address"",""name"":""referralAddr"",""type"":""address""},{""internalType"":""uint256"",""name"":""expectedState"",""type"":""uint256""},{""internalType"":""address"",""name"":""recipient"",""type"":""address""},{""internalType"":""address"",""name"":""refunder"",""type"":""address""}],""internalType"":""struct SettleParameter"",""name"":""settleInfo"",""type"":""tuple""},{""internalType"":""uint256"",""name"":""settlePrice"",""type"":""uint256""}],""name"":""settleOrder"",""outputs"":[],""stateMutability"":""payable"",""type"":""function""}";
            var payloadData = Encode.EncodeFunctionData(jPayloadAbi, payloadObj);

            Console.WriteLine("Payload data : " + payloadData);
            return payloadData;

        }

        static string OrderExchange(string settleData)
        {

            var jInteractWithAbi = @"{""inputs"":[{""internalType"":""string"",""name"":""_interface"",""type"":""string""},{""internalType"":""bytes"",""name"":""_data"",""type"":""bytes""}],""name"":""interactWith"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""}";
            var interactObj = new
            {
                _interface = "ORDER_EXCHANGE",
                _data = settleData
            };

            var data = Encode.EncodeFunctionData(jInteractWithAbi, interactObj);
            Console.WriteLine("Order Exchange : " + data);
            return data;
        }

    }

}