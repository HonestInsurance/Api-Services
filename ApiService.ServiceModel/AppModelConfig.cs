/**
 * @description Class to provide generic functionality and configuration settings that are initialized at the start of the service
 * @copyright (c) 2017 HIC Limited (NZBN: 9429043400973)
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using System;
using System.Text;
using System.IO;
using ServiceStack;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Hex.HexConvertors.Extensions;


namespace ApiService.ServiceModel
{
    /// <summary>
    /// The static AppModelConfig class holds provides generic functionality and configuration settings that are initialized at the start of the web service
    /// </summary>
    public static class AppModelConfig
    {
        // Contract details (ABI, Binary, Contract name, etc.) of the smart contracts this solution iteracts with
        public static readonly string EXTACCESSI;
        public static readonly string INTACCESSI;
        public static readonly string SETUPI;
        public static readonly string LIB;
        public static readonly string POOL;
        public static readonly string BOND;
        public static readonly string BANK;
        public static readonly string POLICY;
        public static readonly string SETTLEMENT;
        public static readonly string ADJUSTOR;
        public static readonly string TIMER;
        public static readonly string TRUST;
        public static IHostingEnvironment hostingEnvironment;
        public static ulong defaultGasPrice;
        public static ulong defaultGasLimit;
        public static ulong maxWaitDurationForTransactionReceipt;
        public static ulong defaultNumberEntriesForLazyLoading;
        public static ulong defaultBlockRangeForEventLogLoading;
        public static string WEB3_URL_ENDPOINT;

        public const string EMPTY_HASH = "0x0000000000000000000000000000000000000000000000000000000000000000";
        public const string EMPTY_KEY = "0x0000000000000000000000000000000000000000000000000000000000000000";
        public const string EMPTY_ADDRESS = "0x0000000000000000000000000000000000000000";


        // Constructor of this static class
        static AppModelConfig() {
            using (StreamReader r = new StreamReader("config/contractABIs.json")) {
                // Create a Json Object and parse the stream reader's input
                JObject json = JObject.Parse(r.ReadToEnd());
                // Load and parse all the Ethereum contract json files this solution interacts with
                EXTACCESSI = json.Value<JArray>("abiExtAccessI").ToString().ReplaceAll("\"", "'").ReplaceAll("\n", "").ReplaceAll(" ", "");
                INTACCESSI = json.Value<JArray>("abiIntAccessI").ToString().ReplaceAll("\"", "'").ReplaceAll("\n", "").ReplaceAll(" ", "");
                SETUPI = json.Value<JArray>("abiSetupI").ToString().ReplaceAll("\"", "'").ReplaceAll("\n", "").ReplaceAll(" ", "");
                LIB = json.Value<JArray>("abiLib").ToString().ReplaceAll("\"", "'").ReplaceAll("\n", "").ReplaceAll(" ", "");
                POOL = json.Value<JArray>("abiPool").ToString().ReplaceAll("\"", "'").ReplaceAll("\n", "").ReplaceAll(" ", "");
                BOND = json.Value<JArray>("abiBond").ToString().ReplaceAll("\"", "'").ReplaceAll("\n", "").ReplaceAll(" ", "");
                BANK = json.Value<JArray>("abiBank").ToString().ReplaceAll("\"", "'").ReplaceAll("\n", "").ReplaceAll(" ", "");
                POLICY = json.Value<JArray>("abiPolicy").ToString().ReplaceAll("\"", "'").ReplaceAll("\n", "").ReplaceAll(" ", "");
                SETTLEMENT = json.Value<JArray>("abiSettlement").ToString().ReplaceAll("\"", "'").ReplaceAll("\n", "").ReplaceAll(" ", "");
                ADJUSTOR = json.Value<JArray>("abiAdjustor").ToString().ReplaceAll("\"", "'").ReplaceAll("\n", "").ReplaceAll(" ", "");
                TIMER = json.Value<JArray>("abiTimer").ToString().ReplaceAll("\"", "'").ReplaceAll("\n", "").ReplaceAll(" ", "");
                TRUST = json.Value<JArray>("abiTrust").ToString().ReplaceAll("\"", "'").ReplaceAll("\n", "").ReplaceAll(" ", "");
            }
        }

        // Uses the provided Hosting Environment varialbe to load and initialise 
        // the web3 url endpoint as configured in the config/config.json file
        public static void setEnvironment(IHostingEnvironment env) {
            // Save the application environment
            hostingEnvironment = env;

            // Load the web 3 url endpoint from the config.json file
            using (StreamReader r = new StreamReader("config/config.json")) {
                // Create a Json Object and parse the stream reader's input
                JObject json = JObject.Parse(r.ReadToEnd());
                // Extract the specified url endpoint based on the environment name
                if (json.Value<string>(env.EnvironmentName) != "")
                    // Set the url to the value configured in the config.json file
                    WEB3_URL_ENDPOINT = json.Value<string>(env.EnvironmentName);
                // In case the environment name is not configured set the url to localhost
                else WEB3_URL_ENDPOINT = "http://localhost:8545/";

                // Get the defaultGasPrice
                defaultGasPrice = json.Value<ulong>("DefaultGasPrice");

                // Get the defaultGasLimit
                defaultGasLimit = json.Value<ulong>("DefaultGasLimit");

                // Get the MaxWaitDurationForTransactionResult
                maxWaitDurationForTransactionReceipt = json.Value<ulong>("MaxWaitDurationForTransactionReceipt");
                
                // Get the DefaultNumberEntriesForLazyLoading
                defaultNumberEntriesForLazyLoading = json.Value<ulong>("DefaultNumberEntriesForLazyLoading");

                // Get the DefaultNumberEntriesForLazyLoading
                defaultBlockRangeForEventLogLoading = json.Value<ulong>("DefaultBlockRangeForEventLogLoading");
            }
        }

        // Verify if a string provided is an empty hash
        public static bool isEmptyHash (string hash) {
            if ((hash == null) || 
                (hash == "") ||
                (hash == "0") ||
                (hash == "0x0") ||
                (hash == EMPTY_HASH) ||
                (hash == EMPTY_HASH.RemoveHexPrefix()))
                return true;
            else return false;
        }

        // Verify if a string provided is an empty private key
        public static bool isEmptyPrivateKey (string key) {
            if ((key == null) || 
                (key == "") ||
                (key == "0") ||
                (key == "0x0") ||
                (key == EMPTY_KEY) ||
                (key == EMPTY_KEY.RemoveHexPrefix()))
                return true;
            else return false;
        }

        // Verify if a string provided is an empty address
        public static bool isEmptyAdr (string adr) {
            if ((adr == null) || 
                (adr == "") ||
                (adr == "0") ||
                (adr == "0x0") ||
                (adr == EMPTY_ADDRESS) ||
                (adr == EMPTY_ADDRESS.RemoveHexPrefix()))
                return true;
            else return false;
        }
        
        public static string adrToString32(string raw) {
            return "0x000000000000000000000000" + raw.RemoveHexPrefix();
        }

        public static string getAdrFromString32(string raw) {
            return raw.Substring(26, 40).EnsureHexPrefix();
        }

        public static string FromHexString(string hexString) {
            hexString = hexString.RemoveHexPrefix().ReplaceAll("00", "");
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return Encoding.ASCII.GetString(bytes); // returns: "Hello world" for "48656C6C6F20776F726C64"
        }

        public static string convertToHex(string raw) {
            if (raw.Length == 0)
                return "";
            else return convertToHex(Encoding.ASCII.GetBytes(raw));
        }

        public static string convertToHex(byte[] raw) {
            if (raw == null)
                return "";
            else return ("0x" + BitConverter.ToString(raw).ReplaceAll("-", "").Replace("0x", "").ToLower()).Replace(AppModelConfig.EMPTY_HASH, "0x0");
        }

        public static string convertToHex64(string raw) {
            if (raw == null)
                return "0x0000000000000000000000000000000000000000000000000000000000000000";
            else return (convertToHex(raw) + "0000000000000000000000000000000000000000000000000000000000000000").Substring(0,66);
        }

        // Error message in case the provided transaction parameters were rejected
        public const string TransactionProcessingError = "Parameters Not Acceptable";
        public const string TransactionProcessingErrorMessage = "The transaction parameters provided were rejected by the Blockchain contract.";

        // Error message in case the provided address appears to be not a valid smart contract address
        public const string InvalidContractAddressError = "Invalid Contract Address";
        public const string InvalidContractAddressErrorMessage = "The address provided does not appear to be a valid address of any contract belonging to this ecosystem.";

        // Address validation function and error response information
        public const string AddressError = "Invalid Address";
        public const string AddressErrorMessage = "Contract address provided must be a valid format (starts with '0x'; 42 characters in length; only lower case characters)";
        public static bool validateAddress(string adr, bool isRequired) {
            // If the address provided is null return the result of weather null is allowed or not
            if (isEmptyAdr(adr)) return !isRequired;
            // Needs to start with 0x, is 42 characters in length and only contains 0-9 and a-f lower case characters
            return new Regex(@"0x((([0-9a-f]){40,40}))$").Match(adr).Success;
        }

        // Hash validation function and error response information
        public const string HashError = "Invalid Hash";
        public const string HashErrorMessage = "Hash provided must be a valid format (starts with '0x'; 66 characters in length; only lower case characters)";
        public static bool validateHash(string hash, bool isRequired) {
            // If the address provided is null return the result of weather null is allowed or not
            if (isEmptyHash(hash)) return !isRequired;
            // Needs to start with 0x, is 42 characters in length and only contains 0-9 and a-f lower case characters
            return new Regex(@"0x((([0-9a-f]){64,64}))$").Match(hash).Success;
        }

        // Address validation function and error response information
        public const string PrivateKeyError = "Invalid Private Key";
        public const string PrivateKeyErrorMessage = "Private Key must be a vaild format (64 characters in length; only lower case characters)";
        public static bool validatePrivateKey(string privateKey, bool isRequired) {
            // If the private key provided is null return the result of weather null is allowed or not
            if (isEmptyPrivateKey(privateKey)) return !isRequired;
            // Needs to start with 0x, is 42 characters in length and only contains 0-9 and a-f lower case characters
            return new Regex(@"((([0-9a-f]){64,64}))$").Match(privateKey).Success;
        }
    }

    /// <summary>
    /// Interface that needs to be implemented by LogFile classes to enable the parsing of Ethereum log files
    /// </summary>
    public interface IParseLog {
        void parseLog(FilterLog fl);
    }

    /// <summary>
    /// Class that provides a container for every smart contract's important data such as abi, contract name and its unlinked bytecode
    /// </summary>
    public class ContractAbi {
        public string contractName { get; set; }
        public string abi { get; set; }
        public string bytecode { get; set; }
    }
}
