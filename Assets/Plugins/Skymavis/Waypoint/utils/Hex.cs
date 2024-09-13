using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace SkyMavis.Utils
{
    internal static class HexUtils
    {
        private static readonly byte[] Empty = new byte[0];
        public static string RemoveHexPrefix(this string value)
        {
            return value.Substring(value.StartsWith("0x") ? 2 : 0);
        }

        public static string StringToHex(string strValue, int size)
        {
            var value = Encoding.UTF8.GetBytes(strValue);
            return BytesToHex(value, size);
        }

        public static string BytesToHex(byte[] bytes, int size = -1)
        {
            var hex = string.Concat(bytes.Select(b => b.ToString("x2")).ToArray());

            if (size != -1)
            {
                return hex.PadHexRight(size);// Pad.PadHex(hex, new PadOptions { Dir = Direction.Right, Size = size });
            }
            return hex;
        }
        public static byte[] HexToByteArray(string hexValue)
        {
            try
            {
                return HexToByteArrayInternal(hexValue);
            }
            catch (FormatException ex)
            {
                throw new FormatException(string.Format(
                    "String '{0}' could not be converted to byte array (not hex?).", hexValue), ex);
            }
        }

        private static byte[] HexToByteArrayInternal(string hexValue)
        {
            byte[] bytes = null;
            if (string.IsNullOrEmpty(hexValue))
            {
                bytes = Empty;
            }
            else
            {
                //var string_length = hexValue.Length;
                //var character_index = hexValue.StartsWith("0x", StringComparison.Ordinal) ? 2 : 0;
                //// Does the string define leading HEX indicator '0x'. Adjust starting index accordingly.               
                //var numberOfCharacters = string_length - character_index;
                int numberOfCharacters = hexValue.Length;
                int offset = 0;

                var add_leading_zero = false;
                if (0 != numberOfCharacters % 2)
                {
                    add_leading_zero = true;

                    numberOfCharacters += 1; // Leading '0' has been striped from the string presentation.
                }

                bytes = new byte[numberOfCharacters / 2]; // Initialize our byte array to hold the converted string.

                var write_index = 0;
                if (add_leading_zero)
                {
                    bytes[write_index++] = FromCharacterToByte(hexValue[offset], offset);
                    offset += 1;
                }

                for (var read_index = offset; read_index < hexValue.Length; read_index += 2)
                {
                    var upper = FromCharacterToByte(hexValue[read_index], read_index, 4);
                    var lower = FromCharacterToByte(hexValue[read_index + 1], read_index + 1);

                    bytes[write_index++] = (byte)(upper | lower);
                }
            }

            return bytes;
        }

        private static byte FromCharacterToByte(char character, int index, int shift = 0)
        {
            var value = (byte)character;
            if (0x40 < value && 0x47 > value || 0x60 < value && 0x67 > value)
            {
                if (0x40 == (0x40 & value))
                    if (0x20 == (0x20 & value))
                        value = (byte)((value + 0xA - 0x61) << shift);
                    else
                        value = (byte)((value + 0xA - 0x41) << shift);
            }
            else if (0x29 < value && 0x40 > value)
            {
                value = (byte)((value - 0x30) << shift);
            }
            else
            {
                throw new FormatException(string.Format(
                    "Character '{0}' at index '{1}' is not valid alphanumeric character.", character, index));
            }

            return value;
        }

        public static string NumberToHex(BigInteger value, bool signed = false, int size = 4)
        {
            BigInteger maxValue;

            if (signed)
                maxValue = (BigInteger.One << (size * 8 - 1)) - 1;
            else
                maxValue = (BigInteger.One << (size * 8)) - 1;


            BigInteger minValue = signed ? -maxValue - 1 : 0;

            if ((maxValue != 0 && value > maxValue) || value < minValue)
            {
                throw new IntegerOutOfRangeError(
                    maxValue.ToString(),
                    minValue.ToString(),
                    signed,
                    size,
                    value.ToString());
            }

            string hex = (signed && value < 0
                               ? (BigInteger.One << (size * 8)) + value
                               : value).ToString("X").ToLower();

            return PadHexLeft(hex, size);
        }

        public static string PadHexLeft(this string hex, int size)
        {
            return hex.PadLeft(size * 2, '0');
        }

        public static string PadHexRight(this string hex, int size)
        {
            return hex.PadRight(size * 2, '0');
        }

        public static bool IsHex(string value, bool strict = false)
        {
            // Simple check: For strict mode, ensure the entire string is valid hex.
            // For non-strict mode, just check for "0x" prefix.
            if (strict)
            {
                // A more comprehensive check can be added here for strict mode.
                throw new NotImplementedException("Strict mode hex check not implemented.");
            }
            else
            {
                return value.StartsWith("0x");
            }
        }

        public static int HexSize(this string hexValue)
        {
            return hexValue.Length / 2;
        }

    }

    public class IntegerOutOfRangeError : Exception
    {
        public string Max { get; }
        public string Min { get; }
        public bool Signed { get; }
        public int? Size { get; }
        public string Value { get; }

        public IntegerOutOfRangeError(string max, string min, bool signed, int? size, string value)
            : base($"Integer out of range: {value} not in [{min}, {max}]")
        {
            Max = max;
            Min = min;
            Signed = signed;
            Size = size;
            Value = value;
        }

    }
    public class SizeExceedsPaddingSizeException : Exception
    {
        public SizeExceedsPaddingSizeException(string message) : base(message) { }
    }
}
