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
    [Activity(Label = "NewTxActivity")]
    public class NewTxActivity : Activity, IDialogInterfaceOnClickListener, IDialogInterfaceOnMultiChoiceClickListener
    {
        private TextView _twInputValue;

        private EditText _editValue;
        private EditText _editAddress;

        private Button _btnInputCoins;
        private Button _btnGenerateTx;
        private Button _btnViewTx;
        private Button _btnSendTx;
        private Button _btnBack;

        private Transaction outTx;

        private List<Coin> inputCoins = new List<Coin>();

        bool[] itemsChecked = new bool[UserWalletAccesser.Instance.Wallet.Coins.Count];

        public void OnClick(IDialogInterface dialog, int which, bool isChecked)
        {
            itemsChecked[which] = isChecked;
        }

        public void OnClick(IDialogInterface dialog, int which)
        {
            var inputMoney = new Money(0L);
            inputCoins.Clear();
            for (var i = 0; i < itemsChecked.Length; ++i)
            {
                if (itemsChecked[i])
                {
                    var coin = UserWalletAccesser.Instance.Wallet.Coins[i];
                    inputMoney += coin.Amount;
                    inputCoins.Add(coin);
                }
            }

            _twInputValue.Text = $"{inputMoney} BTC";
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_tx_new);
            Init();
        }

        protected override Dialog OnCreateDialog(int id)
        {
            var items = GenerateInputChoise(UserWalletAccesser.Instance.Wallet.Coins);
            return new AlertDialog.Builder(this)
                .SetIcon(Resource.Drawable.notification_template_icon_bg)
                .SetTitle("Choose input coins")
                .SetPositiveButton("Confirm", this)
                .SetMultiChoiceItems(items.ToArray(), itemsChecked, this)
                .Create();
        }

        private List<string> GenerateInputChoise(List<Coin> coins)
        {
            var optionsList = new List<string>();
            coins.ForEach(coin =>
            {
                string option = $"{coin.TxOut.Value} BTC {coin.ScriptPubKey.GetDestinationAddress(Network.TestNet)}";
                optionsList.Add(option);
            });
            return optionsList;
        }

        private void Init()
        {
            _twInputValue = FindViewById<TextView>(Resource.Id.tw_input_coins);

            _editValue = FindViewById<EditText>(Resource.Id.edit_out_value);
            _editAddress = FindViewById<EditText>(Resource.Id.edit_address);

            _btnInputCoins = FindViewById<Button>(Resource.Id.btn_input);
            _btnGenerateTx = FindViewById<Button>(Resource.Id.btn_gen_tx);
            _btnViewTx = FindViewById<Button>(Resource.Id.btn_view_tx);
            _btnSendTx = FindViewById<Button>(Resource.Id.btn_send_tx);
            _btnBack = FindViewById<Button>(Resource.Id.btn_back);

            InitClickEvents();
        }

        private void InitClickEvents()
        {

            _btnInputCoins.Click += (sender, e) => {
                ShowDialog(0);
            };

            _btnGenerateTx.Click += (sender, e) => {
                var value = Convert.ToDecimal(_editValue.Text);
                var address = _editAddress.Text;

                outTx = UserWalletAccesser.Instance.Wallet.CreateTransaction(inputCoins, address, value);

                if (outTx != null)
                {
                    _btnGenerateTx.Visibility = ViewStates.Gone;
                    _btnViewTx.Visibility = ViewStates.Visible;
                    _btnSendTx.Visibility = ViewStates.Visible;
                }

            };

            _btnViewTx.Click += (sender, e) => {
                Toast.MakeText(ApplicationContext, "To be implemented", ToastLength.Short).Show();
            };

            _btnSendTx.Click += (sender, e) => {
                WalletHandler.GetRPC().SendRawTransaction(outTx);
                Toast.MakeText(ApplicationContext, "Complete", ToastLength.Short).Show();
                _btnSendTx.Visibility = ViewStates.Gone;
            };

            _btnBack.Click += (sender, e) => {
                OnBackPressed();
            };
        }
    }
}