using Microsoft.Maui.Storage;
using System.ComponentModel;
using System.Text.Json;
using System.Windows.Input;

namespace OneMansSky
{
    public partial class PlanetDetails : ContentPage, INotifyPropertyChanged
    {
        public double PopupWidth { get; set; }
        public double PopupHeight { get; set; }

        private static List<string> discoveredBody  = new List<string>();
        private bool newDiscovery;

        public bool NewDiscovery
        {
            get => newDiscovery;

            set 
            {
                newDiscovery = value;

                OnPropertyChanged(nameof(newDiscovery));

                OnPropertyChanged(nameof(DiscoveredMessage));
            }
        }

        //for getting message if planet was already discovered or not
        public string DiscoveredMessage
        {
            get => NewDiscovery ? "New Discovery" : "Already Discovered!";
        }

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
        public PlanetDetails(CelestialBodiesAPI.CelestialBody selectedPlanet)
        {
            InitializeComponent();
            LoadDiscoveredBodies();

            var screenWidth = DeviceDisplay.Current.MainDisplayInfo.Width;
            var screenHeight = DeviceDisplay.Current.MainDisplayInfo.Height;

            PopupWidth = screenWidth * 0.4;
            PopupHeight = screenHeight * 0.4;

            //command to close the details pop up
            CloseCommand = new Command(async () => await ClosePopup());

            Planet = selectedPlanet;

            //checks if a planet is discovered for the first time
            NewDiscovery = !discoveredBody.Contains(Planet.englishName);

            //if its a new discovery add that name to discoverd bodies
            if (NewDiscovery)
            {
                discoveredBody.Add(Planet.englishName);
                SaveDiscoveredBodies();
            }

            BindingContext = this;
        }

        //OnAppearing to load a random planet when the the details page pops up
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = this;
        }

        //func to save all the bodies that have been discovered
        private async Task SaveDiscoveredBodies()
        {
            string fileName = Path.Combine(FileSystem.AppDataDirectory, "discoveredBodies.json");
            string json = JsonSerializer.Serialize(discoveredBody);

            await File.WriteAllTextAsync(fileName, json);
        }

        //func to load all discovered bodies once the game is started
        private async Task LoadDiscoveredBodies()
        {
            string fileName = Path.Combine(FileSystem.AppDataDirectory, "discoveredBodies.json");
            
            if(File.Exists(fileName))
            {
                string json = await File.ReadAllTextAsync(fileName);
                discoveredBody = JsonSerializer.Deserialize<List<string>> (json) ?? new List<string>();
            }
        }

        //command to close pop up
        private async Task ClosePopup()
        {
            await Navigation.PopModalAsync();
        }
    }
}
