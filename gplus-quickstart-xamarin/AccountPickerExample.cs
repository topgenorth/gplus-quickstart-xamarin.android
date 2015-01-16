using System;

using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.OS;

namespace com.xamarin.googleplus.quickstart
{
    [Activity(Label = "AccountPickerExample")]
    public class AccountPickerExample : Activity, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener
    {
        static readonly string TAG = typeof(AccountPickerExample).FullName;
        static readonly string AUTH_IN_PROGRESS_STATE = "gps_auth_in_progress";

        IGoogleApiClient _googleApiClient;
        bool _authInProgress ;

        public void OnConnected(Bundle connectionHint)
        {
            throw new NotImplementedException();
        }

        public void OnConnectionSuspended(int cause)
        {
            throw new NotImplementedException();
        }

        void IGooglePlayServicesClientOnConnectionFailedListener.OnConnectionFailed(ConnectionResult result)
        {
            throw new NotImplementedException();
        }

        void IGoogleApiClientOnConnectionFailedListener.OnConnectionFailed(ConnectionResult result)
        {
            throw new NotImplementedException();
        }


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (bundle != null)
            {
                _authInProgress = bundle.GetBoolean(AUTH_IN_PROGRESS_STATE, false);
            }

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }
    }
}
