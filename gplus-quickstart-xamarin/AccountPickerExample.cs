using System;

using Android.App;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.OS;

namespace com.xamarin.googleplus.quickstart
{
    [Activity(Label = "AccountPickerExample")]
    public class AccountPickerExample : Activity, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener
    {
        static readonly string TAG = typeof(AccountPickerExample).FullName;
        IGoogleApiClient _googleApiClient;
        bool _authInProgress = false;

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
        }
    }
}
