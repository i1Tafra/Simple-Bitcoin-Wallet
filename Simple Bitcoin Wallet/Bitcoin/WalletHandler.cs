using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NBitcoin;
using NBitcoin.RPC;

namespace Simple_Bitcoin_Wallet.Bitcoin
{
    public static class WalletHandler
    {
        private static readonly string rpc_username = "student";
        private static readonly string rpc_password = "WYVyF5DTERJASAiIiYGg4UkRH";
        private static readonly string rpc_uri = "http://blockchain.oss.unist.hr:8332";

        public static Wallet Wallet { get; private set; }

        internal static Wallet Load(string password)
        {
            Directory.CreateDirectory("data");
            var data = Encoding.ASCII.GetBytes(password);
            var _partialId = new SHA1CryptoServiceProvider().ComputeHash(data);
            ///TODO: Add support for multiple wallets saved (SHA256(_id + PrivateKey)

            IFormatter formatter = new BinaryFormatter();
            using (var stream = new FileStream(@"data\wallet", FileMode.Open, FileAccess.Read))
                return (Wallet)formatter.Deserialize(stream);       
        }

        internal static void Save(Wallet wallet)
        {
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(@"data\wallet", FileMode.Create, FileAccess.Write))
                formatter.Serialize(stream, wallet);
        }

        /// <summary>
        /// Generate mnemonics from password
        /// </summary>
        /// <param name="password">passowrd to use in mnemonic generation</param>
        /// <returns>mnemonic generated with provided password</returns>
        internal static Mnemonic GenerateMnemonic(string password)
        {
            var mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
            var RootKey = mnemonic.DeriveExtKey(password);
            return mnemonic;
        }

        /// <summary>
        /// Generate Wallet from mnemonic and password string
        /// </summary>
        /// <param name="password"></param>
        /// <param name="mnemonic"></param>
        /// <returns>new Wallet</returns>
        internal static Wallet GenerateWallet(string password, string mnemonic)
        {
            Wallet = new Wallet(Recover(password, mnemonic));
            return Wallet;
        }

        private static string GenerateId(string password, ExtKey rootKey)
        {
            //TODO: Make method that will we able to generate unique ID from user password and PrivateKey
            // It should be possible to load Wallet and decrypt PrivateKey from password
            var privateKeyTestNet = rootKey.PrivateKey.GetBitcoinSecret(Network.TestNet);
            var unsecureId = $"{privateKeyTestNet.ToString()}{password}";
            var data = Encoding.ASCII.GetBytes(unsecureId);
            var secureData = new SHA1CryptoServiceProvider().ComputeHash(data);
            return Encoding.ASCII.GetString(secureData);
        }

        /// <summary>
        /// ExtKey from mnemonic and password string
        /// </summary>
        /// <param name="password"></param>
        /// <param name="mnemonics"></param>
        /// <returns></returns>
        private static ExtKey Recover(string password, string mnemonics)
        {
            var mnemo = new Mnemonic(mnemonics, Wordlist.English);
            return mnemo.DeriveExtKey(password);
        }

        internal static RPCClient GetRPC()
        {
            var credentials = new RPCCredentialString();
            credentials.UserPassword = new NetworkCredential(rpc_username, rpc_password);
            return new RPCClient(credentials, rpc_uri, Network.TestNet);
        }
    }
}