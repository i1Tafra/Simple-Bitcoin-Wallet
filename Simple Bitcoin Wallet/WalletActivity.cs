using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using NBitcoin;
using NBitcoin.RPC;
using Simple_Bitcoin_Wallet.Bitcoin;
using Xamarin.Essentials;

namespace Simple_Bitcoin_Wallet
{
    [Activity(Label = "WalletActivity")]
    public class WalletActivity : Activity
    {
        private TextView _twWalletInfo;
        private TextView _twBlockchainInfo;
        private TextView _twAddress;

        private Button _btnGenerateAddress;
        private Button _btnGenerateTransaction;
        private Button _btnSave;

        private RecyclerView _rwTransations;
        private RPCClient rpc = WalletHandler.GetRPC();
        TransactionAdapter transactionAdapter;
        Timer updateWallet;
        int test = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_wallet);
            if (UserWalletAccesser.Instance.Wallet == null)
            {
                OnBackPressed();
                return;
            }

            Init();
        }

        private void Init()
        {
            _twWalletInfo = FindViewById<TextView>(Resource.Id.tw_wallet_info);
            _twBlockchainInfo = FindViewById<TextView>(Resource.Id.tw_blockchain_info);
            _twAddress = FindViewById<TextView>(Resource.Id.tw_address_public);

            _btnGenerateAddress = FindViewById<Button>(Resource.Id.btn_generate_address);
            _btnGenerateTransaction = FindViewById<Button>(Resource.Id.btn_create_transaction);
            _btnSave = FindViewById<Button>(Resource.Id.btn_save);

            _rwTransations = FindViewById<RecyclerView>(Resource.Id.rw_transactions);

            _twWalletInfo.Text = UserWalletAccesser.Instance.Wallet.ToString();

            updateWallet = new Timer(Update, null, 1000, 20000);

            _btnGenerateAddress.Click += (sender, e) => {
               _twAddress.Text = UserWalletAccesser.Instance.Wallet.CreateAddress().ToString();
            };


            _btnSave.Click += (sender, e) => {
                //UserWalletAccesser.Instance.Wallet.TEST();
                WalletHandler.Save(UserWalletAccesser.Instance.Wallet);
            };

            _btnGenerateTransaction.Click += (sender, e) => {
                StartActivity(typeof(NewTxActivity));
            };

            _rwTransations.HasFixedSize = true;
            LinearLayoutManager layoutManager = new LinearLayoutManager(this, LinearLayoutManager.Vertical, false);
            _rwTransations.SetLayoutManager(layoutManager);

            transactionAdapter = new TransactionAdapter(UserWalletAccesser.Instance.Wallet.TransactionInfo);
            _rwTransations.SetAdapter(transactionAdapter);

            transactionAdapter.ItemClick += (sender, e) => {
                var extra = e as TransactionAdapterClickEventArgs;

                var activity = new Intent(this, typeof(TransactionActivity));
                activity.PutExtra("positon", extra.Position);

                StartActivity(activity);
            };
        }

        /// <summary>
        /// Update wallet if needed and blockchain
        /// </summary>
        /// <param name="state"></param>
        private void Update(object state)
        {
            try
            {
                var height = rpc.GetBlockCount();
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _twWalletInfo.Text = UserWalletAccesser.Instance.Wallet.ToString();
                    _twBlockchainInfo.Text = $" Current height: {height} times called: {++test}";
                });
                UserWalletAccesser.Instance.Wallet.ParseBlocks((uint)height);
                transactionAdapter.NotifyItemRangeChanged(0, UserWalletAccesser.Instance.Wallet.TransactionInfo.Count);
                //transactionAdapter.NotifyItemChanged(0);
            }
            catch (Exception)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _twWalletInfo.Text = UserWalletAccesser.Instance.Wallet.ToString();
                    _twBlockchainInfo.Text = $" Current height: FAILED times called: {++test}";
                });
            }
        }

    }
}