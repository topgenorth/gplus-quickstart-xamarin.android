﻿using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Plus;
using Android.Gms.Plus.Model.People;
using Android.OS;
using Android.Util;
using Android.Widget;

using JavaObject = Java.Lang.Object;

namespace com.xamarin.googleplus.quickstart
{

	[Activity(Label = "@string/activity_google_login_example", MainLauncher = true, Icon = "@drawable/ic_launcher")]
	public class GooglePlayServicesActivity : Activity, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener
	{
		const int SIGN_IN_WITH_GOOGLE_PLUS = 0;
		static readonly String TAG = typeof(GooglePlayServicesActivity).FullName;
		static readonly String SAVED_PROGRESS = "sign_in_progress";
		ArrayAdapter<string> _circlesAdapter;
		List<string> _circlesList;
		ListView _circlesListView;
		Button _revokeButton;
		SignInButton _signInButton;
		Button _signOutButton;
		TextView _status;

		IGoogleApiClient _googleApiClient;
		bool _signInClicked = false;
		bool _intentInProgress = false;
		bool _displayUserInformation = true;
		ConnectionResult _connectionResult;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.activity_google_play_services);

			_signInButton = FindViewById<SignInButton>(Resource.Id.sign_in_button);
			_signOutButton = FindViewById<Button>(Resource.Id.sign_out_button);
			_revokeButton = FindViewById<Button>(Resource.Id.revoke_access_button);
			_status = FindViewById<TextView>(Resource.Id.sign_in_status);
			_circlesListView = FindViewById<ListView>(Resource.Id.circles_list);

			DisplayFriendNames(new List<string>());

			if(GooglePlayServicesUtil.IsGooglePlayServicesAvailable(this) == ConnectionResult.Success)
			{
				_signInButton.Click += delegate {
					_status.SetText(Resource.String.status_signing_in);
					_signInClicked = true;
					ResolveSignInError();
				};

				_signOutButton.Click += delegate {
					if(_googleApiClient.IsConnected)
					{
						_googleApiClient.ClearDefaultAccountAndReconnect();
					}
					OnSignedOut();
					_status.SetText(Resource.String.status_signed_out);
					DisplayFriendNames(new List<string>());
				};

				_revokeButton.Click += delegate {
					if(_googleApiClient.IsConnected)
					{
						_googleApiClient.ClearDefaultAccountAndReconnect();

						PlusClass.AccountApi
						.RevokeAccessAndDisconnect(_googleApiClient)
						.SetResultCallback(new GoogleAccountApiAccessRevokedResultCallback(this));

					}
					OnSignedOut();
					_status.SetText(Resource.String.status_revoke_access);
					DisplayFriendNames(new List<string>());
				};
				_googleApiClient = this.BuildGoogleApiClient();
			} else
			{
				Toast.MakeText(this, Resource.String.play_services_error, ToastLength.Short).Show();
				Finish();
			}
		}

		protected override void OnStart()
		{
			base.OnStart();
			Log.Debug(TAG, "OnStart : trying to connect.");
			_googleApiClient.Connect();
		}

		protected override void OnStop()
		{
			base.OnStop();

			if(_googleApiClient.IsConnected)
			{
				Log.Debug(TAG, "OnStop: disconnect.");
				_googleApiClient.Disconnect();
			}
		}

		public void OnConnected(Bundle connectionHint)
		{
			Log.Info(TAG, "OnConnected");

			_signInButton.Enabled = false;
			_signOutButton.Enabled = true;
			_revokeButton.Enabled = true;
			_signInClicked = false;

			if(!this.IsLoggedInToGoogle())
			{
				string account = PlusClass.AccountApi.GetAccountName(_googleApiClient);
				this.SetGoogleAccount(account);
				_displayUserInformation = true;
			}

			if(_displayUserInformation)
			{
				GetFriendsFromGooglePlus();
				_displayUserInformation = false;
			}
		}

		public void OnConnectionSuspended(int cause)
		{
			// The connection to Google Play services was lost for some reason.
			// We call connect() to attempt to re-establish the connection or get a
			// ConnectionResult that we can attempt to resolve.
			Log.Debug(TAG, "OnSuspend - trying to reconnect.");
			_googleApiClient.Connect();
		}

		public void OnConnectionFailed(ConnectionResult result)
		{
			Log.Info(TAG, "OnConnectionFailed: ConnectionResult.ErrorCode = {0}", result.ErrorCode);
			_connectionResult = result;

			if(_signInClicked)
			{
				ResolveSignInError();
			} else
			{
				OnSignedOut();
			}
		}

		void GetFriendsFromGooglePlus()
		{
			PlusClass.PeopleApi.LoadVisible(_googleApiClient, null)
				.SetResultCallback(new DisplayVisibleFriendsResultCallback(this));
			_displayUserInformation = false;
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			switch(requestCode)
			{
				case (SIGN_IN_WITH_GOOGLE_PLUS):
					_intentInProgress = false;
					if(resultCode == Result.Ok)
					{
						_signInClicked = false;
						Log.Debug(TAG, "Sign in succeeded");
						if(!_googleApiClient.IsConnecting)
						{
							_displayUserInformation = true;
							_googleApiClient.Connect();
						}
					} else
					{
						OnSignedOut();
						Log.Debug(TAG, "Sign in cancelled.");
					}
					break;
				default:
					OnSignedOut();
					break;
			}

		}

		void OnSignedOut()
		{
			_signOutButton.Enabled = false;
			_signInButton.Enabled = true;
			_revokeButton.Enabled = false;

			_signInClicked = false;
			_displayUserInformation = false;
			_intentInProgress = false;

			DisplayFriendNames(new List<string>());
		}

		void ResolveSignInError()
		{
			if(_connectionResult.HasResolution)
			{
				try
				{
					_intentInProgress = true;
					_displayUserInformation = true;
					StartIntentSenderForResult(_connectionResult.Resolution.IntentSender, SIGN_IN_WITH_GOOGLE_PLUS, null, 0, 0, 0);
				} catch(IntentSender.SendIntentException e)
				{
					// The Intent was canceled before it was sent. Return to the default state
					// and attempt to connect to get an updated ConnectionResult
					_intentInProgress = false;
					_displayUserInformation = false;
				}
			} else
			{
				Dialog errorDialog;
				int errorCode = _connectionResult.ErrorCode;
				if(GooglePlayServicesUtil.IsUserRecoverableError(errorCode))
				{
					errorDialog = GooglePlayServicesUtil.GetErrorDialog(errorCode, this, SIGN_IN_WITH_GOOGLE_PLUS);
				} else
				{
					errorDialog = new AlertDialog.Builder(this)
                        .SetMessage("Google Play services is not available. Please install Google Play services and try again.")
                        .SetPositiveButton("Close", delegate {
							Log.Error(TAG, "Google Play services error could not resolved error {0}.", errorCode);
							_intentInProgress = false;
							_signInClicked = false;
							_displayUserInformation = false;
							OnSignedOut();
						})
                        .Create();
				}
				ErrorDialogFragment errorFrag = ErrorDialogFragment.NewInstance(errorDialog);
				errorFrag.Show(FragmentManager, "google_play_services");
			}
		}

		public void DisplayFriendNames(IList<string> names)
		{
			if((_googleApiClient != null) && (_googleApiClient.IsConnected))
			{
				string account = this.GetGoogleAccount();
				IPerson currentUser = PlusClass.PeopleApi.GetCurrentPerson(_googleApiClient);

				_status.Text = String.Format(GetString(Resource.String.signed_in_as), currentUser.DisplayName, account);
			}

			_circlesList = names.ToList();
			_circlesAdapter = new ArrayAdapter<string>(this, Resource.Layout.CircleMember, _circlesList);
			_circlesListView.Adapter = _circlesAdapter;
		}

	}
}
