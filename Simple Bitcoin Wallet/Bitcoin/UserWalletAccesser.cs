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

namespace Simple_Bitcoin_Wallet.Bitcoin
{
    /// <summary>
    /// Singleton class to access and store user wallet across activities when app is running
    /// Also it is responsible for Init wallet if wallet is not initialized
    /// </summary>
    public class UserWalletAccesser
    {
        private UserWalletAccesser()
        {
        }

        private static UserWalletAccesser _instance;
        public static UserWalletAccesser Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserWalletAccesser();
                }
                return _instance;
            }
        }

        public Wallet Wallet
        {
            get => wallet.Init();
            set => wallet = value;
        }

        private Wallet wallet;
    }
}