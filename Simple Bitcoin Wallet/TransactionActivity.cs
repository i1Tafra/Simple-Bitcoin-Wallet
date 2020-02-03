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
    [Activity(Label = "TransactionActivity")]
    public class TransactionActivity : Activity
    {

        private TextView _twDate;
        private TextView _twHash;
        private TextView _twFee;
        private TextView _twConfirmations;

        private ListView _input;
        private ListView _output;

        private Button _btnBack;

        private TransactionInfo _tx;

        Money _inputValues = new Money(0L);
        Money _outputValues = new Money(0L);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_transaction);

            init();

            if (_tx.transaction == null)
            {
                OnBackPressed();
                return;
            }
            fillForm();
        }

        private void init()
        {
            var index = Intent.GetIntExtra("positon", -1);
            _tx = UserWalletAccesser.Instance.Wallet.TransactionInfo.ElementAt(index);

            _twDate = FindViewById<TextView>(Resource.Id.tw_date);
            _twHash = FindViewById<TextView>(Resource.Id.tw_hash);
            _twFee = FindViewById<TextView>(Resource.Id.tw_fee);
            _twConfirmations = FindViewById<TextView>(Resource.Id.tw_confirmations);

            _btnBack = FindViewById<Button>(Resource.Id.btn_back);

            _input = FindViewById<ListView>(Resource.Id.lw_inputs);
            _output = FindViewById<ListView>(Resource.Id.lw_outputs);

            _btnBack.Click += (sender, e) => {
                OnBackPressed();
            };

        }

        private void fillForm()
        {
            uint blockHeight = (uint)WalletHandler.GetRPC().GetBlockCount();
            _twDate.Text = _tx.Date.ToString();
            _twHash.Text = _tx.transaction.GetHash().ToString();
            _twConfirmations.Text = $"{blockHeight - _tx.blockHeight} in block #{_tx.blockHeight}";

            FillInputs();
            FillOutputs();
            _twFee.Text = $"{_inputValues - _outputValues} BTC";
        }

        private void FillInputs()
        {
            List<string> inputs = new List<string>();
            _tx.transaction.Inputs.ForEach(input =>
            {
                var tx = BlockchainExplorer.GetTransaction(input.PrevOut.Hash);
                var value = tx.Outputs[input.PrevOut.N].Value;
                _inputValues += value;
                var address = input.ScriptSig.GetSignerAddress(Network.TestNet).ToString();
                inputs.Add($"{value} BTC\n{address}");
            });
            _input.Adapter = new ArrayAdapter<string>(this, Resource.Layout.list_item, inputs);
        }

        private void FillOutputs()
        {
            List<string> outputs = new List<string>();
            _tx.transaction.Outputs.ForEach(output =>
            {
                var value = output.Value;
                _outputValues += value;
                var address = output.ScriptPubKey.GetDestinationAddress(Network.TestNet).ToString();
                outputs.Add($"{value} BTC\n{address}");
            });
            _output.Adapter = new ArrayAdapter<string>(this, Resource.Layout.list_item, outputs);
        }

    }
}