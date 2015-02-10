using Android.App;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Plus;
using Android.Content;
using Android.Preferences;
using System;

namespace com.xamarin.googleplus.quickstart
{
    public static class GoogleApiClientHelpers
    {
        public static readonly int GPS_ERROR_DIALOG_REQUEST_CODE = 1001989;
        public static readonly string GPS_ERROR_DIALOG_FRAGMENT_TAG = "gps_error_dialog_fragment_tag";

		static readonly string ACCOUNT_NAME = "google_account_name";
		static  string _currentEmail;
		static bool _isLoggedIn = false;

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

        public static IGoogleApiClient BuildGoogleApiClient(this Activity activity)
        {
            IGoogleApiClientConnectionCallbacks callbacks = (IGoogleApiClientConnectionCallbacks)activity;
            IGoogleApiClientOnConnectionFailedListener errorCallbacks = (IGoogleApiClientOnConnectionFailedListener)activity;
            return new GoogleApiClientBuilder(activity, callbacks, errorCallbacks)
                .AddApi(PlusClass.Api, new PlusClass.PlusOptions.Builder().Build())
                .AddScope(PlusClass.ScopePlusLogin)
                .Build();
        }

		public static string GetGoogleAccount(this Context context)
		{
			if(string.IsNullOrWhiteSpace(_currentEmail))
			{
				ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
				_currentEmail = prefs.GetString(ACCOUNT_NAME, null);
			}
			return _currentEmail;
		}

		public static void RemoveGoogleAccount(this Context context)
		{
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
			prefs.Edit().Remove(ACCOUNT_NAME).Apply();
			_currentEmail = null;
		}

		public static void SetGoogleAccount(this Context context, string newEmail)
		{
			if(string.IsNullOrWhiteSpace(newEmail))
			{
				RemoveGoogleAccount(context);
				return;
			}

			if(!newEmail.Equals(_currentEmail, StringComparison.OrdinalIgnoreCase))
			{
				_currentEmail = newEmail.Trim();
				ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
				prefs.Edit().PutString(ACCOUNT_NAME, _currentEmail).Apply();
			}
		}

		public static bool IsLoggedInToGoogle(this Context context)
		{
			return !String.IsNullOrWhiteSpace(GetGoogleAccount(context));
		}
    }
}
