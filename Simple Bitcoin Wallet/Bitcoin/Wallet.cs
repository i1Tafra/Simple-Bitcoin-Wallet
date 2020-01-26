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
        //TODO: Make ID unique, restore ExtKey on Load level by just password
        /// <summary>
        /// Wallet id is private key encrypted with provided password maybe? 
        /// </summary>
        public string Id { get; set; }

        ///TODO: Private Key should be stored encryted [NonSerialized]
        public ExtKey RootKey { private get; set; }

        public Wallet(ExtKey _rootKey)
        {
            RootKey = _rootKey;
        }

    }
}