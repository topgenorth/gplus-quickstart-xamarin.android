
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
using Xamarin.Auth;

namespace com.xamarin.googleplus.quickstart
{
	[Activity(Label = "@string/activity_xamarin_auth_example")]			
	public class XamarinAuthActivity : Activity
	{
		Button _loginButton;




		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.activity_xamarin_auth);
			_loginButton = FindViewById<Button>(Resource.Id.sign_in_xam_auth);

			_loginButton.Click += LoginToGoogle;


		}

		void LoginToGoogle (object sender, EventArgs e)
		{
			OAuth2Authenticator auth = new OAuth2Authenticator (
				clientId: "846829989756-bnss6107nj7lmt4lq4emoqh08o8k3sfu.apps.googleusercontent.com",
				scope: "https://www.googleapis.com/auth/userinfo.email",
				authorizeUrl: new Uri ("https://accounts.google.com/o/oauth2/auth"),
				redirectUrl: new Uri ("http://localhost"));

			auth.Completed += GoogleLoginCompleted;

			var intent = auth.GetUI(this);
			StartActivity(intent);

		}

		void GoogleLoginCompleted (object sender, AuthenticatorCompletedEventArgs e)
		{
			Console.WriteLine(e.IsAuthenticated);



		}
	}
}

