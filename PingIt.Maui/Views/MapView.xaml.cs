using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using PingIt.Shared.Dtos;

namespace PingIt.Maui.Views
{
    public partial class MapView : ContentView
    {
        public static readonly BindableProperty PinItemsProperty =
            BindableProperty.Create(
                nameof(PinItems),
                typeof(IEnumerable<LocationDto>),
                typeof(MapView),
                null,
                propertyChanged: OnMapDataChanged);

        public static readonly BindableProperty ShowUserLocationProperty =
            BindableProperty.Create(
                nameof(ShowUserLocation),
                typeof(bool),
                typeof(MapView),
                false,
                propertyChanged: (bindable, oldV, newV) =>
                    ((MapView)bindable).OnShowUserLocationChanged((bool)newV));

        public static readonly BindableProperty SelectedLocationProperty =
            BindableProperty.Create(
                nameof(SelectedLocation),
                typeof(LocationDto),
                typeof(MapView),
                null,
                BindingMode.TwoWay,
                propertyChanged: OnMapDataChanged);

        public static readonly BindableProperty IsPinSelectionEnabledProperty =
            BindableProperty.Create(
                nameof(IsPinSelectionEnabled),
                typeof(bool),
                typeof(MapView),
                true);

        public static readonly BindableProperty MapTypeProperty =
            BindableProperty.Create(
                nameof(MapType),
                typeof(MapType),
                typeof(MapView),
                MapType.Street,
                propertyChanged: OnMapTypeChanged);

        public MapView()
        {
            InitializeComponent();

            // Ensure the map is initialized after the component is loaded
            this.Loaded += OnMapViewLoaded;
        }

        private void OnMapViewLoaded(object sender, EventArgs e)
        {
            // Retry updating pins once the view is fully loaded
            UpdatePins();
        }

        public IEnumerable<LocationDto>? PinItems
        {
            get => (IEnumerable<LocationDto>?)GetValue(PinItemsProperty);
            set => SetValue(PinItemsProperty, value);
        }

        public bool ShowUserLocation
        {
            get => (bool)GetValue(ShowUserLocationProperty);
            set => SetValue(ShowUserLocationProperty, value);
        }

        public LocationDto? SelectedLocation
        {
            get => (LocationDto?)GetValue(SelectedLocationProperty);
            set => SetValue(SelectedLocationProperty, value);
        }

        public bool IsPinSelectionEnabled
        {
            get => (bool)GetValue(IsPinSelectionEnabledProperty);
            set => SetValue(IsPinSelectionEnabledProperty, value);
        }

        public MapType MapType
        {
            get => (MapType)GetValue(MapTypeProperty);
            set => SetValue(MapTypeProperty, value);
        }

        static void OnMapDataChanged(BindableObject bindable, object oldVal, object newVal)
        {
            if (bindable is MapView mapView)
            {
                // Add some debugging
                System.Diagnostics.Debug.WriteLine($"[MapView] OnMapDataChanged called. PinItems count: {mapView.PinItems?.Count() ?? 0}");
                mapView.UpdatePins();
            }
        }

        static void OnMapTypeChanged(BindableObject bindable, object oldVal, object newVal)
        {
            if (bindable is MapView mv && mv.InternalMap != null && newVal is MapType mt)
                mv.InternalMap.MapType = mt;
        }

        private async void OnShowUserLocationChanged(bool show)
        {
            if (InternalMap == null)
            {
                System.Diagnostics.Debug.WriteLine("[MapView] InternalMap is null in OnShowUserLocationChanged");
                return;
            }

            InternalMap.IsShowingUser = show;

            if (show)
            {
                try
                {
                    var gps = await Geolocation.GetLastKnownLocationAsync()
                           ?? await Geolocation.GetLocationAsync(
                                 new GeolocationRequest(
                                    GeolocationAccuracy.Medium,
                                    TimeSpan.FromSeconds(10)));

                    if (gps is not null)
                    {
                        var center = new Location(gps.Latitude, gps.Longitude);
                        InternalMap.MoveToRegion(
                            MapSpan.FromCenterAndRadius(center, Distance.FromMeters(500)));
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[MapView] Unable to get user location: {ex}");
                }
            }
        }

        private void UpdatePins()
        {
            if (InternalMap == null)
            {
                System.Diagnostics.Debug.WriteLine("[MapView] InternalMap is null in UpdatePins - deferring update");
                // If the map isn't ready yet, try again after a short delay  
                Dispatcher.Dispatch(async () =>
                {
                    await Task.Delay(100);
                    if (InternalMap != null)
                        UpdatePins();
                });
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[MapView] UpdatePins called. Current pins: {InternalMap.Pins.Count}");

            InternalMap.MapType = MapType;
            InternalMap.Pins.Clear();

            if (PinItems != null)
            {
                var pinItemsList = PinItems.ToList();
                System.Diagnostics.Debug.WriteLine($"[MapView] Adding {pinItemsList.Count} pins");

                foreach (var dto in pinItemsList)
                {
                    try
                    {
                        var pin = new Pin
                        {
                            Label = dto.Label ?? "Location",
                            Location = new Location((double)dto.Latitude, (double)dto.Longitude),
                            Type = PinType.Place
                        };
                        InternalMap.Pins.Add(pin);
                        System.Diagnostics.Debug.WriteLine($"[MapView] Added pin: {pin.Label} at {pin.Location.Latitude}, {pin.Location.Longitude}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[MapView] Error adding pin for {dto.Label}: {ex}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[MapView] Total pins added: {InternalMap.Pins.Count}");

                // Move to show all pins if we have multiple, or center on the first one  
                if (pinItemsList.Count > 1)
                {
                    // Calculate bounds to show all pins  
                    var latitudes = pinItemsList.Select(p => (double)p.Latitude);
                    var longitudes = pinItemsList.Select(p => (double)p.Longitude);

                    var minLat = latitudes.Min();
                    var maxLat = latitudes.Max();
                    var minLng = longitudes.Min();
                    var maxLng = longitudes.Max();

                    var centerLat = (minLat + maxLat) / 2;
                    var centerLng = (minLng + maxLng) / 2;

                    // Calculate appropriate zoom level  
                    var latDelta = Math.Max(maxLat - minLat, 0.01); // Minimum delta for zoom  
                    var lngDelta = Math.Max(maxLng - minLng, 0.01);
                    var maxDelta = Math.Max(latDelta, lngDelta);

                    var center = new Location(centerLat, centerLng);
                    var radius = Distance.FromMeters(maxDelta * 111000 / 2); // Rough conversion from degrees to meters  

                    InternalMap.MoveToRegion(MapSpan.FromCenterAndRadius(center, radius));
                }
                else if (pinItemsList.Count == 1)
                {
                    var first = pinItemsList.First();
                    var loc = new Location((double)first.Latitude, (double)first.Longitude);
                    InternalMap.MoveToRegion(
                        MapSpan.FromCenterAndRadius(loc, Distance.FromMeters(1000)));
                }
            }

            if (SelectedLocation != null)
            {
                var sel = new Location(
                    (double)SelectedLocation.Latitude,
                    (double)SelectedLocation.Longitude);

                var selectedPin = new Pin
                {
                    Label = "Selected",
                    Location = sel,
                    Type = PinType.Place
                };
                InternalMap.Pins.Add(selectedPin);

                System.Diagnostics.Debug.WriteLine($"[MapView] Added selected pin at {sel.Latitude}, {sel.Longitude}");

                InternalMap.MoveToRegion(
                    MapSpan.FromCenterAndRadius(sel, Distance.FromMeters(1000)));
            }
        }

        void HandleMapClicked(object sender, MapClickedEventArgs e)
        {
            if (!IsPinSelectionEnabled)
                return;

            SelectedLocation = new LocationDto
            {
                Latitude = (decimal)e.Location.Latitude,
                Longitude = (decimal)e.Location.Longitude
            };
        }
    }
}