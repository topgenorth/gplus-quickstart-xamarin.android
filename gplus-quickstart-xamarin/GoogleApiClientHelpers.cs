using Android.App;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Plus;

namespace com.xamarin.googleplus.quickstart
{
    public static class GoogleApiClientHelpers
    {
        public static readonly int GPS_ERROR_DIALOG_REQUEST_CODE = 1001989;
        public static readonly string GPS_ERROR_DIALOG_FRAGMENT_TAG = "gps_error_dialog_fragment_tag";

        public static bool IsGooglePlayServicesInstalled(this Activity activity)
        {
            int errorCode = GooglePlayServicesUtil.IsGooglePlayServicesAvailable(activity);
            if (errorCode == ConnectionResult.Success)
            {
                return true;
            }
            if (!GooglePlayServicesUtil.IsUserRecoverableError(errorCode))
            {
                return false;
            }

            Dialog dialog = GooglePlayServicesUtil.GetErrorDialog(errorCode, activity, GPS_ERROR_DIALOG_REQUEST_CODE);
            ErrorDialogFragment errorFrag = ErrorDialogFragment.NewInstance(dialog);
            errorFrag.Show(activity.FragmentManager, GPS_ERROR_DIALOG_FRAGMENT_TAG);
            return false;
        }

        public static IGoogleApiClient BuildGoogleApiClient(Activity activity)
        {
            IGoogleApiClientConnectionCallbacks callbacks = (IGoogleApiClientConnectionCallbacks)activity;
            IGoogleApiClientOnConnectionFailedListener errorCallbacks = (IGoogleApiClientOnConnectionFailedListener)activity;
            return new GoogleApiClientBuilder(activity, callbacks, errorCallbacks)
                .AddApi(PlusClass.Api, new PlusClass.PlusOptions.Builder().Build())
                .AddScope(PlusClass.ScopePlusLogin)
                .Build();
        }
    }
}
