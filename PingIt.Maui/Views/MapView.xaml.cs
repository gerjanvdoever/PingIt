using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using PingIt.Shared.Dtos;

namespace PingIt.Maui.Views
{
    /*
     MapView Usage:
     ----------------
     In your XAML page:
     
       xmlns:views="clr-namespace:PingIt.Maui.Views"
       xmlns:maps="http://schemas.microsoft.com/dotnet/2021/maui/maps"
     
       <views:MapView
           HeightRequest="200"
           PinItems="{Binding PinItems}"
           ShowUserLocation="{Binding ShowUserLocation}"
           SelectedLocation="{Binding SelectedLocation, Mode=TwoWay}"
           IsPinSelectionEnabled="True"
           MapType="Hybrid" />
     
     Properties:
       • PinItems (IEnumerable<LocationDto>): the pins to display
       • SelectedLocation (LocationDto?, TwoWay): tapped location
       • ShowUserLocation (bool): blue-dot current position
       • IsPinSelectionEnabled (bool): enable/disable tap-to-add
       • MapType (MapType): Street, Satellite, or Hybrid
     */

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
                false);

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

        public MapView() => InitializeComponent();

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
            ((MapView)bindable).UpdatePins();
        }

        static void OnMapTypeChanged(BindableObject bindable, object oldVal, object newVal)
        {
            if (bindable is MapView mv && mv.InternalMap != null && newVal is MapType mt)
                mv.InternalMap.MapType = mt;
        }

        void UpdatePins()
        {
            if (InternalMap == null)
                return;

            InternalMap.MapType = MapType;
            InternalMap.Pins.Clear();

            if (PinItems != null)
            {
                foreach (var dto in PinItems)
                {
                    InternalMap.Pins.Add(new Pin
                    {
                        Label = dto.Label ?? "Location",
                        Location = new Location((double)dto.Latitude, (double)dto.Longitude),
                        Type = PinType.Place
                    });
                }

                var first = PinItems.FirstOrDefault();
                if (first != null)
                {
                    var center = new Location((double)first.Latitude, (double)first.Longitude);
                    InternalMap.MoveToRegion(MapSpan.FromCenterAndRadius(center, Distance.FromMeters(500)));
                }
            }

            if (SelectedLocation != null)
            {
                var sel = new Location((double)SelectedLocation.Latitude, (double)SelectedLocation.Longitude);
                InternalMap.Pins.Add(new Pin
                {
                    Label = "Selected",
                    Location = sel,
                    Type = PinType.Place
                });
                InternalMap.MoveToRegion(MapSpan.FromCenterAndRadius(sel, Distance.FromMeters(500)));
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
