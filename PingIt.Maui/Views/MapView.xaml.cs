// Reusable MapView Control for MAUI, uses LocationDto for pins and optionally shows user location.
//
// Usage as follows:
// add namespace: xmlns:views="clr-namespace:PingIt.Maui.Views;assembly=PingIt.Maui"
// declare mapview in layout with following optional properties:
// - PinItems: IEnumerable<LocationDto> for pins
// - ShowUserLocation: bool to show user's current location
// - SelectedLocation: LocationDto for currently selected pin (TwoWay binding)
// - AllowDetailNavigation: bool to allow navigation on pin selection
// - IsPinSelectionEnabled: bool to allow selecting pins by tapping on the map
// - MapType: MapType enum to set the map type (Street, Satellite, Hybrid)
//
// set following in viewmodel:
// - IEnumerable<LocationDto> YourLocationsCollection
// - LocationDto SelectedLocation (TwoWay binding)
//
// Handle selection changes to navigate or update UI when
// SelectedLocation is set.
//
// The control will automatically update pins when PinItems
// or SelectedLocation changes, and on first location update it
// prioritizes pin centering over user location centering.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public static readonly BindableProperty AllowDetailNavigationProperty =
            BindableProperty.Create(
                nameof(AllowDetailNavigation),
                typeof(bool),
                typeof(MapView),
                true);

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

        private bool _isListening;
        private bool _firstLocationUpdate = true;

        public MapView()
        {
            InitializeComponent();
            Loaded += OnMapViewLoaded;
        }

        private void OnMapViewLoaded(object? sender, EventArgs e)
        {
            // Initial pin setup and centering  
            UpdatePins();
            // Reset flag so first location update won't override pin centering  
            _firstLocationUpdate = true;
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

        public bool AllowDetailNavigation
        {
            get => (bool)GetValue(AllowDetailNavigationProperty);
            set => SetValue(AllowDetailNavigationProperty, value);
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
                System.Diagnostics.Debug.WriteLine($"[MapView] Data changed. Pins: {mapView.PinItems?.Count() ?? 0}");
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
                return;

            InternalMap.IsShowingUser = show;

            if (show && !_isListening)
            {
                // pre‐emptively mark as listening so re‐entrant calls won't jump in
                _isListening = true;

                // wire up the event
                Geolocation.Default.LocationChanged += OnLocationChanged;

                var request = new GeolocationListeningRequest(
                    GeolocationAccuracy.Best,
                    TimeSpan.FromSeconds(1));

                try
                {
                    // this will throw if already listening
                    await Geolocation.Default.StartListeningForegroundAsync(request);
                }
                catch (InvalidOperationException ioe)
                {
                    // Throws, doesn't matter, only log it
                    System.Diagnostics.Debug.WriteLine($"[MapView] StartListening skipped: {ioe.Message}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MapView] Failed to start listening: {ex}");
                    Geolocation.Default.LocationChanged -= OnLocationChanged;
                    _isListening = false;
                }
            }
            else if (!show && _isListening)
            {
                // stop listening and unwind
                Geolocation.Default.LocationChanged -= OnLocationChanged;
                Geolocation.Default.StopListeningForeground();
                _isListening = false;
            }
        }


        private void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
        {
            // Skip first update if we have pins to preserve initial centering  
            if (_firstLocationUpdate && PinItems != null && PinItems.Any())
            {
                _firstLocationUpdate = false;
                return;
            }
            _firstLocationUpdate = false;

            var userLoc = e.Location;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var center = new Location(userLoc.Latitude, userLoc.Longitude);
                InternalMap.MoveToRegion(
                    MapSpan.FromCenterAndRadius(center, Distance.FromMeters(500)));
            });
        }

        private void UpdatePins()
        {
            if (InternalMap == null)
            {
                Dispatcher.Dispatch(async () =>
                {
                    await Task.Delay(100);
                    if (InternalMap != null)
                        UpdatePins();
                });
                return;
            }

            InternalMap.MapType = MapType;
            InternalMap.Pins.Clear();

            if (PinItems != null)
            {
                var list = PinItems.ToList();
                foreach (var dto in list)
                {
                    try
                    {
                        var pin = new Pin
                        {
                            Label = dto.Label ?? "Location",
                            Location = new Location((double)dto.Latitude, (double)dto.Longitude),
                            Type = PinType.Place
                        };
                        pin.MarkerClicked += (s, args) => args.HideInfoWindow = false;
                        pin.InfoWindowClicked += (s, args) =>
                        {
                            if (AllowDetailNavigation)
                                SelectedLocation = dto;
                        };
                        InternalMap.Pins.Add(pin);
                    }
                    catch { }
                }

                // Center on all pins or single pin (big calculation big brain)
                if (list.Count > 1)
                {
                    var minLat = list.Min(p => (double)p.Latitude);
                    var maxLat = list.Max(p => (double)p.Latitude);
                    var minLng = list.Min(p => (double)p.Longitude);
                    var maxLng = list.Max(p => (double)p.Longitude);
                    var centerLat = (minLat + maxLat) / 2;
                    var centerLng = (minLng + maxLng) / 2;
                    var delta = Math.Max(maxLat - minLat, maxLng - minLng);
                    var center = new Location(centerLat, centerLng);
                    var radius = Distance.FromMeters(delta * 111000 / 2);
                    InternalMap.MoveToRegion(MapSpan.FromCenterAndRadius(center, radius));
                }
                else if (list.Count == 1)
                {
                    var dto = list[0];
                    var loc = new Location((double)dto.Latitude, (double)dto.Longitude);
                    InternalMap.MoveToRegion(MapSpan.FromCenterAndRadius(loc, Distance.FromMeters(1000)));
                }
            }

            if (SelectedLocation != null)
            {
                var sel = new Location((double)SelectedLocation.Latitude, (double)SelectedLocation.Longitude);
                InternalMap.Pins.Add(new Pin { Label = "Selected", Location = sel, Type = PinType.Place });
                InternalMap.MoveToRegion(MapSpan.FromCenterAndRadius(sel, Distance.FromMeters(1000)));
            }
        }
        
        void HandleMapClicked(object sender, MapClickedEventArgs e)
        {
            if (!IsPinSelectionEnabled) return;
            SelectedLocation = new LocationDto
            {
                Latitude = (decimal)e.Location.Latitude,
                Longitude = (decimal)e.Location.Longitude
            };
        }
    }
}
