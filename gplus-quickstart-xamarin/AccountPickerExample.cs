using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Util;

namespace com.xamarin.googleplus.quickstart
{
    [Activity(Label = "@string/activity_account_picker_example")]
    public class AccountPickerExample : Activity, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener
    {
        static readonly string TAG = typeof(AccountPickerExample).FullName;
        static readonly string AUTH_IN_PROGRESS_STATE = "gps_auth_in_progress";
        static readonly int GOOGLE_PLAY_SERVICES_REQUEST_AUTHORIZATION = 1001900;
        bool _authInProgress;
        IGoogleApiClient _googleApiClient;

        public void OnConnected(Bundle connectionHint)
        {
            Log.Debug(TAG, "Connected to the Google+ API");
        }

        public void OnConnectionSuspended(int cause)
        {
            Log.Debug(TAG, "Connection to the Google+ API suspected: {0}. Trying to reconnect...", cause);
            _googleApiClient.Connect();
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            Log.Error(TAG, "Connect to Google API failed: {0}.", result.ErrorCode);
            if (_authInProgress)
            {
                return;
            }
            if (!result.HasResolution)
            {
                return;
            }

            try
            {
                _authInProgress = true;
                result.StartResolutionForResult(this, GOOGLE_PLAY_SERVICES_REQUEST_AUTHORIZATION);
            }
            catch (IntentSender.SendIntentException e)
            {
                _authInProgress = false;
                Log.Error(TAG, "Could not resolve Google Play Services error: {0}", e.LocalizedMessage);
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            if ((_googleApiClient != null) && (_googleApiClient.IsConnected))
            {
                _googleApiClient.Disconnect();
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (bundle != null)
            {
                _authInProgress = bundle.GetBoolean(AUTH_IN_PROGRESS_STATE, false);
            }
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutBoolean(AUTH_IN_PROGRESS_STATE, _authInProgress);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            _authInProgress = savedInstanceState.GetBoolean(AUTH_IN_PROGRESS_STATE, false);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == GOOGLE_PLAY_SERVICES_REQUEST_AUTHORIZATION)
            {
                _authInProgress = false;
                if (resultCode == Result.Ok)
                {
                    ConnectToGoogleApi();
                }
                else
                {
                    Log.Warn(TAG, "Google+ API authentication was cancelled.");
                }
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        void ConnectToGoogleApi(bool connect = true)
        {
            if (_googleApiClient == null)
            {
                return;
            }
            if ((!_googleApiClient.IsConnected) || (_googleApiClient.IsConnecting))
            {
                return;
            }

            if (connect)
            {
                _googleApiClient.Connect();
            }
            else
            {
                _googleApiClient.Disconnect();
            }
        }
    }
}
