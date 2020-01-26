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
using Simple_Bitcoin_Wallet.Bitcoin;

namespace Simple_Bitcoin_Wallet
{
    [Activity(Label = "CreateWalletActivity")]
    public class CreateWalletActivity : Activity
    {

        private Mnemonic _generated_mnemonic;
        private Button _btnGenMnemonics;
        private Button _btnRepeatMnemonics;
        private Button _btnGenerateWallet;
        private Button _btnCancel;

        private TextView _twMnemonicList;
        private TextView _twMnemonicWarning;

        private EditText _editMnenonicsList;
        private EditText _editPass;
        private EditText _editPassRepeat;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_create_wallet);

            init();
        }

        /// <summary>
        /// Find elements on form (xml) and init Click events for buttons in form
        /// </summary>
        private void init()
        {
            _btnGenMnemonics = FindViewById<Button>(Resource.Id.btn_generate);
            _btnRepeatMnemonics = FindViewById<Button>(Resource.Id.btn_repeat_mnemonic);
            _btnGenerateWallet = FindViewById<Button>(Resource.Id.btn_generate_wallet);
            _btnCancel = FindViewById<Button>(Resource.Id.btn_cancel);

            _twMnemonicList = FindViewById<TextView>(Resource.Id.tw_mnemonic);
            _twMnemonicWarning = FindViewById<TextView>(Resource.Id.tw_mnemonic_warning);

            _editMnenonicsList = FindViewById<EditText>(Resource.Id.edit_mnemonics);
            _editPass = FindViewById<EditText>(Resource.Id.edit_password);
            _editPassRepeat = FindViewById<EditText>(Resource.Id.edit_password_repeat);

            _btnGenMnemonics.Click += (sender, e) => {
                GenerateMnemonics();
            };

            _btnRepeatMnemonics.Click += (sender, e) => {
                _twMnemonicList.Visibility = ViewStates.Invisible;
                _btnRepeatMnemonics.Visibility = ViewStates.Invisible;
                _twMnemonicWarning.Visibility = ViewStates.Invisible;
                _editMnenonicsList.Visibility = ViewStates.Visible;
                _btnGenerateWallet.Visibility = ViewStates.Visible;
            };

            _btnGenerateWallet.Click += (sender, e) => {
                GenerateWallet();
            };

            _btnCancel.Click += (sender, e) => {
                StartActivity(typeof(MainActivity));
            };

            //TODO: Remove, just for connection testing
            var rpcClient = WalletHandler.GetRPC();
            Toast.MakeText(ApplicationContext, $"Block: {rpcClient.GetBlockCount()}!", ToastLength.Short).Show();
        }

        /// <summary>
        /// Generate mnemonics from provdied password, if password is not empty and valid
        /// </summary>
        private void GenerateMnemonics()
        {
            if (!string.IsNullOrEmpty(_editPass.Text) && _editPass.Text.Equals(_editPassRepeat.Text))
            {
                _btnRepeatMnemonics.Visibility = ViewStates.Visible;
                _btnGenMnemonics.Visibility = ViewStates.Invisible;
                _editPass.Enabled = false;
                _editPassRepeat.Enabled = false;

                _generated_mnemonic = WalletHandler.GenerateMnemonic(_editPass.Text);
                _twMnemonicList.Text = _generated_mnemonic.ToString();
            }
            else
            {
                Toast.MakeText(ApplicationContext, "Password does not match or empty!", ToastLength.Short).Show();
            }
        }

        /// <summary>
        /// Generate wallet from provided password and repeated mnemonics
        /// </summary>
        private void GenerateWallet()
        {
            if (_editMnenonicsList.Text.Equals(_generated_mnemonic.ToString()))
            {
                WalletHandler.GenerateWallet(_editPass.Text, _editMnenonicsList.Text);
                //TODO: Go to wallet activity page
            }
            else
            {
                Toast.MakeText(ApplicationContext, "Mnemonics does not match!", ToastLength.Short).Show();
            }
        }
    }
}