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
using NBitcoin.RPC;
using Simple_Bitcoin_Wallet.Bitcoin;
using Xamarin.Essentials;

namespace Simple_Bitcoin_Wallet
{
    [Activity(Label = "RecoverWalletActivity")]
    public class RecoverWalletActivity : Activity
    {

        private Button _btnRecoverWallet;
        private Button _btnCancel;
        private Button _btnGoToWallet;

        private EditText _editPass;
        private EditText _editBlock;
        private EditText _editKeys;

        private TextView _status;

        Timer recovery;
        private RPCClient rpc = WalletHandler.GetRPC();

        private MultiAutoCompleteTextView _acMnemonics;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_recover_wallet);
            init();
        }

        /// <summary>
        /// Find elements on form (xml) and init Click events for buttons in form
        /// </summary>
        private void init()
        {
            _btnRecoverWallet = FindViewById<Button>(Resource.Id.btn_recover);
            _btnCancel = FindViewById<Button>(Resource.Id.btn_cancel);
            _btnGoToWallet = FindViewById<Button>(Resource.Id.btn_to_wallet);

            _editPass = FindViewById<EditText>(Resource.Id.edit_password);
            _editBlock = FindViewById<EditText>(Resource.Id.edit_block);
            _editKeys = FindViewById<EditText>(Resource.Id.edit_keys);

            _status = FindViewById<TextView>(Resource.Id.tw_status);

            _acMnemonics = FindViewById<MultiAutoCompleteTextView>(Resource.Id.multi_ac_mnemonics);
            var arrayAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleExpandableListItem1, Wordlist.English.GetWords());
            _acMnemonics.Adapter = arrayAdapter;
            _acMnemonics.Threshold = 1;
            _acMnemonics.SetTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());

            _btnRecoverWallet.Click += (sender, e) => {
                RecoverWallet();
            };

            _btnCancel.Click += (sender, e) => {
                StartActivity(typeof(MainActivity));
            };

            _btnGoToWallet.Click += (sender, e) => {
                StartActivity(typeof(WalletActivity));
            };
        }

        private void RecoverWallet()
        {
            if(!string.IsNullOrEmpty(_editPass.Text) && !string.IsNullOrEmpty(_acMnemonics.Text))
            {
                try
                {
                    var listMnemonics = _acMnemonics.Text.Split(',').ToList();
                    var trimedList = listMnemonics.Select(x => x.Trim()).ToList(); ;
                    string mnemonics = String.Join(" ", trimedList);
                    uint startBlock = Convert.ToUInt32(_editBlock.Text);
                    uint keys = Convert.ToUInt32(_editKeys.Text);
                    UserWalletAccesser.Instance.Wallet = WalletHandler.GenerateWallet(_editPass.Text, mnemonics, startBlock, keys);

                    _btnRecoverWallet.Visibility = ViewStates.Invisible;
                    recovery = new Timer(Recover, null, 100, 20000);
                }
                catch (Exception)
                {
                    Toast.MakeText(ApplicationContext, "Invalid mnemonics", ToastLength.Short).Show();
                }
            }
            else
            {
                Toast.MakeText(ApplicationContext, "Password and mnemonics can't be empty!", ToastLength.Short).Show();
            }
        }

        /// <summary>
        /// Start recover process
        /// </summary>
        /// <param name="state"></param>
        private void Recover(object state)
        {
            var height = rpc.GetBlockCount();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _status.Text = $@" Block parsed: { UserWalletAccesser.Instance.Wallet.ParsedBlockHeight} / {height}";
            });
            if(UserWalletAccesser.Instance.Wallet.ParsedBlockHeight != height)
             UserWalletAccesser.Instance.Wallet.ParseBlocks((uint)height);
            
            if (UserWalletAccesser.Instance.Wallet.ParsedBlockHeight == height)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _btnGoToWallet.Visibility = ViewStates.Visible;
                });
            }
        }
    }
}