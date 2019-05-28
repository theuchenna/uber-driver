
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Uber_Driver.Fragments
{
    public class NewRequestFragment : Android.Support.V4.App.DialogFragment
    {

        //Views
        RelativeLayout acceptRideButton;
        RelativeLayout rejectRideButton;
        TextView pickupAddressText;
        TextView destinationAddressText;

        string mPickupAddress;
        string mDestinationAddress;

        //Events
        public event EventHandler RideAccepted;
        public event EventHandler RideRejected;

        public NewRequestFragment(string pickupAddress, string destinationAddress)
        {
            mPickupAddress = pickupAddress;
            mDestinationAddress = destinationAddress;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
           View view =   inflater.Inflate(Resource.Layout.newrequest_dialogue, container, false);
            pickupAddressText = (TextView)view.FindViewById(Resource.Id.newridePickupText);
            destinationAddressText = (TextView)view.FindViewById(Resource.Id.newrideDestinationText);

            pickupAddressText.Text = mPickupAddress;
            destinationAddressText.Text = mDestinationAddress;

            acceptRideButton = (RelativeLayout)view.FindViewById(Resource.Id.acceptRideButton);
            rejectRideButton = (RelativeLayout)view.FindViewById(Resource.Id.rejectRideButton);

            acceptRideButton.Click += AcceptRideButton_Click;
            rejectRideButton.Click += RejectRideButton_Click;

            return view;
        }

        void AcceptRideButton_Click(object sender, EventArgs e)
        {
            RideAccepted?.Invoke(this, new EventArgs());
        }

        void RejectRideButton_Click(object sender, EventArgs e)
        {
            RideRejected?.Invoke(this, new EventArgs());
        }

    }
}
