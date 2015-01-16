using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace com.xamarin.googleplus.quickstart
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/ic_launcher")]
    public class MainActivity : ListActivity
    {

        static readonly string TAG = typeof(MainActivity).FullName;
        static readonly string[] _examples = { "Google+ Sign In Button 1" };
        ArrayAdapter<string> _menuAdapter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            int errorCode = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(this);

            if (errorCode == ConnectionResult.Success)
            {
                Log.Debug(TAG, "Google Play Services are installed.");
                _menuAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, _examples);
            }
            else
            {
                Toast.MakeText(this, "Google Play Services is not available on this device. Please install and restart", ToastLength.Short).Show();
                _menuAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, new string[0]);

                if (GooglePlayServicesUtil.IsUserRecoverableError(errorCode))
                {
                    Dialog dialog = GooglePlayServicesUtil.GetErrorDialog(errorCode, this, 1111);
                    DialogFragment errorFrag = ErrorDialogFragment.NewInstance(dialog);
                    errorFrag.Show(FragmentManager, "google_play_services_error_dialog");
                }
            }

            ListAdapter = _menuAdapter;
        }

        protected override void OnListItemClick(ListView l, View v, int position, long id)
        {
            base.OnListItemClick(l, v, position, id);

            switch (position)
            {
                case 0:
                    Intent intent = new Intent(this, typeof(SignInButtonExample));
                    StartActivity(intent);
                    break;
                default:
                    Toast.MakeText(this, "Don't know how to handle item " + position, ToastLength.Short).Show();
                    break;
            }
        }
    }
}
