using System.ComponentModel;
using System.Windows.Input;

namespace OneMansSky
{
    public partial class PlanetDetails : ContentPage, INotifyPropertyChanged
    {
        public double PopupWidth { get; set; }
        public double PopupHeight { get; set; }
        
        private CelestialBodiesAPI.CelestialBody planet;

        public CelestialBodiesAPI.CelestialBody Planet 
        {
            get => planet;
            set
            {
                planet = value;
                OnPropertyChanged(nameof(Planet));
            }
        }

        public ICommand CloseCommand { get; set; }

        //constructor that gets screen dimensions and sets the popup size to be smaller
        public PlanetDetails()
        {
            InitializeComponent();

            var screenWidth = DeviceDisplay.Current.MainDisplayInfo.Width;
            var screenHeight = DeviceDisplay.Current.MainDisplayInfo.Height;

            PopupWidth = screenWidth * 0.4;
            PopupHeight = screenHeight * 0.4;

            //command to close the details pop up
            CloseCommand = new Command(async () => await ClosePopup());

            BindingContext = this;
        }

        //OnAppearing to load a random planet when the the details page pops up
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadRandomPlanetAsync();

            BindingContext = this;
        }

        //picks the random planet / celestial body to display
        private async Task LoadRandomPlanetAsync()
        {
            var allBodies = await CelestialBodiesAPI.GetCelestialInfo();
            if (allBodies?.Any() != true)
            {
                Planet = new CelestialBodiesAPI.CelestialBody
                {
                    englishName = "Unknown",
                    discoveryDate = "N/A",
                    bodyType = "N/A"
                };
                return;
            }

            var random = new Random();
            Planet = allBodies[random.Next(allBodies.Count)];

        }
        //command to close pop up
        private async Task ClosePopup()
        {
            await Navigation.PopModalAsync();
        }
    }
}
