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
        public enum TransactionType
        {
            RECEIVING,
            SENDING,
        }
        //TODO: Make ID unique, restore ExtKey on Load level by just password
        /// <summary>
        /// Wallet id is private key encrypted with provided password maybe? 
        /// </summary>
        public string Id { get; set; }

        
        public ExtKey RootKey 
        {
            get =>_rootKey ?? createExtKey(); 
            set => _rootKey = value; 
        }

        /// <summary>
        /// To determine how many keys are used by this account
        /// </summary>
        private uint MaxDeriveDepth { get; set; } = 1;

        /// <summary>
        /// To determine blocks that needs to be parsed in search for UTXO for this wallet
        /// </summary>
        public uint ParsedBlockHeight { get; set; } = 0;

        /// <summary>
        /// List of transaction locations.
        /// uint deriveDepth - determines key location
        /// uint? blockHeight - determines block height where transaction is stored (null if that transaction is yet to be found)
        /// TransactionType type - determines if money is coming to us or going from us :)
        /// </summary>
        private List<(uint deriveDepth, uint? blockHeight, TransactionType type)> TransactionLocations { get; set; } 
            = new List<(uint deriveDepth, uint? blockHeight, TransactionType type)>();

        [NonSerialized]
        private List<(uint deriveDepth, Transaction tx)> transactions = new List<(uint deriveDepth, Transaction tx)>();

        [NonSerialized]
       private ExtKey _rootKey = null;
        /// <summary>
        /// Used to generate master root ExtKey, BitcoinSecret string
        /// </summary>
        private string rootSecret = null;

        /// <summary>
        /// Used to generate master root ExtKey
        /// </summary>
        private byte[] chainCode = null;

        public Wallet(ExtKey rootKey)
        {
            if (rootKey == null)
                throw new NullReferenceException("Master key should not be NULL!");

            chainCode = rootKey.ChainCode;
            rootSecret = rootKey.PrivateKey.GetBitcoinSecret(Network.TestNet).ToString();
            RootKey = rootKey;
        }

        private ExtKey createExtKey()
        {

            var key = new BitcoinSecret(rootSecret).PrivateKey;
            return RootKey = new ExtKey(key, chainCode);
        }
        /// <summary>
        /// To create next address to use in transaction
        /// </summary>
        /// <returns></returns>
        public BitcoinAddress CreateAddress()
        {
            InsertTransactionIndex(TransactionType.RECEIVING);
            var pubKey = RootKey.Neuter().Derive(MaxDeriveDepth++);
            return pubKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.TestNet);
        }

        /// <summary>
        /// Insert new transaction location in list;
        /// </summary>
        /// <param name="type">Receiving or sending transaction, to know where to search</param>
        private void InsertTransactionIndex(TransactionType type)
        {
            TransactionLocations.Add((MaxDeriveDepth, null, type));
        }

        private void updateTransactions()
        {
            TransactionLocations.ForEach(tl => {

                var current_transaction = transactions
                .SingleOrDefault(x => x.deriveDepth == tl.deriveDepth);

                if (current_transaction.tx != null)
                    return;

                if (tl.blockHeight == null) ; //TODO Update tx list
            });
        }

    }
}