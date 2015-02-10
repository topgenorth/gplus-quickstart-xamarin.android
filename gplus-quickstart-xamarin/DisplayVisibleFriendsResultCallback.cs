using System.Collections.Generic;
using Android.Gms.Common.Apis;
using Android.Gms.Plus;
using Android.Gms.Plus.Model.People;
using Android.Runtime;
using Android.Util;

using JavaObject = Java.Lang.Object;

namespace com.xamarin.googleplus.quickstart
{
	class DisplayVisibleFriendsResultCallback: JavaObject, IResultCallback
	{
		static readonly string TAG = typeof(DisplayVisibleFriendsResultCallback).FullName;
		readonly GooglePlayServicesActivity _activity;

		public DisplayVisibleFriendsResultCallback(GooglePlayServicesActivity activity)
		{
			_activity = activity;
		}

		public void OnResult(JavaObject result)
		{
			IPeopleLoadPeopleResult peopleData = result.JavaCast<IPeopleLoadPeopleResult>();
			List<string> names = new List<string>();

			if(peopleData.Status.StatusCode == CommonStatusCodes.Success)
			{
				PersonBuffer personBuffer = peopleData.PersonBuffer;
				try
				{
					int count = personBuffer.Count;
					for(int i = 0; i < count; i++)
					{
						IPerson person = personBuffer.Get(i).JavaCast<IPerson>();
						names.Add(person.DisplayName);
					}
					_activity.DisplayFriends(names);

				} finally
				{
					personBuffer.Close();
				}
			} else
			{
				Log.Error(TAG, "Error requesting visible circles: {0}", peopleData.Status);
			}
		}
	}
	
}
