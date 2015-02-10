using Android.Gms.Common.Apis;
using JavaObject = Java.Lang.Object;
using Android.Util;
using Android.Runtime;
using Android.Content;

namespace com.xamarin.googleplus.quickstart
{
	class GoogleAccountApiAccessRevokedResultCallback : JavaObject, IResultCallback
	{
		readonly Context _context;

		public GoogleAccountApiAccessRevokedResultCallback(Context context)
		{
			_context = context;
		}

		static readonly string TAG = typeof(GoogleAccountApiAccessRevokedResultCallback).FullName;
		public void OnResult(JavaObject result)
		{
			Log.Debug(TAG, "Revoke user access");
			var status = result.JavaCast<Statuses>();
			if(status.IsSuccess)
			{
				_context.RemoveGoogleAccount();
			}

		}
	}	
}
