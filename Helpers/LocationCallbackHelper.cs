﻿using System;
using Android.Gms.Location;
using Android.Util;

namespace Uber_Driver.Helpers
{
    public class LocationCallbackHelper : LocationCallback
    {


        public EventHandler<OnLocationCaptionEventArgs> MyLocation;

        public class OnLocationCaptionEventArgs : EventArgs
        {
            public Android.Locations.Location Location { get; set; }
        }

        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            Log.Debug("Uber Clone", "IsLocationAvailable: {0}", locationAvailability.IsLocationAvailable);
        }


        public override void OnLocationResult(LocationResult result)
        {

            if (result.Locations.Count != 0)
            {
                MyLocation?.Invoke(this, new OnLocationCaptionEventArgs { Location = result.Locations[0] });
            }
        }

    }
}
