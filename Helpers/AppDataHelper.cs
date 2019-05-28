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
using Firebase;
using Firebase.Auth;
using Firebase.Database;

namespace Uber_Driver.Helpers
{
   public class AppDataHelper
    {
        static ISharedPreferences pref = Application.Context.GetSharedPreferences("userinfo", FileCreationMode.Private);

        public static FirebaseDatabase GetDatabase()
        {
            var app = FirebaseApp.InitializeApp(Application.Context);
            FirebaseDatabase database;
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("uber-clone-da636")
                    .SetApiKey("AIzaSyBpBjZCW6lj0r9KbfZ0ymvpzDziJvaJeu4")
                    .SetDatabaseUrl("https://uber-clone-da636.firebaseio.com")
                    .SetStorageBucket("uber-clone-da636.appspot.com")
                    .Build();
                app = FirebaseApp.InitializeApp(Application.Context, options);
                database = FirebaseDatabase.GetInstance(app);
            }
            else
            {
                database = FirebaseDatabase.GetInstance(app);
            }
            return database;
        }

        public static FirebaseAuth GetFirebaseAuth()
        {

            var app = FirebaseApp.InitializeApp(Application.Context);
            FirebaseAuth mAuth;
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("uber-clone-da636")
                    .SetApiKey("AIzaSyBpBjZCW6lj0r9KbfZ0ymvpzDziJvaJeu4")
                    .SetDatabaseUrl("https://uber-clone-da636.firebaseio.com")
                    .SetStorageBucket("uber-clone-da636.appspot.com")
                    .Build();
                app = FirebaseApp.InitializeApp(Application.Context, options);
                mAuth = FirebaseAuth.Instance;
            }
            else
            {
                mAuth = FirebaseAuth.Instance;
            }

            return mAuth;
        }

        public static FirebaseUser GetCurrentUser()
        {
            var app = FirebaseApp.InitializeApp(Application.Context);
            FirebaseAuth mAuth;
            FirebaseUser mUser;
            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("uber-clone-da636")
                    .SetApiKey("AIzaSyBpBjZCW6lj0r9KbfZ0ymvpzDziJvaJeu4")
                    .SetDatabaseUrl("https://uber-clone-da636.firebaseio.com")
                    .SetStorageBucket("uber-clone-da636.appspot.com")
                    .Build();
                app = FirebaseApp.InitializeApp(Application.Context, options);
                mAuth = FirebaseAuth.Instance;
                mUser = mAuth.CurrentUser;
            }
            else
            {
                mAuth = FirebaseAuth.Instance;
                mUser = mAuth.CurrentUser;
            }

            return mUser;
        }

        public static string GetFullname()
        {
            string fullname = pref.GetString("fullname", "");
            return fullname;
        }

        public static string GetEmail()
        {
            string email = pref.GetString("email", "");
            return email;
        }

        public static string GetPhone()
        {
            string phone = pref.GetString("phone", "");
            return phone;
        }

    }
}