using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Firebase.Auth;
using Firebase.Database;
using Uber_Driver.EventListeners;
using Uber_Driver.Helpers;

namespace Uber_Driver.Activities
{
    [Activity(Label = "LoginActivity", Theme = "@style/UberTheme", MainLauncher = false)]
    public class LoginActivity : AppCompatActivity
    {
        Button loginButton;
        TextInputLayout textInputEmail;
        TextInputLayout textInputPassword;
        CoordinatorLayout rootView;
        TextView clickToSignUp;

        FirebaseDatabase database;
        FirebaseAuth mAuth;
        FirebaseUser currentUser;

        Android.Support.V7.App.AlertDialog.Builder alert;
        Android.Support.V7.App.AlertDialog alertDialog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.login);
            ConnectViews();
            InitializeFirebase();
        }

        void InitializeFirebase()
        {
            mAuth = AppDataHelper.GetFirebaseAuth();
            currentUser = AppDataHelper.GetCurrentUser();
            database = AppDataHelper.GetDatabase();
        }

        void ConnectViews()
        {
            loginButton = (Button)FindViewById(Resource.Id.loginButton);
            textInputEmail = (TextInputLayout)FindViewById(Resource.Id.emailText);
            textInputPassword = (TextInputLayout)FindViewById(Resource.Id.passwordText);
            rootView = (CoordinatorLayout)FindViewById(Resource.Id.rootView);
            clickToSignUp = (TextView)FindViewById(Resource.Id.clickToSignUpText);
           
            loginButton.Click += LoginButton_Click;
            clickToSignUp.Click += ClickToSignUp_Click;
        }

        private void ClickToSignUp_Click(object sender, EventArgs e)
        {
            
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            string email, password;
            email = textInputEmail.EditText.Text;
            password = textInputPassword.EditText.Text;

            ShowProgressDialogue();

            TaskCompletionListeners taskCompletionListener = new TaskCompletionListeners();
            taskCompletionListener.Successful += TaskCompletionListener_Successful;
            taskCompletionListener.Failure += TaskCompletionListener_Failure;

            mAuth.SignInWithEmailAndPassword(email, password)
                .AddOnSuccessListener(this, taskCompletionListener)
                .AddOnFailureListener(this, taskCompletionListener);
        }

        private void TaskCompletionListener_Failure(object sender, EventArgs e)
        {
            CloseProgressDialogue();
            Snackbar.Make(rootView, "Login Failed", Snackbar.LengthShort).Show();
        }

        private void TaskCompletionListener_Successful(object sender, EventArgs e)
        {
            CloseProgressDialogue();
            StartActivity(typeof(MainActivity));
        }

        void ShowProgressDialogue()
        {
            alert = new Android.Support.V7.App.AlertDialog.Builder(this);
            alert.SetView(Resource.Layout.progress);
            alert.SetCancelable(false);
            alertDialog = alert.Show();
        }


        void CloseProgressDialogue()
        {
           if(alert != null)
            {
                alertDialog.Dismiss();
                alertDialog = null;
                alert = null;
            }
        }
       
    }
}