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
using Newtonsoft.Json;

namespace Simple_Bitcoin_Wallet.Bitcoin
{
    public static class WalletHandler
    {
        private const string rpc_username = "student";
        private const string rpc_password = "WYVyF5DTERJASAiIiYGg4UkRH";
        private const string rpc_uri = "http://blockchain.oss.unist.hr:8332";

        /// <summary>
        /// Load wallet from Andorid internal store
        /// TODO:Implement multi wallet support
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        internal static Wallet Load(string password)
        {
            string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "data");
            Directory.CreateDirectory(path);
            ///TODO: Add support for multiple wallets saved (SHA256(ExtKey + password) ?

            IFormatter formatter = new BinaryFormatter();
            using (var stream = new FileStream(Path.Combine(path, "walletTT"), FileMode.Open, FileAccess.Read))
                //walletT -> block 1664606, keys 2
                return (Wallet)formatter.Deserialize(stream);       
        }

        /// <summary>
        /// Save wallet to andorid internal store
        /// TODO:Implement multi wallet support
        /// </summary>
        /// <param name="wallet"></param>
        internal static void Save(Wallet wallet)
        {
            string path = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "data");
            Directory.CreateDirectory(path);
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(Path.Combine(path,"walletTT"), FileMode.Create, FileAccess.Write))//wallet
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
        /// starting blockchain parsing from provided block and using provided number of used keys
        /// </summary>
        /// <param name="password"></param>
        /// <param name="mnemonic"></param>
        /// <param name="blockStart"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        internal static Wallet GenerateWallet(string password, string mnemonic, uint blockStart, uint keys)
        {
            return new Wallet(Recover(password, mnemonic), blockStart, keys);
        }

        /// <summary>
        /// Generate Wallet from mnemonic and password string
        /// </summary>
        /// <param name="password"></param>
        /// <param name="mnemonic"></param>
        /// <returns>new Wallet</returns>
        internal static Wallet GenerateWallet(string password, string mnemonic)
        {
            return new Wallet(Recover(password, mnemonic));
        }

        /// <summary>
        /// ExtKey from mnemonic and password string
        /// </summary>
        /// <param name="password"></param>
        /// <param name="mnemonics"></param>
        /// <returns></returns>
        private static ExtKey Recover(string password, string mnemonics)
        {
            var mnemo = new Mnemonic(mnemonics.ToLower().Trim(), Wordlist.English);
            return mnemo.DeriveExtKey(password);
        }

        /// <summary>
        /// Obtain connected RPC client
        /// </summary>
        /// <returns></returns>
        internal static RPCClient GetRPC()
        {
            var credentials = new RPCCredentialString();
            credentials.UserPassword = new NetworkCredential(rpc_username, rpc_password);
            return new RPCClient(credentials, rpc_uri, Network.TestNet);
        }
    }
}