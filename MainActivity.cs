using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Support.V4.View;
using Com.Ittianyu.Bottomnavigationviewex;
using System;
using Uber_Driver.Adapter;
using Uber_Driver.Fragments;
using Android.Graphics;
using Android;
using Android.Support.V4.App;
using Uber_Driver.EventListeners;
using Android.Gms.Maps.Model;
using Android.Support.V4.Content;
using Uber_Driver.DataModels;
using Android.Media;
using Uber_Driver.Helpers;
using Android.Content;

namespace Uber_Driver
{
    [Activity(Label = "@string/app_name", Theme = "@style/UberTheme", MainLauncher = false)]
    public class MainActivity : AppCompatActivity
    {
        //Buttons
        Button goOnlineButton;

        //Views
        ViewPager viewpager;
        BottomNavigationViewEx bnve;

        //Fragments
        HomeFragment homeFragment = new HomeFragment();
        RatingsFragment ratingsFragment = new RatingsFragment();
        EarningsFragment earningsFragment = new EarningsFragment();
        AccountFragment accountFragment = new AccountFragment();
        NewRequestFragment requestFoundDialogue;

        //PermissionRequest
        const int RequestID = 0;
        readonly string[] permissionsGroup =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation,
        };


        //EventListeners
        ProfileEventListener profileEventListener = new ProfileEventListener();
        AvailablityListener availablityListener;
        RideDetailsListener rideDetailsListener;
        NewTripEventListener newTripEventListener;


        //Map Stuffs
        Android.Locations.Location mLastLocation;
        LatLng mLastLatLng;


        //Flags
        bool availablityStatus;
        bool isBackground;
        bool newRideAssigned;
        string status = "NORMAL"; //REQUESTFOUND, ACCEPTED, ONTRIP

        //Datamodels
        RideDetails newRideDetails;

        //MediaPlayer
        MediaPlayer player;

        //Helpers
        MapFunctionHelper mapHelper;

        Android.Support.V7.App.AlertDialog.Builder alert;
        Android.Support.V7.App.AlertDialog alertDialog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            ConnectViews();
            CheckSpecialPermission();
            profileEventListener.Create();
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
            if (alert != null)
            {
                alertDialog.Dismiss();
                alertDialog = null;
                alert = null;
            }
        }


        void ConnectViews()
        {
            goOnlineButton = (Button)FindViewById(Resource.Id.goOnlineButton);
            bnve = (BottomNavigationViewEx)FindViewById(Resource.Id.bnve);
            bnve.EnableItemShiftingMode(false);
            bnve.EnableShiftingMode(false);

            goOnlineButton.Click += GoOnlineButton_Click;
            bnve.NavigationItemSelected += Bnve_NavigationItemSelected;


            var img0 = bnve.GetIconAt(0);
            var txt0 = bnve.GetLargeLabelAt(0);
            img0.SetColorFilter(Color.Rgb(24, 191, 242));
            txt0.SetTextColor(Color.Rgb(24, 191, 242));

            viewpager = (ViewPager)FindViewById(Resource.Id.viewpager);
            viewpager.OffscreenPageLimit = 3;
            viewpager.BeginFakeDrag(); 

            SetupViewPager();

            homeFragment.CurrentLocation += HomeFragment_CurrentLocation;
            homeFragment.TripActionArrived += HomeFragment_TripActionArrived;
            homeFragment.CallRider += HomeFragment_CallRider;
            homeFragment.Navigate += HomeFragment_Navigate;
            homeFragment.TripActionStartTrip += HomeFragment_TripActionStartTrip;
            homeFragment.TripActionEndTrip += HomeFragment_TripActionEndTrip;
        }

        async void HomeFragment_TripActionEndTrip(object sender, EventArgs e)
        {
            //Reset app
            status = "NORMAL";
            homeFragment.ResetAfterTrip();

            ShowProgressDialogue();
            LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
            double fares = await mapHelper.CalculateFares(pickupLatLng, mLastLatLng);
            CloseProgressDialogue();

            newTripEventListener.EndTrip(fares);
            newTripEventListener = null;

            CollectPaymentFragment collectPaymentFragment = new CollectPaymentFragment(fares);
            collectPaymentFragment.Cancelable = false;
            var trans = SupportFragmentManager.BeginTransaction();
            collectPaymentFragment.Show(trans, "pay");
            collectPaymentFragment.PaymentCollected += (o, u) =>
            {
                collectPaymentFragment.Dismiss();
            };

            availablityListener.ReActivate();

        }


        void HomeFragment_TripActionStartTrip(object sender, EventArgs e)
        {
            Android.Support.V7.App.AlertDialog.Builder startTripAlert = new Android.Support.V7.App.AlertDialog.Builder(this);
            startTripAlert.SetTitle("START TRIP");
            startTripAlert.SetMessage("Are you sure");
            startTripAlert.SetPositiveButton("Continue", (senderAlert, args) =>
            {
                status = "ONTRIP";

                // Update Rider that Driver has started the trip
                newTripEventListener.UpdateStatus("ontrip");
            });

            startTripAlert.SetNegativeButton("Cancel", (senderAlert, args) =>
            {
                startTripAlert.Dispose();
            });

            startTripAlert.Show();
        }


        void HomeFragment_Navigate(object sender, EventArgs e)
        {
            string uriString = "";

            if(status == "ACCEPTED")
            {
                uriString = "google.navigation:q=" + newRideDetails.PickupLat.ToString() + "," + newRideDetails.PickupLng.ToString();
            }
            else
            {
                uriString = "google.navigation:q=" + newRideDetails.DestinationLat.ToString() + "," + newRideDetails.DestinationLng.ToString();
            }

            Android.Net.Uri googleMapIntentUri = Android.Net.Uri.Parse(uriString);
            Intent mapIntent = new Intent(Intent.ActionView, googleMapIntentUri);
            mapIntent.SetPackage("com.google.android.apps.maps");

            try
            {
                StartActivity(mapIntent);
            }
            catch
            {
                Toast.MakeText(this, "Google Map is not Installed on this device", ToastLength.Short).Show();
            }
        }


        void HomeFragment_CallRider(object sender, EventArgs e)
        {
            var uri = Android.Net.Uri.Parse("tel:" + newRideDetails.RiderPhone);
            Intent intent = new Intent(Intent.ActionDial, uri);
            StartActivity(intent);
        }


        async void HomeFragment_TripActionArrived(object sender, EventArgs e)
        {
            //Notifies Rider that Driver has arrived
            newTripEventListener.UpdateStatus("arrived");
            status = "ARRIVED";

            LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
            LatLng destinationLatLng = new LatLng(newRideDetails.DestinationLat, newRideDetails.DestinationLng);

            ShowProgressDialogue();
            string directionJson = await mapHelper.GetDirectionJsonAsync(pickupLatLng, destinationLatLng);
            CloseProgressDialogue();

            //Clear Map
            homeFragment.mainMap.Clear();
            mapHelper.DrawTripToDestination(directionJson);
        }


        void HomeFragment_CurrentLocation(object sender, Helpers.LocationCallbackHelper.OnLocationCaptionEventArgs e)
        {
            mLastLocation = e.Location;
            mLastLatLng = new LatLng(mLastLocation.Latitude, mLastLocation.Longitude);

            if(availablityListener != null)
            {
                availablityListener.UpdateLocation(mLastLocation);
            }

            if (availablityStatus && availablityListener == null)
            {
                TakeDriverOnline();
            }

            if(status == "ACCEPTED")
            {
                //Update and Animate driver movement to pick up lOcation
                LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);
                mapHelper.UpdateMovement(mLastLatLng, pickupLatLng, "Rider");

                //Updates Location in rideRequest Table, so that Rider can receive Updates
                newTripEventListener.UpdateLocation(mLastLocation);

            }
            else if(status == "ARRIVED")
            {
                newTripEventListener.UpdateLocation(mLastLocation);
            }
            else if(status == "ONTRIP")
            {
                //Update and animate driver movement to Destination
                LatLng destinationLatLng = new LatLng(newRideDetails.DestinationLat, newRideDetails.DestinationLng);
                mapHelper.UpdateMovement(mLastLatLng, destinationLatLng, "Destination");

                //Update Location on firebase
                newTripEventListener.UpdateLocation(mLastLocation);
            }


        }

        private void TakeDriverOnline()
        {
            availablityListener = new AvailablityListener();
            availablityListener.Create(mLastLocation);
            availablityListener.RideAssigned += AvailablityListener_RideAssigned;
            availablityListener.RideTimedOut += AvailablityListener_RideTimedOut;
            availablityListener.RideCancelled += AvailablityListener_RideCancelled;
        }

        void TakeDriverOffline()
        {
            availablityListener.RemoveListener();
            availablityListener = null;
        }

        void AvailablityListener_RideAssigned(object sender, AvailablityListener.RideAssignedIDEventArgs e)
        {
           
            //Get Details
            rideDetailsListener = new RideDetailsListener();
            rideDetailsListener.Create(e.RideId);
            rideDetailsListener.RideDetailsFound += RideDetailsListener_RideDetailsFound;
            rideDetailsListener.RideDetailsNotFound += RideDetailsListener_RideDetailsNotFound;

        }

        void RideDetailsListener_RideDetailsNotFound(object sender, EventArgs e)
        {

        }

        void CreateNewRequestDialogue()
        {
            requestFoundDialogue = new NewRequestFragment(newRideDetails.PickupAddress, newRideDetails.DestinationAddress);
            requestFoundDialogue.Cancelable = false;
            var trans = SupportFragmentManager.BeginTransaction();
            requestFoundDialogue.Show(trans, "Request");

            //Play Alert
            player = MediaPlayer.Create(this, Resource.Raw.alert);
            player.Start();

            requestFoundDialogue.RideRejected += RequestFoundDialogue_RideRejected;
            requestFoundDialogue.RideAccepted += RequestFoundDialogue_RideAccepted;
        }

       async void RequestFoundDialogue_RideAccepted(object sender, EventArgs e)
        {
            newTripEventListener = new NewTripEventListener(newRideDetails.RideId, mLastLocation);
            newTripEventListener.Create();

            status = "ACCEPTED";

            //Stop Alert
            if(player != null)
            {
                player.Stop();
                player = null;
            }

            //Dissmiss Dialogue
            if(requestFoundDialogue != null)
            {
                requestFoundDialogue.Dismiss();
                requestFoundDialogue = null;
            }

            homeFragment.CreateTrip(newRideDetails.RiderName);
            mapHelper = new MapFunctionHelper(Resources.GetString(Resource.String.mapkey), homeFragment.mainMap);
            LatLng pickupLatLng = new LatLng(newRideDetails.PickupLat, newRideDetails.PickupLng);

            ShowProgressDialogue();
            string directionJson =  await mapHelper.GetDirectionJsonAsync(mLastLatLng, pickupLatLng);
            CloseProgressDialogue();

            mapHelper.DrawTripOnMap(directionJson);
        }


        void RequestFoundDialogue_RideRejected(object sender, EventArgs e)
        {
            //Stop Alert
            if (player != null)
            {
                player.Stop();
                player = null;
            }

            //Dissmiss Dialogue
            if (requestFoundDialogue != null)
            {
                requestFoundDialogue.Dismiss();
                requestFoundDialogue = null;
            }

            availablityListener.ReActivate();

            //Do other stuff
        }


        void RideDetailsListener_RideDetailsFound(object sender, RideDetailsListener.RideDetailsEventArgs e)
        {
            if(status != "NORMAL")
            {
                return;
            }
            newRideDetails = e.RideDetails;

            if (!isBackground)
            {
                CreateNewRequestDialogue();
            }
            else
            {
                newRideAssigned = true;
                NotificationHelper notificationHelper = new NotificationHelper();
                if((int)Build.VERSION.SdkInt >= 26)
                {
                    notificationHelper.NotifyVersion26(this, Resources, (NotificationManager)GetSystemService(NotificationService));
                }
            }
        }


        void AvailablityListener_RideTimedOut(object sender, EventArgs e)
        {
            if(requestFoundDialogue != null)
            {
                requestFoundDialogue.Dismiss();
                requestFoundDialogue = null;
                player.Stop();
                player = null;
            }

            Toast.MakeText(this, "New trip Timeout", ToastLength.Short).Show();
            availablityListener.ReActivate();
        }



        void AvailablityListener_RideCancelled(object sender, EventArgs e)
        {
            if (requestFoundDialogue != null)
            {
                requestFoundDialogue.Dismiss();
                requestFoundDialogue = null;
                player.Stop();
                player = null;
            }

            Toast.MakeText(this, "New trip was cancelled", ToastLength.Short).Show();
            availablityListener.ReActivate();
        }


        void GoOnlineButton_Click(object sender, EventArgs e)
        {
            if (!CheckSpecialPermission())
            {
                return;
            }

            if (availablityStatus)
            {
                Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder(this);
                alert.SetTitle("GO OFFLINE");
                alert.SetMessage("You will not be able to receive Ride Request");
                alert.SetPositiveButton("Continue", (senderAlert, args) =>
                {
                    homeFragment.GoOffline();
                    goOnlineButton.Text = "Go Online";
                    goOnlineButton.Background = ContextCompat.GetDrawable(this, Resource.Drawable.uberroundbutton);

                    availablityStatus = false;
                    TakeDriverOffline();
                });

                alert.SetNegativeButton("Cancel", (senderAlert, args) =>
                {
                    alert.Dispose();
                });

                alert.Show();
            }
            else
            {
                availablityStatus = true;
                homeFragment.GoOnline();
                goOnlineButton.Text = "Go offline";
                goOnlineButton.Background = ContextCompat.GetDrawable(this, Resource.Drawable.uberroundbutton_green);
            }

        }


        private void Bnve_NavigationItemSelected(object sender, Android.Support.Design.Widget.BottomNavigationView.NavigationItemSelectedEventArgs e)
        {
            if (e.Item.ItemId == Resource.Id.action_earning)
            {
                viewpager.SetCurrentItem(1, true);
                BnveToAccentColor(1);
            }
            else if (e.Item.ItemId == Resource.Id.action_home)
            {
                viewpager.SetCurrentItem(0, true);
                BnveToAccentColor(0);
            }
            else if (e.Item.ItemId == Resource.Id.action_rating)
            {
                viewpager.SetCurrentItem(2, true);
                BnveToAccentColor(2);

            }
            else if(e.Item.ItemId == Resource.Id.action_account)
            {
                viewpager.SetCurrentItem(3, true);
                BnveToAccentColor(3);
            }
          
        }

        void BnveToAccentColor(int index)
        {
            //Set all to white
            var img = bnve.GetIconAt(1);
            var txt = bnve.GetLargeLabelAt(1);
            img.SetColorFilter(Color.Rgb(255, 255, 255));
            txt.SetTextColor(Color.Rgb(255, 255, 255));

            var img0 = bnve.GetIconAt(0);
            var txt0 = bnve.GetLargeLabelAt(0);
            img0.SetColorFilter(Color.Rgb(255, 255, 255));
            txt0.SetTextColor(Color.Rgb(255, 255, 255));

            var img2 = bnve.GetIconAt(2);
            var txt2 = bnve.GetLargeLabelAt(2);
            img2.SetColorFilter(Color.Rgb(255, 255, 255));
            txt2.SetTextColor(Color.Rgb(255, 255, 255));

            var img3 = bnve.GetIconAt(3);
            var txt3 = bnve.GetLargeLabelAt(3);
            img2.SetColorFilter(Color.Rgb(255, 255, 255));
            txt2.SetTextColor(Color.Rgb(255, 255, 255));

            //Sets Accent Color
            var imgindex = bnve.GetIconAt(index);
            var textindex = bnve.GetLargeLabelAt(index);
            imgindex.SetColorFilter(Color.Rgb(24, 191, 242));
            textindex.SetTextColor(Color.Rgb(24, 191, 242));

        }

        private void SetupViewPager()
        {
            ViewPagerAdapter adapter = new ViewPagerAdapter(SupportFragmentManager);
            adapter.AddFragment(homeFragment, "Home");
            adapter.AddFragment(earningsFragment, "Earnings");
            adapter.AddFragment(ratingsFragment, "Rating");
            adapter.AddFragment(accountFragment, "Account");
            viewpager.Adapter = adapter;
        }

        bool CheckSpecialPermission()
        {
            bool permissionGranted = false;
            if(ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted && 
                ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessCoarseLocation) != Android.Content.PM.Permission.Granted)
            {
                RequestPermissions(permissionsGroup, RequestID);
            }
            else
            {
                permissionGranted = true;
            }

            return permissionGranted;
        }

        protected override void OnPause()
        {
            isBackground = true;
            base.OnPause();
        }

        protected override void OnResume()
        {
            isBackground = false;
            if (newRideAssigned)
            {
                CreateNewRequestDialogue();
                newRideAssigned = false;
            }
            base.OnResume();
        }
    }
}