using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Simple_Bitcoin_Wallet.Bitcoin;
using System.Collections.Generic;
using NBitcoin;

namespace Simple_Bitcoin_Wallet
{
    class TransactionAdapter : RecyclerView.Adapter
    {
        public event EventHandler<TransactionAdapterClickEventArgs> ItemClick;
        public event EventHandler<TransactionAdapterClickEventArgs> ItemLongClick;
        List<TransactionInfo> items;

        public TransactionAdapter(List<TransactionInfo> data)
        {
            items = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            View itemView = LayoutInflater.From(parent.Context).
                        Inflate(Resource.Layout.recycler_transactions, parent, false);

            var vh = new TransactionAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = items[position];
            // Replace the contents of the view with that element
            var holder = viewHolder as TransactionAdapterViewHolder;
            holder.TransactionHash.Text = item.transaction?.GetHash()?.ToString() ?? "Transaction not yet parsed";
            holder.BlockHeight.Text = item.blockHeight?.ToString() ?? "x";
            holder.KeyDerivation.Text = item.deriveDepth.ToString();
            holder.Date.Text = item.Date?.ToString() ?? "UNKNOWN";
        }

        public override int ItemCount => items.Count;

        void OnClick(TransactionAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(TransactionAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class TransactionAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView TransactionHash { get; set; }
        public TextView BlockHeight { get; set; }
        public TextView KeyDerivation { get; set; }
        public TextView Date { get; set; }


        public TransactionAdapterViewHolder(View itemView, Action<TransactionAdapterClickEventArgs> clickListener,
                            Action<TransactionAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            TransactionHash = itemView.FindViewById<TextView>(Resource.Id.tw_tx_hash);
            BlockHeight = itemView.FindViewById<TextView>(Resource.Id.tw_in_block);
            KeyDerivation = itemView.FindViewById<TextView>(Resource.Id.tw_key_derivation);
            Date = itemView.FindViewById<TextView>(Resource.Id.tw_date);

            itemView.Click += (sender, e) => clickListener(new TransactionAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new TransactionAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class TransactionAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}