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
    [Activity(Label = "RecoverWalletActivity")]
    public class RecoverWalletActivity : Activity
    {

        private Button _btnRecoverWallet;
        private Button _btnCancel;

        private EditText _editPass;

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

            _editPass = FindViewById<EditText>(Resource.Id.edit_password);

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
        }

        private void RecoverWallet()
        {
            if(!string.IsNullOrEmpty(_editPass.Text) && !string.IsNullOrEmpty(_acMnemonics.Text))
            {
                try
                {
                    var listMnemonics = _acMnemonics.Text.Split(',').ToList();
                    listMnemonics.ForEach(x => x = x.Trim());
                    string mnemonics = String.Join(" ", listMnemonics);
                    UserWalletAccesser.Instance.Wallet = WalletHandler.GenerateWallet(_editPass.Text, mnemonics);
                    StartActivity(typeof(WalletActivity));
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
    }
}