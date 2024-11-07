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
        };

        [TestCaseSource(nameof(TestCases))]
        public string EncodeFunctionDataTest(string readableAbi, object parameters) => ABI.EncodeFunctionData(readableAbi, parameters);
    }
}
