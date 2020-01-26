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

namespace Simple_Bitcoin_Wallet.Bitcoin
{
    public static class WalletConverter
    {

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
    }
}