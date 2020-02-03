using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        /// Not needed remove from all locations
        /// </summary>
        public enum TransactionType
        {
            RECEIVING,
            SENDING,
        }

        //TODO: 
        /// <summary>
        /// Wallet id should be SHA256(private key + password)
        /// Not yet implemented
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ExtKey could not be serialied that's why it is created 
        /// from rootSecret and chainCode
        /// </summary>
        public ExtKey RootKey 
        {
            get =>_rootKey ?? createExtKey(); 
            set => _rootKey = value; 
        }

        /// <summary>
        /// What derivation will be used in next address
        /// </summary>
        public uint NextDeriveDepth { get; private set; } = 1;

        /// <summary>
        /// Current key depth that is used
        /// </summary>
        public uint CurrentDeriveDepth { get => NextDeriveDepth - 1; }

        /// <summary>
        /// To determine blocks that needs to be parsed in search for UTXO and tansactions for this wallet
        /// </summary>
        public uint ParsedBlockHeight { get; set; } = 0;

        /// <summary>
        /// List of transaction locations and confirmed transaction from node
        /// uint deriveDepth - determines PubKey Hash
        /// uint? blockHeight - determines Block where transaction is located (null if that transaction is yet to be found)
        /// TransactionType type - Not used, should be deleted
        /// </summary>
        internal List<TransactionInfo> TransactionInfo
        {
            get => _transactionInfo ?? createTransactionInfoList();
        }

        private List<TransactionInfo> _transactionInfo;

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

        /// <summary>
        /// Used to determine if Wallet is initialized. 
        /// Transactions that knows its block will be downloaded before loading a wallet
        /// </summary>
        [NonSerialized]
        private bool walletInitialized = false;

        /// <summary>
        /// Determines if block parsing from a node is currently ongoing
        /// </summary>
        [NonSerialized]
        private bool parsing = false;

        /// <summary>
        /// Coins that we are able to spend in new transactions
        /// </summary>
        public List<Coin> Coins { get => _coins ?? createCoinList(); }

        /// <summary>
        /// Used to init empty coin list
        /// </summary>
        /// <returns></returns>
        private List<Coin> createCoinList()
        {
           return  _coins = new List<Coin>();
        }

        [NonSerialized]
        private List<Coin> _coins;

        /// <summary>
        /// Used to obtain all PubKey address that are used by wallet and have at least one confirmation
        /// </summary>
        public List<BitcoinAddress> PublicAddresses 
        { get
            {
                var publicAddresses = new List<BitcoinAddress>();
                TransactionInfo.ForEach(txInfo =>
                {
                    if (txInfo.transaction != null)
                    {
                        var address = GetAddress(txInfo.deriveDepth);
                        publicAddresses.Add(address);
                    }
                });
                return publicAddresses;
            } 
        }

        /// <summary>
        /// Create new transaction based on provided inputs
        /// </summary>
        /// <param name="coins">what input to use</param>
        /// <param name="destination">location to send</param>
        /// <param name="value">how many BTC to send to a location</param>
        /// <returns>Transaction if valid, otherwise null</returns>
        public Transaction CreateTransaction(List<Coin> coins, string destination, decimal value)
        {
            
            var publicKeyHash = BitcoinAddress.Create(destination, Network.TestNet);
            var keys = KeysForCoins(coins);
            var txBuilder = Network.TestNet.CreateTransactionBuilder();
            var tx = txBuilder
                .AddCoins(coins)
                .AddKeys(keys.ToArray())
                .Send(publicKeyHash, new Money(value,MoneyUnit.BTC))
                .SendFees(new Money(300, MoneyUnit.Satoshi))
                .SetChange(GetAddress(NextDeriveDepth++))
                .BuildTransaction(true);
            if(txBuilder.Verify(tx))
                return tx;

            return null;
        }

        /// <summary>
        /// Obtain keys for unlocking provided coins
        /// </summary>
        /// <param name="coins"></param>
        /// <returns></returns>
        private List<ExtKey> KeysForCoins(List<Coin> coins)
        {
            var keys = new List<ExtKey>();
            coins.ForEach(coin =>
            {
                TransactionInfo.ForEach(txInfo =>
                {
                    var tx = txInfo.transaction;
                    if (tx != null)
                    {
                        tx.Outputs.ForEach(output =>
                        {
                            if (output.Equals(coin.TxOut))
                                keys.Add(GetKey(txInfo.deriveDepth));
                        });
                    }
                });
            });
            return keys;
        }

        /// <summary>
        /// Parse Coins available to spend in new transactions
        /// </summary>
        private void ParseCoins()
        {
            Coins.Clear();

            TransactionInfo.ForEach(txInfo =>
            {
                if (txInfo.transaction != null)
                {
                    var txCoins = BlockchainExplorer.GetCoins(txInfo.transaction, PublicAddresses);
                    Coins.AddRange(txCoins);
                }
            });
            RemoveUsedCoins();
        }

        /// <summary>
        /// Remove Coins that are already used in another transaction
        /// </summary>
        private void RemoveUsedCoins()
        {
            var coinsToRemove = new List<Coin>();

            TransactionInfo.ForEach(txInfo =>
            {
                if (txInfo.transaction != null)
                {
                    Coins.ForEach(coin =>
                    {
                        if (BlockchainExplorer.IsInTransactionInput(txInfo.transaction, coin.Outpoint))
                            coinsToRemove.Add(coin);
                    });
                }
            });
            coinsToRemove = coinsToRemove.Distinct().ToList();
            Coins.RemoveAll( coin => 
            {
                return coinsToRemove.Contains(coin);
            });
        }

        /// <summary>
        /// Create wallet for recovery
        /// </summary>
        /// <param name="rootKey"></param>
        /// <param name="startBlock">from this block parsing will start</param>
        /// <param name="keysUsed">Wallet will consider this number of keys as already used and confirmed in blockchain</param>
        public Wallet(ExtKey rootKey, uint startBlock, uint keysUsed)
        {
            if (rootKey == null)
                throw new NullReferenceException("Master key should not be NULL!");

            rootSecret = rootKey.PrivateKey.GetBitcoinSecret(Network.TestNet).ToString();
            chainCode = rootKey.ChainCode;
            RootKey = rootKey;

            ParsedBlockHeight = startBlock;
            NextDeriveDepth += keysUsed;
        }

        /// <summary>
        /// Create wallet from root seed
        /// </summary>
        /// <param name="rootKey"></param>
        public Wallet(ExtKey rootKey)
        {
            if (rootKey == null)
                throw new NullReferenceException("Master key should not be NULL!");

            rootSecret = rootKey.PrivateKey.GetBitcoinSecret(Network.TestNet).ToString();
            chainCode = rootKey.ChainCode;
            RootKey = rootKey;
        }

        /// <summary>
        /// Download transactions that block id is known and parse awailable coins from them
        /// Should be done before first accessing a wallet on loading
        /// </summary>
        /// <returns></returns>
        public Wallet Init()
        {
            if (walletInitialized)
                return this;

            updateTransactions();
            ParseCoins();

            walletInitialized = true;
            return this;
        }

        /// <summary>
        /// Init empty TransactionInfo list
        /// </summary>
        /// <returns></returns>
        private List<TransactionInfo> createTransactionInfoList()
        {
            return _transactionInfo = new List<TransactionInfo>();
        }

        /// <summary>
        /// Create root seed from rootSecred nad chainCode
        /// </summary>
        /// <returns></returns>
        private ExtKey createExtKey()
        {
            var key = new BitcoinSecret(rootSecret).PrivateKey;
            return RootKey = new ExtKey(key, chainCode);
        }
        /// <summary>
        /// Create next address to use in transaction
        /// </summary>
        /// <returns></returns>
        public BitcoinAddress CreateAddress()
        {
            InsertTransactionIndex(TransactionType.RECEIVING);
            return GetAddress(NextDeriveDepth++);
        }

        /// <summary>
        /// Obtain address on specified index
        /// </summary>
        /// <returns></returns>
        public BitcoinAddress GetAddress(uint deriveDepth)
        {
            var pubKey = RootKey.Neuter().Derive(deriveDepth);
            return pubKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.TestNet);
        }

        /// <summary>
        /// Obtain address on specified index
        /// </summary>
        /// <returns></returns>
        private ExtKey GetKey(uint deriveDepth)
        {
            return RootKey.Derive(deriveDepth);
        }

        /// <summary>
        /// Insert new transaction location in list;
        /// </summary>
        /// <param name="type">Receiving or sending transaction, to know where to search</param>
        private void InsertTransactionIndex(TransactionType type)
        {
            TransactionInfo.Add(new TransactionInfo(NextDeriveDepth, null, type, null));
        }

        /// <summary>
        /// Download transactions that block location is known
        /// </summary>
        private void updateTransactions()
        {
            if (TransactionInfo.Count != CurrentDeriveDepth)
                InsertTransactions();

            TransactionInfo.ForEach(txInfo => {

                if (txInfo.transaction != null)
                    return;

                if (txInfo.blockHeight != null)
                    txInfo.transaction = getTransaction(txInfo.blockHeight, txInfo.deriveDepth);
            });
        }

        /// <summary>
        /// Insert not found transaction based on DeriveDepth
        /// </summary>
        private void InsertTransactions()
        {
            for(var i = TransactionInfo.Count + 1; i < NextDeriveDepth; i++)
            {
                TransactionInfo.Add(new TransactionInfo((uint)i, null, TransactionType.RECEIVING, null));
            }
        }

        /// <summary>
        /// Parse blocks on blockchain to locate TxOut and Transactions from wallet
        /// </summary>
        /// <param name="currentMaxHeight"></param>
        public void ParseBlocks(uint currentMaxHeight)
        {
            if (parsing || currentMaxHeight <= ParsedBlockHeight)
                return;

            parsing = true;

            for (; ParsedBlockHeight < currentMaxHeight; ParsedBlockHeight++)
            {
                var block = BlockchainExplorer.GetBlock(ParsedBlockHeight);
                TransactionInfo.ForEach(txInfo =>
                {
                    if (txInfo.transaction == null)
                    {
                        var address = GetAddress(txInfo.deriveDepth);
                        txInfo.transaction = BlockchainExplorer.GetTransaction(block, address);
                        if (txInfo.transaction != null)
                        { 
                        txInfo.blockHeight = ParsedBlockHeight;
                        txInfo.Date = block.Header.BlockTime.UtcDateTime;
                        }
                    }
                });
            }
            ParseCoins();
            parsing = false;
        }

        /// <summary>
        /// Parse blocks on blockchain to locate TxOut and Transactions from wallet
        /// </summary>
        /// <param name="currentMaxHeight"></param>
        public void ParseBlocksParallel(uint currentMaxHeight)
        {
            if (parsing || currentMaxHeight <= ParsedBlockHeight)
                return;

            parsing = true;

            Parallel.For(ParsedBlockHeight, currentMaxHeight,
              index => {
                  var block = BlockchainExplorer.GetBlock(ParsedBlockHeight);
                  TransactionInfo.ForEach(txInfo =>
                  {
                      if (txInfo.transaction == null)
                      {
                          var address = GetAddress(txInfo.deriveDepth);
                          txInfo.transaction = BlockchainExplorer.GetTransaction(block, address);
                          if (txInfo.transaction != null)
                          {
                              txInfo.blockHeight = ParsedBlockHeight = (uint)index;
                              txInfo.Date = block.Header.BlockTime.UtcDateTime;
                          }
                      }
                  });
              });

            ParseCoins();
            parsing = false;
        }

        /// <summary>
        /// Get transaction if key from deriveDepth is located as outputa
        /// </summary>
        /// <param name="blockHeight"></param>
        /// <param name="deriveDepth"></param>
        /// <returns></returns>
        private Transaction getTransaction(uint? blockHeight, uint deriveDepth)
        {
            var block = BlockchainExplorer.GetBlock((uint)blockHeight);
            var address = GetAddress(deriveDepth);
            return BlockchainExplorer.GetTransaction(block, address);
        }

        /// <summary>
        /// Get number of transactionInfos that have Transaction confirmed from blockchain
        /// </summary>
        /// <returns></returns>
        private int TransactionInfoWithTransactions()
        {
            return TransactionInfo.Count(t => t.transaction != null);
        }

        /// <summary>
        /// Calculate available balance in wallet
        /// </summary>
        /// <returns></returns>
        private Money CalcualteBalance()
        {
            var totalValue = new Money(0);

            Coins.ForEach(coin =>
            {
                totalValue += coin.TxOut.Value;
            });
            return totalValue;
        }

        /// <summary>
        /// Display information about wallet
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $@"Parsed blocks: {ParsedBlockHeight}
Derived keys: {NextDeriveDepth}
Transactions {TransactionInfo.Count}
Transactions with Tx {TransactionInfoWithTransactions()}
Balance {CalcualteBalance()} BTC
is parsing blockchain: {parsing}";
        }
    }

    [Serializable]
    internal class TransactionInfo
    {
        public uint deriveDepth;
        public uint? blockHeight;
        public Wallet.TransactionType type;
        public DateTime? Date;
        [NonSerialized]
        public Transaction transaction;

        public TransactionInfo(uint deriveDepth, uint? blockHeight, Wallet.TransactionType type, Transaction transaction)
        {
            this.deriveDepth = deriveDepth;
            this.blockHeight = blockHeight;
            this.type = type;
            this.transaction = transaction;
        }

        public override bool Equals(object obj)
        {
            return obj is TransactionInfo other &&
                   deriveDepth == other.deriveDepth &&
                   blockHeight == other.blockHeight &&
                   type == other.type;
        }
    }
}