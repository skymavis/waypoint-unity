using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SkyMavis.Utils
{
    [Serializable]
    public class AbiParameter
    {
        public string type;
        public string name;
        public string internalType;
        public List<AbiParameter> components;
        public List<AbiParameter> inputs;
        public AbiParameter Clone()
        {
            var serializeObject = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<AbiParameter>(serializeObject);
        }
    }

    public class InvalidDefinitionTypeError : Exception
    {
        public InvalidDefinitionTypeError(string type)
            : base($"Missing or invalid abi function definition: {type}")
        {
        }
    }

    internal class PreparedParam
    {
        public bool dynamic;
        public string encoded;
    }

    public static class ABI
    {
        #region private
        private static PreparedParam EncodeAddress(string value)
        {
            var encoded = value.ToLower().RemoveHexPrefix().PadHexLeft(32);
            return new PreparedParam { dynamic = false, encoded = encoded };

        }

        private static PreparedParam EncodeNumber(int value, bool signed, int size)
        {
            var encoded = HexUtils.NumberToHex(value, signed, size);
            return new PreparedParam { dynamic = false, encoded = encoded };
        }

        private static PreparedParam EncodeNumber(BigInteger value, bool signed, int size)
        {
            var encoded = HexUtils.NumberToHex(value, signed, size);
            return new PreparedParam { dynamic = false, encoded = encoded };
        }

        private static PreparedParam GetEncodeValue(object obj, string inputType)
        {
            if (inputType.StartsWith("uint") || inputType.StartsWith("int"))
            {
                bool signed = inputType.StartsWith("int");
                int size = int.Parse(inputType.Substring(inputType.IndexOf("int") + 3)) / 8;
                BigInteger value;
                if (obj is string strValue)
                {
                    if (strValue.StartsWith("0x"))
                    {
                        value = BigInteger.Parse(strValue.RemoveHexPrefix(), System.Globalization.NumberStyles.AllowHexSpecifier, null);
                    }
                    else
                    {
                        value = BigInteger.Parse(strValue);
                    }
                }
                else if (obj is long longValue)
                {
                    value = new BigInteger(longValue);
                }
                else if (obj is int intValue)
                {
                    value = new BigInteger(intValue);
                }
                else
                {
                    throw new System.NotImplementedException();
                }
                if (size % 32 != 0)
                {
                    size = (int)(Math.Ceiling(size / 32f) * 32);
                }
                return EncodeNumber(value, signed, size);
            }
            else if (inputType == "address")
            {
                string value = (string)obj;
                return EncodeAddress(value.RemoveHexPrefix());
            }
            else if (inputType.Contains("byte"))
            {
                if (obj is string value)
                {
                    return EncodeBytes(inputType, value.RemoveHexPrefix());
                }
                else
                {
                    throw new InvalidDataException();
                }
            }
            else if (inputType == "string")
            {
                if (obj is string value)
                {
                    return EncodeString(value);
                }
                else
                {
                    throw new InvalidDataException();
                }
            }
            else
            {
                throw new InvalidDataException();
            }
        }

        private static PreparedParam EncodeArray(object[] collection, int length, AbiParameter element)
        {
            bool dynamic = length == -1;

            List<PreparedParam> preparedParams = new List<PreparedParam>();
            bool dynamicChild = false;
            for (int i = 0; i < collection.Length; i++)
            {
                var preparedParam = InternalEncodeInput(collection[i], 0, element);
                if (preparedParam.dynamic) dynamicChild = true;
                preparedParams.Add(preparedParam);
            }
            if (dynamic || dynamicChild)
            {
                var data = EncodePreparedParam(preparedParams);
                if (dynamic)
                {
                    var length2 = EncodeNumber(preparedParams.Count, false, 32);
                    return new PreparedParam
                    {
                        dynamic = true,
                        encoded = preparedParams.Count > 0 ? length2.encoded + data : length2.encoded,
                    };
                }
                if (dynamicChild) return new PreparedParam { dynamic = true, encoded = data };
            }

            string encoded = "";
            preparedParams.ForEach(x => encoded += x.encoded.RemoveHexPrefix());
            return new PreparedParam { dynamic = false, encoded = encoded };
        }

        private static PreparedParam EncodeTuple(object obj, AbiParameter element)
        {
            var valType = obj.GetType();
            bool dynamic = false;

            List<PreparedParam> preparedParams = new List<PreparedParam>();
            var components = element.components;
            for (int i = 0; i < components.Count; i++)
            {
                var component = components[i];
                var fields = Reflector.Reflect(valType);
                string componentName = $"<{component.name}>i__Field";
                var field = fields.FirstOrDefault(x => x.Name == componentName);
                if (field == null)
                {
                    throw new System.ArgumentException();
                }
                var preparedParam = InternalEncodeInput(field.GetValue(obj), 0, component);
                preparedParams.Add(preparedParam);
                if (preparedParam.dynamic) dynamic = true;
            }

            string encoded = "";
            if (dynamic)
            {
                encoded = EncodePreparedParam(preparedParams);
            }
            else
            {
                preparedParams.ForEach(x => encoded += x.encoded.RemoveHexPrefix());
            }

            return new PreparedParam { dynamic = dynamic, encoded = encoded };
        }

        private static PreparedParam EncodeBytes(string type, string hexValue)
        {
            var regex = new Regex(@"bytes(\d+)");
            var match = regex.Match(type);
            // Detect bytes size of type
            string paramSize = match.Success ? match.Groups[1].Value : null;

            var bytesSize = hexValue.HexSize();// HexUtils.Size(hexValue);

            if (string.IsNullOrEmpty(paramSize))
            {
                // If the size is not divisible by 32 bytes, pad the end
                // with empty bytes to the ceiling 32 bytes.
                var value = hexValue;
                if (bytesSize % 32 != 0)
                {
                    int size = (int)Math.Ceiling(((double)bytesSize / 32));
                    value = value.PadHexRight(size * 32);
                }
                string paddedBytesSize = HexUtils.NumberToHex(bytesSize).PadHexLeft(32);

                return new PreparedParam { dynamic = true, encoded = paddedBytesSize + value };
            }
            if (bytesSize != Int32.Parse(paramSize))
            {
                throw new ArgumentException("Value size is not equal bytes size");
            }
            return new PreparedParam { dynamic = false, encoded = hexValue.PadHexRight(32) };
        }

        private static PreparedParam EncodeString(string value)
        {
            var hexValue = HexUtils.StringToHex(value, 32).RemoveHexPrefix();

            var partsLength = (int)Math.Ceiling((double)((hexValue.Length / 2) / 32));

            List<string> parts = new List<string>();
            for (var i = 0; i < partsLength; i++)
            {
                var subHex = hexValue.Substring(i * 32, (i + 1) * 32);
                parts.Add(subHex.PadHexRight(32));
            }

            string paddedValue = HexUtils.NumberToHex(value.Length).PadHexLeft(32);


            parts.Insert(0, paddedValue);
            string encoded = "";
            parts.ForEach(x => encoded += x);
            return new PreparedParam { dynamic = true, encoded = encoded };
        }

        private static string EncodePreparedParam(List<PreparedParam> preparedParams)
        {
            int staticSize = 0;
            for (int i = 0; i < preparedParams.Count; i++)
            {
                var preparedParam = preparedParams[i];
                if (preparedParam.dynamic) staticSize += 32;
                else staticSize += preparedParam.encoded.Length / 2;
            }

            // 2. Split the parameters into static and dynamic parts.
            var staticParams = new List<string>();
            var dynamicParams = new List<string>();
            int dynamicSize = 0;
            for (int i = 0; i < preparedParams.Count; i++)
            {
                var preparedParam = preparedParams[i];
                if (preparedParam.dynamic)
                {
                    staticParams.Add(EncodeNumber(staticSize + dynamicSize, false, 32).encoded);
                    dynamicParams.Add(preparedParam.encoded);
                    dynamicSize += preparedParam.encoded.Length / 2;
                }
                else
                {
                    staticParams.Add(preparedParam.encoded);
                }
            }

            // 3. Concatenate static and dynamic parts.
            string ret = "";
            staticParams.ForEach(x => ret += x.Replace("0x", ""));
            dynamicParams.ForEach(x => ret += x.Replace("0x", ""));
            return ret;
        }

        private static PreparedParam InternalEncodeInput(object obj, int index, AbiParameter input)
        {
            string inputType = input.type;
            var valType = obj.GetType();

            if (valType.IsArray)
            {
                var collection = obj as object[];
                if (inputType.Contains("[]"))
                {
                    string elemType = inputType.Replace("[]", "");
                    var element = input.Clone();
                    element.type = elemType;
                    return EncodeArray(collection, -1, element);
                }
                else
                {
                    var val = collection[index];
                    return InternalEncodeInput(val, 0, input);
                }
            }
            else if (inputType == "tuple")
            {
                return EncodeTuple(obj, input);
            }
            else
            {
                return GetEncodeValue(obj, inputType);
            }
        }

        private static string EncodeFunctionName(string functionName)
        {
            var input = Encoding.UTF8.GetBytes(functionName);
            var digest = new KeccakDigest(256);
            var output = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(input, 0, input.Length);
            digest.DoFinal(output, 0);
            return HexUtils.BytesToHex(output);
        }

        private static string FormatAbiParams(IEnumerable<AbiParameter> inputs, bool includeName = false)
        {
            if (inputs == null) return "";
            return string.Join(includeName ? ", " : ",", inputs.Select(param => FormatAbiParam(param, includeName)));
        }

        private static string FormatAbiParam(AbiParameter param, bool includeName)
        {
            if (param.type.StartsWith("tuple"))
            {
                return $"({FormatAbiParams(param.components, includeName)}){param.type.Substring(5)}";
            }
            return param.type + (includeName ? $" {param.name}" : "");
        }

        private static List<AbiParameter> ParseParameters(string parameters)
        {
            var parameterList = new List<AbiParameter>();
            if (string.IsNullOrEmpty(parameters)) return parameterList;

            int start = 0;
            int depth = 0;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == '(') depth++;
                else if (parameters[i] == ')') depth--;
                else if (parameters[i] == ',' && depth == 0)
                {
                    parameterList.Add(ParseSingleParameter(parameters.Substring(start, i - start).Trim()));
                    start = i + 1;
                }
            }
            parameterList.Add(ParseSingleParameter(parameters.Substring(start).Trim()));

            return parameterList;
        }

        private static AbiParameter ParseSingleParameter(string param)
        {
            var parts = param.Trim().Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                string[] subparts = parts[0].Split(' ');
                var abiParam = ParseType(subparts[0]);
                abiParam.name = subparts[1];
                return abiParam;
            }
            else if (parts.Length == 2 || parts.Length == 3)
            {
                if (parts[0] == "tuple" && parts.Length == 2)
                {
                    var tupleParams = ParseParameters(parts[1]);
                    return new AbiParameter { type = "tuple", components = tupleParams };
                }
                else if (parts[0] == "tuple" && parts.Length == 3)
                {
                    var tupleParams = ParseParameters(parts[1]);
                    if (parts[2].Contains("[]"))
                        return new AbiParameter { type = "tuple[]", components = tupleParams, name = parts[2].Remove(0, 3) };
                    return new AbiParameter { type = "tuple", components = tupleParams, name = parts[2].Remove(0, 1) };
                }
                else
                {
                    var abiParam = ParseType(parts[0]);
                    abiParam.name = parts[1];
                    return abiParam;
                }
            }
            else
            {
                throw new ArgumentException("Invalid parameter format");
            }
        }
        private static AbiParameter ParseType(string type)
        {
            if (type.StartsWith("tuple"))
            {
                var abiParam = new AbiParameter { type = "tuple" };
                int start = type.IndexOf('(');
                int end = type.LastIndexOf(')');
                if (start >= 0 && end >= 0)
                {
                    var components = type.Substring(start + 1, end - start - 1);
                    abiParam.components = ParseParameters(components);
                }
                if (type.EndsWith("[]"))
                {
                    abiParam.type = "tuple[]";
                }
                return abiParam;
            }
            return new AbiParameter { type = type };
        }
        #endregion

        #region public
        public static string FormatAbiItem(AbiParameter abiItem, bool includeName = false)
        {
            if (abiItem.type != "function" && abiItem.type != "event" && abiItem.type != "error")
            {
                throw new InvalidDefinitionTypeError(abiItem.type);
            }

            return $"{abiItem.name}({FormatAbiParams(abiItem.inputs, includeName)})";
        }

        public static string EncodeFunctionData(string readableAbi, object values)
        {
            AbiParameter abiItems = ParseAbi(readableAbi);
            var data = EncodeAbiParameters(abiItems, values);
            string funcNormalized = FormatAbiItem(abiItems, false);
            var sig = EncodeFunctionName(funcNormalized).Substring(0, 8);

            if (data == "") return "0x";
            return "0x" + sig + data.RemoveHexPrefix();
        }

        private static string EncodeAbiParameters(AbiParameter abiParameter, object values)
        {
            var inputs = abiParameter.inputs;
            if (inputs == null)
            {
                throw new System.ArgumentException();
            }
            List<PreparedParam> preparedParams = new List<PreparedParam>();
            bool dynamic = false;
            for (int i = 0; i < inputs.Count; i++)
            {
                AbiParameter input = inputs[i];
                var valType = values.GetType();
                if (valType.IsArray)
                {
                    var preparedParam = InternalEncodeInput(values, i, input);
                    preparedParams.Add(preparedParam);
                    if (preparedParam.dynamic) dynamic = true;
                }
                else if (valType.IsClass)
                {
                    var fields = Reflector.Reflect(valType);
                    string inputName = $"<{input.name}>i__Field";
                    var field = fields.FirstOrDefault(x => x.Name == inputName);
                    string fieldValues = string.Join(", ", fields.Select(f => f.GetValue(values).ToString()));
                    if (field == null)
                    {
                        throw new System.ArgumentException();
                    }
                    var preparedParam = InternalEncodeInput(field.GetValue(values), 0, input);
                    preparedParams.Add(preparedParam);
                    if (preparedParam.dynamic) dynamic = true;
                }
            }
            string encoded = "";
            if (dynamic)
            {
                encoded = EncodePreparedParam(preparedParams);
            }
            else
            {
                preparedParams.ForEach(x => encoded += x.encoded.Replace("0x", ""));
            }
            return "0x" + encoded.ToLower();
        }

        public static AbiParameter ParseAbi(string abiString)
        {

            string type;
            if (abiString.Contains("function")) type = "function";
            else if (abiString.Contains("event")) type = "event";
            else if (abiString.Contains("error")) type = "error";
            else throw new ArgumentException("Invalid ABI string");
            string pattern = @"^function (?<name>[a-zA-Z$_][a-zA-Z0-9$_]*)\((?<parameters>.*?)\)(?: (?<scope>external|public{1}))?(?: (?<stateMutability>pure|view|nonpayable|payable{1}))?(?: returns\s?\((?<returns>.*?)\))?$";
            Match match;
            Regex regex = new Regex(pattern);
            match = regex.Match(abiString);
            if (!match.Success) throw new ArgumentException("Invalid ABI format");
            string name = match.Groups["name"].Value;
            string parameters = match.Groups["parameters"].Value;
            List<AbiParameter> inputs = ParseParameters(parameters);
            AbiParameter AbiObj = new AbiParameter
            {
                name = name,
                type = type,
                inputs = inputs,
            };
            return AbiObj;
        }

        public static List<AbiParameter> ParseAbis(string[] abiString)
        {
            string type;
            var abiList = new List<AbiParameter>();
            string pattern = @"^function (?<name>[a-zA-Z$_][a-zA-Z0-9$_]*)\((?<parameters>.*?)\)(?: (?<scope>external|public{1}))?(?: (?<stateMutability>pure|view|nonpayable|payable{1}))?(?: returns\s?\((?<returns>.*?)\))?$";
            Match match;
            Regex regex = new Regex(pattern);
            foreach (var abi in abiString)
            {
                if (abi.Contains("function")) type = "function";
                else if (abi.Contains("event")) type = "event";
                else if (abi.Contains("error")) type = "error";
                else throw new ArgumentException("Invalid ABI string");
                match = regex.Match(abi);
                if (!match.Success) throw new ArgumentException("Invalid ABI format");
                string name = match.Groups["name"].Value;
                string parameters = match.Groups["parameters"].Value;
                var inputs = ParseParameters(parameters);
                var abiObject = new AbiParameter
                {
                    name = name,
                    type = type,
                    inputs = inputs,
                };
                abiList.Add(abiObject);
            }
            return abiList;
        }

        #endregion
    }

}
