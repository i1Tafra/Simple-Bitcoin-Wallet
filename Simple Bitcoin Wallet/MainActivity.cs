using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using Simple_Bitcoin_Wallet.Bitcoin;
using Android.Support.V4.App;
using Android;
using Android.Content.PM;

namespace Simple_Bitcoin_Wallet
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private static readonly int REQUEST_INTERNET = 1;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            RequestPermissions();
            init();
        }

        private void init()
        {
            Button btnExit = FindViewById<Button>(Resource.Id.btn_exit);
            btnExit.Click += (sender, e) =>
            {
                FinishAffinity();
            };

            Button btnWalletNew = FindViewById<Button>(Resource.Id.btn_wallet_new);
            btnWalletNew.Click += (sender, e) =>
            {
                StartActivity(typeof(CreateWalletActivity));
            };
        }

        /// <summary>
        /// Request needed permissions to function
        /// </summary>
        private void RequestPermissions()
        {
            ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.Internet }, REQUEST_INTERNET);
        }

        /// <summary>
        /// Validate that permissions are granted
        /// </summary>
        /// <param name="requestCode"> specifies reuqest</param>
        /// <param name="permissions"></param>
        /// <param name="grantResults">list of permissions with answers (GRANTED or NOT)</param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            // Exit application if INTERNET access is not granted
            //TODO: Maybe make this more user friendly
            if (!(requestCode == REQUEST_INTERNET && (grantResults.Length == 1) && (grantResults[0] == Permission.Granted)))
                FinishAffinity();

        }
    }
}