using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using PingIt.Shared.Dtos;

namespace PingIt.Maui.Views;

public partial class MapView : ContentView
{

    public static readonly BindableProperty PinItemsProperty =
      BindableProperty.Create(
        nameof(PinItems),
        typeof(IEnumerable<LocationDto>),
        typeof(MapView),
        propertyChanged: OnPinsChanged);

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
        propertyChanged: OnSelectedLocationChanged);

    public IEnumerable<LocationDto> PinItems
    {
        get => (IEnumerable<LocationDto>)GetValue(PinItemsProperty);
        set => SetValue(PinItemsProperty, value);
    }

    public bool ShowUserLocation
    {
        get => (bool)GetValue(ShowUserLocationProperty);
        set => SetValue(ShowUserLocationProperty, value);
    }

    public LocationDto SelectedLocation
    {
        get => (LocationDto)GetValue(SelectedLocationProperty);
        set => SetValue(SelectedLocationProperty, value);
    }

    public MapView()
    {
        InitializeComponent();
    }

    private static void OnSelectedLocationChanged(BindableObject bindable, object oldVal, object newVal)
    {
        var control = (MapView)bindable;
        control.InternalMap.Pins.Clear();

        if (newVal is LocationDto dto)
        {
            // 1) Create MAUI Location + pin
            var loc = new Location(
                          (double)dto.Latitude,
                          (double)dto.Longitude);
            var pin = new Pin
            {
                Label = "Gekozen locatie",
                Location = loc,
                Type = PinType.Place
            };
            control.InternalMap.Pins.Add(pin);

            // 2) Center & zoom the map on it
            control.InternalMap.MoveToRegion(
                MapSpan.FromCenterAndRadius(loc, Distance.FromMeters(500)));
        }
    }

    private void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        // 1) Update the bound SelectedLocation
        SelectedLocation = new LocationDto
        {
            Latitude = (decimal)e.Location.Latitude,
            Longitude = (decimal)e.Location.Longitude
        };

        // 2) Show a pin at that spot
        InternalMap.Pins.Clear();
        var pin = new Pin
        {
            Label = "Chosen position",
            Location = e.Location,
            Type = PinType.Place
        };
        InternalMap.Pins.Add(pin);
    }

    private static void OnPinsChanged(BindableObject b, object oldVal, object newVal)
    {
        var control = (MapView)b;
        control.InternalMap.Pins.Clear();
        if (newVal is IEnumerable<LocationDto> list)
        {
            foreach (var dto in list)
            {
                control.InternalMap.Pins.Add(new Pin
                {
                    Location = new Location(
                               (double)dto.Latitude,
                               (double)dto.Longitude)
                });
            }
        }
    }
}