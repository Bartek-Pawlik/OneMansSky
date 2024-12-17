using OneMansSky;
using System.Text.Json;

namespace OneMansSky
{
    public partial class MainPage : ContentPage
    {
        //fields
        double charHeight = 0;
        bool startGame = false;
        Player? player;

        private const int NumRows = 10;
        private const int NumCols = 10;

        //constructor
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            //setting game area
            base.OnSizeAllocated(width, height);
            GameLayout.WidthRequest = width;
            GameLayout.HeightRequest = height - Bottom.Height;
        }

        //setting windows height and width
        //doing nothing if on android
        protected override async void OnAppearing()
        {
            base.OnAppearing();

#if WINDOWS
            this.Window.X = 0;
            this.Window.Y = 0;
            this.Window.MaximumHeight = DeviceDisplay.Current.MainDisplayInfo.Height - 50;
            this.Window.MinimumHeight = DeviceDisplay.Current.MainDisplayInfo.Height - 50;
            this.Window.MaximumWidth = DeviceDisplay.Current.MainDisplayInfo.Width;
            this.Window.MinimumWidth = DeviceDisplay.Current.MainDisplayInfo.Width;
#endif

            var bodies = await CelestialDataService.GetCelestialInfo();

            if (CelestialMap != null && CelestialMap.Children.Count == 0)
            {
                BuildCelestialMap(bodies);
            }
        }

        //func to fill the map with random celestial bodies
        private void BuildCelestialMap(List<CelestialDataService.CelestialBody> bodies)
        {
            //clears all rows and cols first
            CelestialMap.RowDefinitions.Clear();
            CelestialMap.ColumnDefinitions.Clear();

            //creates row and col definitions for the grid
            for (int r = 0; r < NumRows; r++)
                CelestialMap.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            for (int c = 0; c < NumCols; c++)
                CelestialMap.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Random random = new Random();

            //loops through each row and col
            for (int row = 0; row < NumRows; row++)
            {
                for (int col = 0; col < NumCols; col++)
                {
                    //random value from 0 to 1
                    double chance = random.NextDouble();
                    //5% chance for a cell to have something in it (may be tweaked later)
                    if (chance > 0.95) 
                    {
                        //selects random body from bodies list
                        int index = random.Next(bodies.Count);
                        var body = bodies[index];

                        //placeholder image for now, more will be added later
                        Image planetImage = new Image
                        {
                            Source = ImageSource.FromFile("planettest.png"),
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                            WidthRequest = 60,   
                            HeightRequest = 60
                        };
                        //adding body to the map
                        CelestialMap.Add(planetImage, col, row);
                    }
                }
            }
        }

        private void Start_Button_Clicked(object sender, EventArgs e)
        {
            //initializes game when start is clicked, by creating new player, and sets data binding
            if (!startGame)
            {
                charHeight = GameLayout.Height / 12.0;
                player = new Player(charHeight, GameLayout, GameLayout.Height, GameLayout.Width);
                BindingContext = player;
                startGame = true;
            }
        }

        //func for moving the player to tapped position
        private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            if (player != null)
            {
                await player.MovePlayer((Point)e.GetPosition(GameLayout));
            }

        }

    }

 }
