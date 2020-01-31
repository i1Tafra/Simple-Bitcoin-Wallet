using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NBitcoin;
using Simple_Bitcoin_Wallet.Bitcoin;
using Xamarin.Essentials;

namespace Simple_Bitcoin_Wallet
{
    [Activity(Label = "WalletActivity")]
    public class WalletActivity : Activity
    {
        private TextView _twDemo;
        private TextView _twDemo2;
        Timer blockHeight;
        int test = 0;
        int count = 0;
        string target;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_wallet);
            if (UserWalletAccesser.Instance.Wallet == null)
                StartActivity(typeof(MainActivity));

            init();
        }

        private void init()
        {
            _twDemo = FindViewById<TextView>(Resource.Id.tw_demo);
            _twDemo2 = FindViewById<TextView>(Resource.Id.tw_demo2);

            _twDemo.Text = target = UserWalletAccesser.Instance.Wallet.CreateAddress().ScriptPubKey.ToString();
            _twDemo2.Text = UserWalletAccesser.Instance.Wallet.ParsedBlockHeight.ToString();
            blockHeight = new Timer(logBlock, null, 5000, 30000);
        }

        private void logBlock(object state)
        {
            var rpc = WalletHandler.GetRPC();
            var block = rpc.GetBlock(1664411);
            
            block.Transactions.ForEach(t => ParseTransaction(t, target));
            var height = rpc.GetBlockCount();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _twDemo2.Text = $"{++test} | {height} | {block}";
            });
        }

        private void ParseTransaction(Transaction transaction, string target)
        {
            if (transaction.ToString().Contains(target))
                Console.WriteLine("IMAM GA");
            Console.WriteLine($"{count++} TR: {transaction.GetHash()}");
            if (count == 222)
            {
                if(transaction.ToString().Contains(target))
                    Console.WriteLine("IMAM GA");
                else
                    Console.WriteLine("STA JE OVO?");
            }
        }
    }
}