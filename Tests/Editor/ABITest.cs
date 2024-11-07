using NUnit.Framework;
using SkyMavis.Waypoint.Utils;

namespace SkyMavis.Waypoint.Tests.Editor
{
    public class ABITest
    {
        const string TestNameTemplate = "{0:120}";

        static readonly TestCaseData[] TestCases = new[] {
            new TestCaseData(
                "function abc(address maker, uint256 kind) nonpayable",
                new {
                    maker = "0x70275A9B6828F83cF710eE735F3E02f973353FF7",
                    kind = 1000
                }
            )
                .Returns("0xbdc5ea0b00000000000000000000000070275a9b6828f83cf710ee735f3e02f973353ff700000000000000000000000000000000000000000000000000000000000003e8")
                .SetName(TestNameTemplate)
            ,
            new TestCaseData(
                "function abc(address[] maker, uint256 kind) nonpayable",
                new {
                    maker = new [] { "0x70275A9B6828F83cF710eE735F3E02f973353FF7" },
                    kind = 1000
                }
            )
                .Returns("0x16d08d95000000000000000000000000000000000000000000000000000000000000004000000000000000000000000000000000000000000000000000000000000003e8000000000000000000000000000000000000000000000000000000000000000100000000000000000000000070275a9b6828f83cf710ee735f3e02f973353ff7")
                .SetName(TestNameTemplate)
            ,
            new TestCaseData(
                "function abc(tuple(address maker, uint256 kind) order) nonpayable",
                new {
                    order = new {
                        maker = "0x70275A9B6828F83cF710eE735F3E02f973353FF7",
                        kind = 1000
                    }
                }
            )
                .Returns("0x3c26d85800000000000000000000000070275a9b6828f83cf710ee735f3e02f973353ff700000000000000000000000000000000000000000000000000000000000003e8")
                .SetName(TestNameTemplate)
            ,
            new TestCaseData(
                "function abc(tuple(address maker, uint256 kind)[] order) nonpayable",
                new {
                    order = new [] {
                        new {
                            maker = "0x70275A9B6828F83cF710eE735F3E02f973353FF7",
                            kind = 1000
                        }
                    }
                }
            )
                .Returns("0x4b3ed09e0000000000000000000000000000000000000000000000000000000000000020000000000000000000000000000000000000000000000000000000000000000100000000000000000000000070275a9b6828f83cf710ee735f3e02f973353ff700000000000000000000000000000000000000000000000000000000000003e8")
                .SetName(TestNameTemplate)
            ,
            // Katana router
            new TestCaseData(
                "function swapExactRONForTokens(uint256 _amountOutMin, address[] _path, address _to, uint256 _deadline) payable",
                new
                {
                    _amountOutMin = 0,
                    _path = new [] { "0xa959726154953bae111746e265e6d754f48570e6", "0x3c4e17b9056272ce1b49f6900d8cfd6171a1869d" },
                    _to = "0x6B190089ed7F75Fe17B3b0A17F6ebd69f72c3F63",
                    _deadline = 19140313050
                }
            )
                .Returns("0x7da5cd66000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000800000000000000000000000006b190089ed7f75fe17b3b0a17f6ebd69f72c3f630000000000000000000000000000000000000000000000000000000474d9ffda0000000000000000000000000000000000000000000000000000000000000002000000000000000000000000a959726154953bae111746e265e6d754f48570e60000000000000000000000003c4e17b9056272ce1b49f6900d8cfd6171a1869d")
                .SetName(TestNameTemplate)
            ,
            new TestCaseData(
                "function swapExactTokensForTokens(uint256 _amountIn, uint256 _amountOutMin, address[] _path, address _to, uint256 _deadline)",
                new
                {
                    _amountIn = "1166912850812407680945",
                    _amountOutMin = "1580474142",
                    _path = new[] { "0xe514d9DEB7966c8BE0ca922de8a064264eA6bcd4", "0x0B7007c13325C48911F73A2daD5FA5dCBf808aDc" },
                    _to = "0x1d3286A3348Fa99852d147C57A79045B41c4f713",
                    _deadline = "1730877010"
                }
            )
                .Returns("0x38ed173900000000000000000000000000000000000000000000003f422b3f3e9ff48fb1000000000000000000000000000000000000000000000000000000005e341f1e00000000000000000000000000000000000000000000000000000000000000a00000000000000000000000001d3286a3348fa99852d147c57a79045b41c4f71300000000000000000000000000000000000000000000000000000000672b16520000000000000000000000000000000000000000000000000000000000000002000000000000000000000000e514d9deb7966c8be0ca922de8a064264ea6bcd40000000000000000000000000b7007c13325c48911f73a2dad5fa5dcbf808adc")
                .SetName(TestNameTemplate)
            ,
            new TestCaseData(
                "function swapTokensForExactTokens(uint256 _amountOut, uint256 _amountInMax, address[] _path, address _to, uint256 _deadline)",
                new
                {
                    _amountOut = "1500000000000000000000",
                    _amountInMax = "176279009572945650000",
                    _path = new[] { "0xe514d9DEB7966c8BE0ca922de8a064264eA6bcd4", "0x7EAe20d11Ef8c779433Eb24503dEf900b9d28ad7" },
                    _to = "0xa3410F80f7eA541Dddb2Fae89A0607Fb96412EF9",
                    _deadline = "2000000000"
                }
            )
                .Returns("0x8803dbee00000000000000000000000000000000000000000000005150ae84a8cdf000000000000000000000000000000000000000000000000000098e5cd93d891be15000000000000000000000000000000000000000000000000000000000000000a0000000000000000000000000a3410f80f7ea541dddb2fae89a0607fb96412ef900000000000000000000000000000000000000000000000000000000773594000000000000000000000000000000000000000000000000000000000000000002000000000000000000000000e514d9deb7966c8be0ca922de8a064264ea6bcd40000000000000000000000007eae20d11ef8c779433eb24503def900b9d28ad7")
                .SetName(TestNameTemplate)
            ,
            // ERC20
            new TestCaseData(
                "function transfer(address _to, uint256 _value)",
                new
                {
                    _to = "0xa87773578e7e8058809D5cd228Cf9fFD9dA1790C",
                    _value = "2430"
                }
            )
                .Returns("0xa9059cbb000000000000000000000000a87773578e7e8058809d5cd228cf9ffd9da1790c000000000000000000000000000000000000000000000000000000000000097e")
                .SetName(TestNameTemplate)
            ,
             // Axie Proxy
            new TestCaseData(
                "function safeTransferFrom(address _from, address _to, uint256 _tokenId)",
                new
                {
                    _from = "0x8D93571e5bDeE4dAF6AcE0d3e6A666adEBa8fB94",
                    _to = "0x7BEC858bb8Dd67035Bd8748a22Ec46A7A02CD63d",
                    _tokenId = "2132104"
                }
            )
                .Returns("0x42842e0e0000000000000000000000008d93571e5bdee4daf6ace0d3e6a666adeba8fb940000000000000000000000007bec858bb8dd67035bd8748a22ec46a7a02cd63d0000000000000000000000000000000000000000000000000000000000208888")
                .SetName(TestNameTemplate)
            ,
            new TestCaseData(
                "function disperseRON(address[] _recipients, uint256[] _values)",
                new
                {
                    _recipients = new [] { "0x7749dD0ef17Da3Ded340b6f171BE7688675C26e3", "0x4BDCEA276059FcAc9f90d609772701EEEcd9e583", "0xb6Ce40C04cD96078EdabA890c458ec0b87fE2092", "0xdCb9330DA9658f7995eE86e32208456d8be87d1A", "0x83F2EDC44fBe81F70acfA706aCc388F4f6135B3f", "0x1fb69ae1C1733B23ee6b2738B4Bf2a781BAe31a7" },
                    _values = new[] { "2000000000000000000", "2000000000000000000", "2000000000000000000", "2000000000000000000", "2000000000000000000", "2000000000000000000" }
                }
            )
                .Returns("0x2d8b6c810000000000000000000000000000000000000000000000000000000000000040000000000000000000000000000000000000000000000000000000000000012000000000000000000000000000000000000000000000000000000000000000060000000000000000000000007749dd0ef17da3ded340b6f171be7688675c26e30000000000000000000000004bdcea276059fcac9f90d609772701eeecd9e583000000000000000000000000b6ce40c04cd96078edaba890c458ec0b87fe2092000000000000000000000000dcb9330da9658f7995ee86e32208456d8be87d1a00000000000000000000000083f2edc44fbe81f70acfa706acc388f4f6135b3f0000000000000000000000001fb69ae1c1733b23ee6b2738b4bf2a781bae31a700000000000000000000000000000000000000000000000000000000000000060000000000000000000000000000000000000000000000001bc16d674ec800000000000000000000000000000000000000000000000000001bc16d674ec800000000000000000000000000000000000000000000000000001bc16d674ec800000000000000000000000000000000000000000000000000001bc16d674ec800000000000000000000000000000000000000000000000000001bc16d674ec800000000000000000000000000000000000000000000000000001bc16d674ec80000")
                .SetName(TestNameTemplate)
            ,
        };

        [TestCaseSource(nameof(TestCases))]
        public string EncodeFunctionDataTest(string readableAbi, object parameters) => ABI.EncodeFunctionData(readableAbi, parameters);
    }
}
