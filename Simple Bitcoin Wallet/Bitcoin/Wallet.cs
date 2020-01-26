using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Simple_Bitcoin_Wallet.Bitcoin
{
    [Serializable]
    public class Wallet
    {
        /// <summary>
        /// Wallet id is private key encrypted with provided password
        /// </summary>
        public string Id { get; set; }

        ///TODO: Private Key should be stored encryted [NonSerialized]
        public ExtKey RootKey { private get; set; }

        internal Mnemonic Generate(string password)
        {
            var mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
            RootKey = mnemonic.DeriveExtKey(password);
            Id = GenerateId(password);
            
            return mnemonic;
        }

        private string GenerateId(string password)
        {
            var privateKeyTestNet = RootKey.PrivateKey.GetBitcoinSecret(Network.TestNet);
            var unsecureId = $"{privateKeyTestNet.ToString()}{password}";
            var data = Encoding.ASCII.GetBytes(unsecureId);
            var secureData = new SHA1CryptoServiceProvider().ComputeHash(data);
            return System.Text.Encoding.ASCII.GetString(secureData);
        }

        internal ExtKey Recover(string mnemonics, string password)
        {
            var mnemo = new Mnemonic(mnemonics, Wordlist.English);
            return mnemo.DeriveExtKey(password);
        }
    }
}