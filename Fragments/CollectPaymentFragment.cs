﻿
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
    public class CollectPaymentFragment : Android.Support.V4.App.DialogFragment
    {
        double mfares;

        TextView totalfaresText;
        Button collectPayButton;

        public event EventHandler PaymentCollected;

        public CollectPaymentFragment(double fares)
        {
            mfares = fares;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.collect_payment, container, false);

            totalfaresText = (TextView)view.FindViewById(Resource.Id.totalfaresText);
            collectPayButton = (Button)view.FindViewById(Resource.Id.collectPayButton);

            totalfaresText.Text = "$" + mfares.ToString();
            collectPayButton.Click += CollectPayButton_Click;
            return view;
        }

        void CollectPayButton_Click(object sender, EventArgs e)
        {
            PaymentCollected.Invoke(this, new EventArgs());
        }

    }
}
