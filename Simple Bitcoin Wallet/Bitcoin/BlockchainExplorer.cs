using System;
using System.Collections.Generic;
using System.Linq;
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
    public static class BlockchainExplorer
    {
        /// <summary>
        /// Get block based on provided bloc height
        /// </summary>
        /// <param name="blockHeight"></param>
        /// <returns></returns>
        public static Block GetBlock(uint blockHeight)
        {
            var rpc = WalletHandler.GetRPC();
            return rpc.GetBlock(blockHeight);
        }

        /// <summary>
        /// Get transaction from a block that contains target address
        /// </summary>
        /// <param name="block"></param>
        /// <param name="targetAddress"></param>
        /// <returns>transaction or null if address is not mentioned in block</returns>
        public static Transaction GetTransaction(Block block, BitcoinAddress targetAddress)
        {
            var outScript = targetAddress.ScriptPubKey.ToString();
            return block.Transactions.FirstOrDefault(t => t.ToString().Contains(outScript));
        }

        /// <summary>
        /// Get transaction based on TxId 
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static Transaction GetTransaction(uint256 hash)
        {
            var rpc = WalletHandler.GetRPC();
            return rpc.GetRawTransaction(hash);
        }

        /// <summary>
        /// Get spendable coins from transaction and provided address
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="yourAddresses"></param>
        /// <returns></returns>
        public static List<Coin> GetCoins(Transaction tx, List<BitcoinAddress> yourAddresses)
        {
            var coins = new List<Coin>();

            tx.Outputs.ForEach(output =>
            {
                var value = output.Value;
                var address = output.ScriptPubKey.GetDestinationAddress(Network.TestNet);
                if(yourAddresses.Contains(address))
                {
                    var coin = new Coin(tx, output);
                    coins.Add(coin);
                }
            });

            return coins;
        }
        /// <summary>
        /// Check if provided OutPoint is mentioned in transaction Input
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsInTransactionInput(Transaction tx, OutPoint target)
        {
            foreach (var input in tx.Inputs)
            {
                if (input.PrevOut.Equals(target))
                    return true;
            }

            return false;
        }
    }
}