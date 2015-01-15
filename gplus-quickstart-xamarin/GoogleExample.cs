/**
 * Copyright 2013 Google Inc. All Rights Reserved.
 *
 *  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 * in compliance with the License. You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software distributed under the
 * License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Plus;
using Android.Gms.Plus.Model.People;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;

using JavaObject = Java.Lang.Object;

namespace com.xamarin.googleplus.quickstart
{
    [Activity(Label = "@string/activity_google_login_example")]
    public class GoogleExample : Activity, IGoogleApiClientConnectionCallbacks, IGoogleApiClientOnConnectionFailedListener, IResultCallback
    {
        const int DIALOG_PLAY_SERVICES_ERROR = 0;
        static readonly String TAG = "xamarin-android-plus-quickstart";
        static readonly int RC_SIGN_IN = 0;
        static readonly String SAVED_PROGRESS = "sign_in_progress";
        ArrayAdapter<string> _circlesAdapter;
        List<string> _circlesList;
        ListView _circlesListView;
        IGoogleApiClient _googleApiClient;
        Button _revokeButton;
        SignInButton _signInButton;
        /// <summary>
        ///   Used to store the error code most recently returned by Google Play services
        ///   until the user clicks 'sign in'.
        /// </summary>
        int _signInError;
        /// <summary>
        ///   Used to store the PendingIntent most recently returned by Google Play
        ///   services until the user clicks 'sign in'.
        /// </summary>
        PendingIntent _signInIntent;
        SignInProgressState _signInProgress;
        Button _signOutButton;
        TextView _status;

        public void OnConnected(Bundle connectionHint)
        {
            Log.Info(TAG, "OnConnected");

            _signInButton.Enabled = true;
            _signOutButton.Enabled = true;
            _revokeButton.Enabled = true;

            var currentUser = PlusClass.PeopleApi.GetCurrentPerson(_googleApiClient);
            _status.Text = String.Format("Signed in as {0}.", currentUser.DisplayName);

            PlusClass.PeopleApi.LoadVisible(_googleApiClient, null).SetResultCallback(this);

            _signInProgress = SignInProgressState.Default;
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

            if (result.ErrorCode == ConnectionResult.ApiUnavailable)
            {
                // An API requested for GoogleApiClient is not available. The device's current
                // configuration might not be supported with the requested API or a required component
                // may not be installed, such as the Android Wear application. You may need to use a
                // second GoogleApiClient to manage the application's optional APIs.
            }
            else if (_signInProgress != SignInProgressState.InProgress)
            {
                // We do not have an intent in progress so we should store the latest
                // error resolution intent for use when the sign in button is clicked.
                _signInIntent = result.Resolution;
                _signInError = result.ErrorCode;

                if (_signInProgress == SignInProgressState.SignIn)
                {
                    ResolveSignInError();
                }
            }

            // In this sample we consider the user signed out whenever they do not have
            // a connection to Google Play services.
            OnSignedOut();
        }

        public void OnResult(JavaObject result)
        {
            var peopleData = result.JavaCast<IPeopleLoadPeopleResult>();
            if (peopleData.Status.StatusCode == CommonStatusCodes.Success)
            {
                _circlesList.Clear();
                var personBuffer = peopleData.PersonBuffer;
                try
                {
                    int count = personBuffer.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var person = personBuffer.Get(i).JavaCast<IPerson>();
                        _circlesList.Add(person.DisplayName);
                    }
                    _circlesAdapter.NotifyDataSetChanged();
                }
                finally
                {
                    personBuffer.Close();
                }
            }
            else
            {
                Log.Error(TAG, "Error requesting visible circles: {0}", peopleData.Status);
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            _signInButton = FindViewById<SignInButton>(Resource.Id.sign_in_button);
            _signInButton.Click += delegate{
                                       _status.Text = "Signing in with Goolge+...";
                                       ResolveSignInError();
                                   };

            _signOutButton = FindViewById<Button>(Resource.Id.sign_out_button);
            _signOutButton.Click += delegate{
                                        // We clear the default account on sign out so that Google Play
                                        // services will not return an onConnected callback without user
                                        // interaction.
#pragma warning disable 618
                                        PlusClass.AccountApi.ClearDefaultAccount(_googleApiClient);
#pragma warning restore 618
                                        _googleApiClient.Disconnect();
                                        _googleApiClient.Connect();
                                    };

            _revokeButton = FindViewById<Button>(Resource.Id.revoke_access_button);
            _revokeButton.Click += delegate{
                                       // After we revoke permissions for the user with a GoogleApiClient
                                       // instance, we must discard it and create a new one.
#pragma warning disable 618
                                       PlusClass.AccountApi.ClearDefaultAccount(_googleApiClient);
#pragma warning restore 618
                                       // Our sample has caches no user data from Google+, however we
                                       // would normally register a callback on revokeAccessAndDisconnect
                                       // to delete user data so that we comply with Google developer
                                       // policies.
                                       PlusClass.AccountApi.RevokeAccessAndDisconnect(_googleApiClient);
                                       _googleApiClient = BuildGoogleApiClient();
                                       _googleApiClient.Connect();
                                   };

            _status = FindViewById<TextView>(Resource.Id.sign_in_status);

            _circlesList = new List<string>();
            _circlesAdapter = new ArrayAdapter<string>(this, Resource.Layout.CircleMember, _circlesList);
            _circlesListView = FindViewById<ListView>(Resource.Id.circles_list);
            _circlesListView.Adapter = _circlesAdapter;

            if (bundle != null)
            {
                _signInProgress = (SignInProgressState)bundle.GetInt(SAVED_PROGRESS, (int)SignInProgressState.Default);
            }

            _googleApiClient = BuildGoogleApiClient();
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

            if (_googleApiClient.IsConnected)
            {
                Log.Debug(TAG, "OnStop: disconnect.");
                _googleApiClient.Disconnect();
            }
        }

        protected override Dialog OnCreateDialog(int id)
        {
            Dialog d;
            switch (id)
            {
                case DIALOG_PLAY_SERVICES_ERROR:
                    if (GooglePlayServicesUtil.IsUserRecoverableError(_signInError))
                    {
                        d = GooglePlayServicesUtil.GetErrorDialog(_signInError, this, RC_SIGN_IN);
                        d.CancelEvent += delegate{
                                             Log.Error(TAG, "Google Play services resolution cancelled.");
                                             _signInProgress = SignInProgressState.Default;
                                             _status.Text = "Signed Out";
                                         };
                        return d;
                    }
                    d = new AlertDialog.Builder(this)
                        .SetMessage("Google Play services is not available. This application...")
                        .SetPositiveButton("Close", delegate{
                                                        Log.Error(TAG, "Google Play services error cold not resolved: {0}", _signInError);
                                                        _signInProgress = SignInProgressState.Default;
                                                        _status.Text = "Signed out";
                                                    })
                        .Create();
                    break;
                default:
                    d = base.OnCreateDialog(id);
                    break;
            }

            return d;
        }

        void OnSignedOut()
        {
            _signOutButton.Enabled = false;
            _signInButton.Enabled = true;
            _revokeButton.Enabled = false;

            _status.Text = "Signed out";
            _circlesList.Clear();
            _circlesAdapter.NotifyDataSetChanged();
        }

        void ResolveSignInError()
        {
            if (_signInIntent != null)
            {
                // We have an intent which will allow our user to sign in or
                // resolve an error.  For example if the user needs to
                // select an account to sign in with, or if they need to consent
                // to the permissions your app is requesting.

                try
                {
                    // Send the pending intent that we stored on the most recent
                    // OnConnectionFailed callback.  This will allow the user to
                    // resolve the error currently preventing our connection to
                    // Google Play services.
                    _signInProgress = SignInProgressState.InProgress;
                    StartIntentSenderForResult(_signInIntent.IntentSender, RC_SIGN_IN, null, 0, 0, 0);
                }
                catch (IntentSender.SendIntentException e)
                {
                    Log.Info(TAG, "Sign in intent could not be sent: " + e.LocalizedMessage);
                    // The intent was canceled before it was sent.  Attempt to connect to
                    // get an updated ConnectionResult.
                    _signInProgress = SignInProgressState.SignIn;
                    _googleApiClient.Connect();
                }
            }
            else
            {
                ShowDialog(DIALOG_PLAY_SERVICES_ERROR);
            }
        }

        IGoogleApiClient BuildGoogleApiClient()
        {
            // When we build the GoogleApiClient we specify where connected and
            // connection failed callbacks should be returned, which Google APIs our
            // app uses and which OAuth 2.0 scopes our app requests.

            return new GoogleApiClientBuilder(this, this, this)
                .AddApi(PlusClass.Api, PlusClass.PlusOptions.InvokeBuilder().Build())
                .AddScope(PlusClass.ScopePlusLogin)
                .Build();
        }
    }

    /// <summary>
    ///   This value represents if the user has clicked sign in.
    /// </summary>
    enum SignInProgressState
    {
        /// <summary>
        ///   The default state of the application before the user
        ///   has clicked 'sign in', or after they have clicked
        ///   'sign out'.  In this state we will not attempt to
        ///   resolve sign in errors and so will display our
        /// </summary>
        Default,
        /// <summary>
        ///   This state indicates that the user has clicked 'sign
        ///   in', so resolve successive errors preventing sign in
        ///   until the user has successfully authorized an account
        ///   for our app.
        /// </summary>
        SignIn,
        /// <summary>
        ///   This state indicates that we have started an intent to
        ///   resolve an error, and so we should not start further
        ///   intents until the current intent completes.
        /// </summary>
        InProgress
    }
}
