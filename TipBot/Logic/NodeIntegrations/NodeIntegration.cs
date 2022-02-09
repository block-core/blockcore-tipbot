﻿using NBitcoin;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TipBot.Database;
using TipBot.Logic.NodeIntegrations.Models;

namespace TipBot.Logic.NodeIntegrations
{
    public interface INodeIntegration : IDisposable
    {
        /// <summary>Initializes the class.</summary>
        /// <remarks>Generates addresses and loads them to <see cref="BotDbContext.UnusedAddresses"/> if <see cref="BotDbContext.UnusedAddresses"/> is empty.</remarks>
        void Initialize();

        /// <summary>Withdraws specified amount or money to specified address.</summary>
        /// <remarks>Address will be validated prior to withdrawal.</remarks>
        /// <remarks>The fee will be taken out of this amount.</remarks>
        BuildTransactionResult Withdraw(decimal amount, string address);
    }

    public class InvalidAddressException : Exception
    {
        public InvalidAddressException() : base("Address specified is invalid.") { }
    }

    public class WalletModel
    {
        public string Mnemonic { get; set; }

        public string Password { get; set; }

        public string Passphrase { get; set; } = string.Empty;

        public string Name { get; set; }

        public DateTime CreationDate { get; set; }
    }

    public class BlockCoreNodeAPI
    {
        private string ApiUrl { get; set; }
        private string WalletName { get; set; }
        private string WalletPassword { get; set; }
        private string AccountName { get; set; }
        private decimal MinFee { get; set; }
        private bool UseSegwit { get; set; }

        private readonly TipBotSettings settings;

        public BlockCoreNodeAPI(TipBotSettings settings, string accountName)
        {
            this.settings = settings;

            ApiUrl = settings.ApiUrl;
            WalletName = settings.WalletName;
            WalletPassword = settings.WalletPassword;
            AccountName = accountName;
            MinFee = settings.NetworkFee;
            UseSegwit = settings.UseSegwit;
        }

        public async Task<ValidateAddressResult> ValidateAddress(string address)
        {
            var result = new ValidateAddressResult();
            var client = new RestClient($"{ApiUrl}");
            var request = new RestRequest("/api/Node/validateaddress", Method.Get);
            request.AddParameter("address", address);

            var response = await client.ExecuteAsync<ValidateAddressResult>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = response.Data;
            }
            return result;
        }

        public async Task<BuildTransactionResult> SendTo(string address, decimal amount)
        {
            var buildResult = await BuildTransaction(address, Money.FromUnit(amount, MoneyUnit.BTC));
            if (buildResult == null)
            {
                throw new Exception("There was an issue building a transaction.");
            }
            else
            {
                await SendTransaction(buildResult.Hex);
            }
            return buildResult;
        }

        public async Task<BuildTransactionResult> BuildTransaction(string address, Money amount)
        {
            var result = new BuildTransactionResult();
            var recipient = new Recipient()
            {
                amount = amount.ToString(),
                destinationAddress = address
            };
            var recipients = new List<Recipient>
            {
                recipient
            };

            var newTransaction = new BuildTransactionRequest()
            {
                accountName = AccountName,
                allowUnconfirmed = true,
                feeAmount = MinFee.ToString(),
                password = WalletPassword,
                walletName = WalletName,
                recipients = recipients,
                segwitChangeAddress = UseSegwit
            };

            var client = new RestClient(ApiUrl);
            var request = new RestRequest("/api/Wallet/build-transaction", Method.Post);
            var transactionToSend = JsonConvert.SerializeObject(newTransaction);
            request.AddParameter("application/json; charset=utf-8", transactionToSend, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;

            var response = await client.ExecuteAsync<BuildTransactionResult>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = response.Data;
            }
            else
            {
                result = null;
            }
            return result;
        }

        public async Task<bool> SendTransaction(string transactionHex)
        {
            var result = false;
            var sendTransactionRequest = new SendTransactionRequest()
            {
                hex = transactionHex
            };

            var client = new RestClient(ApiUrl);
            var request = new RestRequest("/api/Wallet/send-transaction", Method.Post);
            var transactionToSend = JsonConvert.SerializeObject(sendTransactionRequest);
            request.AddParameter("application/json; charset=utf-8", transactionToSend, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;

            var response = await client.ExecuteAsync<BuildTransactionResult>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = true;
            }
            return result;
        }

        public async Task<List<string>> GetUnusedAddresses(uint count)
        {
            var result = new List<string>();
            var client = new RestClient($"{ApiUrl}");
            var request = new RestRequest("/api/Wallet/unusedaddresses", Method.Get);
            request.AddParameter("Count", count);
            request.AddParameter("Segwit", UseSegwit);
            request.AddParameter("WalletName", WalletName);
            request.AddParameter("AccountName", AccountName);

            var response = await client.ExecuteAsync<List<string>>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = response.Data;
            }
            return result;
        }

        public async Task<bool> CreateWallet()
        {
            var client = new RestClient($"{ApiUrl}");
            var request = new RestRequest("/api/Wallet/recover", Method.Post);

            var body = new WalletModel { 
                Name = settings.WalletName,
                Password = settings.WalletPassword,
                Mnemonic = settings.WalletRecoveryPhrase,
                CreationDate = DateTime.UtcNow
            };

            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);

            return (response.StatusCode == HttpStatusCode.OK);
        }

        public async Task<bool> LoadWallet()
        {
            var client = new RestClient($"{ApiUrl}");
            var request = new RestRequest("/api/Wallet/load", Method.Post);

            var body = new WalletModel
            {
                Name = settings.WalletName,
                Password = settings.WalletPassword
            };

            request.AddJsonBody(body);

            var response = await client.ExecuteAsync(request);

            return (response.StatusCode == HttpStatusCode.OK);
        }

        public async Task<GetNodeStatusResult> GetNodeStatus()
        {
            var result = new GetNodeStatusResult();
            var client = new RestClient($"{ApiUrl}");
            var request = new RestRequest("/api/Node/status", Method.Get);

            var response = await client.ExecuteAsync<GetNodeStatusResult>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = response.Data;
            }
            return result;
        }

        public async Task<GetAddressBalanceResult> GetAddressesBalances(string addresses, int minConfirmations)
        {
            var result = new GetAddressBalanceResult();
            var client = new RestClient($"{ApiUrl}");
            var request = new RestRequest("/api/BlockStore/getaddressesbalances", Method.Get);
            request.AddParameter("addresses", addresses);
            request.AddParameter("minConfirmations", minConfirmations);

            var response = await client.ExecuteAsync<GetAddressBalanceResult>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = response.Data;
            }
            return result;
        }

        public async Task<GetAddressesHistory> GetAddressesHistory(string addresses)
        {
            var result = new GetAddressesHistory();
            var client = new RestClient($"{ApiUrl}");
            var request = new RestRequest("/api/Wallet/history", Method.Get);
            request.AddParameter("WalletName", WalletName);
            request.AddParameter("AccountName", AccountName);
            request.AddParameter("Address", addresses);

            var response = await client.ExecuteAsync<GetAddressesHistory>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = response.Data;
            }
            return result;
        }
    }
}
