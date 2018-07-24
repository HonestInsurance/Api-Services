/**
 * @description Static class to provide various functions to the Bank-, Bond-, etc. Services class
 * @copyright (c) 2017 HIC Limited (NZBN: 9429043400973)
 * @author Martin Stellnberger
 * @license GPL-3.0
 */

using System;
using System.Net;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using ServiceStack;
using ApiService.ServiceModel;
using Nethereum;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3.Accounts;
using Nethereum.Web3;
using Nethereum.Signer;


namespace ApiService.ServiceInterface
{
    /// <summary>
    /// The static AppServices class provides generic Service functionality and contains environment specific settings.
    /// </summary>
    public static class AppServices
    {
        public static Nethereum.Web3.Web3 web3;
        // Timer to schedule auto ping functionality
        private static System.Timers.Timer pingTimer = new System.Timers.Timer();
        private static string pingTimerContractAdr;
        private static string pingTimerSigningPrivateKey;

        static AppServices() {
            web3 = new Nethereum.Web3.Web3(AppModelConfig.WEB3_URL_ENDPOINT);
        }

        public static HexBigInteger getTransactionCount(string privateKey)
        {
            return web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(EthECKey.GetPublicAddress(privateKey)).Result;
        }

        public static Tuple<BlockParameter, BlockParameter> getBlockParameterConfiguration(ulong requestFromBlock, ulong requestToBlock, bool useDefaultSetting)
        {
            // Create from and to Block parameter for the request
            BlockParameter fromBlock = BlockParameter.CreateEarliest();
            BlockParameter toBlock = BlockParameter.CreateLatest();
            // If from and to block parameters are provided set them
            if (requestFromBlock != 0)
                fromBlock = new BlockParameter(requestFromBlock);
            if (requestToBlock != 0)
                toBlock = new BlockParameter(requestToBlock);
            // If no filter parameter and neither from or to block are supplied set retrieve only the latest log files
            if ((useDefaultSetting == true) && (requestFromBlock == 0) && (requestToBlock == 0)) {
                // Get the current block number
                long currentBlockNr = Convert.ToInt64(AppServices.web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().Result.HexValue, 16);
                // Set the from Block to a value of latest Block minus defaultBlockRangeForEventLogLoading parameter
                fromBlock = new BlockParameter((ulong)Math.Max(0, currentBlockNr - (long)AppModelConfig.defaultBlockRangeForEventLogLoading));
            }
            return Tuple.Create(fromBlock, toBlock);
        }

        // Returns all the contract addresses that belong to this ecosystem
        public static EcosystemContractAddresses GetEcosystemAdr(string contractAdr)
        {
            // Retrieve contract of the INTACCESS interface abi by specifiey the provided address
            var contract = AppServices.web3.Eth.GetContract(AppModelConfig.INTACCESSI.abi, contractAdr);
            try {
                // Retrieve the contract addresses that form the ecosystem for this bond contract
                return contract.GetFunction("getContractAdr").CallDeserializingToObjectAsync<EcosystemContractAddresses>().Result;
            } catch {
                throw new HttpError(
                    HttpStatusCode.NotAcceptable,
                    AppModelConfig.InvalidContractAddressError,
                    AppModelConfig.InvalidContractAddressErrorMessage);
            }
        }
        
        // Creates, Signs and publishes a new transaction
        public static TransactionHash createSignPublishTransaction(string abi, string contractAddress, 
            string signingPrivateKey, string contractFunction, params object []inputParams)
        {
            // Sign the transaction offline first
            string signedTransaction = new TransactionSigner().SignTransaction(
                signingPrivateKey,                  // privateKey
                contractAddress,                    // to
                0,                                  // amount in wei to send with transaction
                web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(EthECKey.GetPublicAddress(signingPrivateKey)).Result,      // nonce value
                AppModelConfig.defaultGasPrice,     // gasPrice
                AppModelConfig.defaultGasLimit,     // gasLimit
                web3.Eth.GetContract(abi, contractAddress).GetFunction(contractFunction).GetData(inputParams)       // data
            );

            // Publish the raw and signed transaction
            try {
                return new TransactionHash { Hash = web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction).Result };
            } catch (Exception exc) {
                // If the transaction was rejected by the blockchain return and throw an HTTP Error
                throw new HttpError(
                    HttpStatusCode.NotAcceptable,
                    AppModelConfig.TransactionProcessingError,
                    AppModelConfig.TransactionProcessingErrorMessage + "   ---   " + exc.Message);
            }

            // ***************************************************************************************
            // Alternative method of publishing transactions by not using offline transaction signing
            //  => Disadvantage of this method is that every transaction creates a new web3 connection
            //  => Advantage of this method is that multiple transactions can be published at the same time (no issues with nonce values)
            // ***************************************************************************************

            // Function contractFunction = 
            //     new Web3(new Account(signingPrivateKey), AppModelConfig.WEB3_URL_ENDPOINT).Eth.GetContract(abi, address).GetFunction(function);

            // // Create, Sign and publish transaction
            // try {
            //     return new TransactionHash {
            //         Hash = contractFunction.SendTransactionAsync(
            //             EthECKey.GetPublicAddress(signingPrivateKey), 
            //             new HexBigInteger(4712388), 
            //             null, 
            //             inputParams
            //             ).Result
            //     };
            // } catch {
            //     // If the transaction was rejected by the blockchain return and throw an HTTP Error
            //     throw new HttpError(
            //         HttpStatusCode.NotAcceptable,
            //         AppModelConfig.TransactionProcessingError,
            //         AppModelConfig.TransactionProcessingErrorMessage);
            // }
        }

        // Creates and deploys the specified contract to the Blockchain
        public static TransactionHash createSignDeployContract(string abi, string linkedBinary, 
            string signingPrivateKey, params object []inputParams)
        {
            try {
                return new TransactionHash {            
                    Hash = new Web3(new Account(signingPrivateKey), AppModelConfig.WEB3_URL_ENDPOINT).Eth.DeployContract.SendRequestAsync(
                        abi,                                                        // Contract ABI
                        linkedBinary,                                               // Linked contract binary
                        EthECKey.GetPublicAddress(signingPrivateKey),               // Public key of transaction signer key used
                        new HexBigInteger(AppModelConfig.defaultGasLimit),          // Gas limit
                        new HexBigInteger(AppModelConfig.defaultGasPrice),          // Gas price
                        new HexBigInteger(0),                                       // Amount in wei sent with deployment
                        inputParams).Result                                         // Deployment parameters
                };

                // Hash = new Web3(new Account(signingPrivateKey), AppModelConfig.WEB3_URL_ENDPOINT).Eth.DeployContract.SendRequestAsync(
                //     abi, 
                //     linkedBinary,
                //     EthECKey.GetPublicAddress(signingPrivateKey),
                //     new HexBigInteger(4712388),             // This is the default block gas limit
                //     null, 
                //     inputParams).Result
                
            } catch (Exception exc) {
                // If the transaction was rejected by the blockchain return and throw an HTTP Error
                throw new HttpError(
                    HttpStatusCode.NotAcceptable,
                    AppModelConfig.TransactionProcessingError,
                    AppModelConfig.TransactionProcessingErrorMessage + "   ---   " + exc.Message);
            }
        }

        public static void configureTimerPing(string timerAdr, string signingPrivateKey, ulong autoSchedulePingDuration)
        {
            // If auto ping duration is set to 0 deactivate auto ping functionality
            if (autoSchedulePingDuration == 0) {
                // Deactivate auto scheduling of ping
                pingTimer.Enabled = false;
                // Remove the saved Timer contract address
                pingTimerContractAdr = "";
                // Remove the saved private key to sign the ping transactions with
                pingTimerSigningPrivateKey = "";
            }
            else {
                // Set the timer interval
                pingTimer.Interval = autoSchedulePingDuration * 1000;
                // Register the elapsed event for the timer.
                pingTimer.Elapsed += OnPingTimerEvent;
                // Have the timer fire repeated events
                pingTimer.AutoReset = true;
                // Enable the timer
                pingTimer.Enabled = true;
                // Save the pingTimerAdr and the signing private key
                pingTimerContractAdr = timerAdr;
                pingTimerSigningPrivateKey = signingPrivateKey;
            }
        }

        public static ulong getPingTimerInterval() {
            return (ulong)pingTimer.Interval / 1000;
        }

        private static void OnPingTimerEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            // When the timer event is fired create a ping transaction
            AppServices.createSignPublishTransaction(
                AppModelConfig.TIMER.abi, 
                pingTimerContractAdr, 
                pingTimerSigningPrivateKey,
                "ping"
            );
        }
    }
}