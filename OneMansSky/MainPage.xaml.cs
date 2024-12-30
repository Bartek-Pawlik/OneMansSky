using OneMansSky;
using System.Text.Json;
using static OneMansSky.CelestialBodiesAPI;

namespace OneMansSky
{
    public partial class MainPage : ContentPage
    {
        //fields
        double charHeight = 0;
        bool startGame = false;
        Player? player;
        private CelestialBody? planetToLand;

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

            var bodies = await CelestialBodiesAPI.GetCelestialInfo();

            if (CelestialMap != null && CelestialMap.Children.Count == 0)
            {
                BuildCelestialMap(bodies);
            }
        }

        //func to fill the map with random celestial bodies
        private void BuildCelestialMap(List<CelestialBodiesAPI.CelestialBody> bodies)
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

                        //random number from 1-20 to choose planet image
                        Random randomPlanet = new Random();
                        int randNum = random.Next(1,21);
    
                        //planet image depends on random num above
                        Image planetImage = new Image
                        {
                            Source = ImageSource.FromFile("planet" + randNum + ".png"),
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                            WidthRequest = 60,
                            HeightRequest = 60,

                            BindingContext = body
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

                IsPlanetHover();
            }

        }

        //func to check if players ship is hovering over a planet, to display land button
        //for calculating the distance i used: https://stackoverflow.com/questions/61237236/trying-to-find-the-distance-between-2-points, https://stackoverflow.com/questions/26157199/how-to-calculate-distance-between-2-coordinates
        private void IsPlanetHover()
        {
            //get center coordinates of the player
            double playerCenterX = player.Position.X + player.Position.Width / 2;
            double playerCenterY = player.Position.Y + player.Position.Height / 2;

            CelestialBody hoveredPlanet = null;
            double hoverDistanceThreshold = 50; //how close the player needs to be to be considered hovering over the planet

            //loop to go through all the children in the map
            foreach (var child in CelestialMap.Children)
            {
                if (child is Image planetImage && planetImage.BindingContext is CelestialBody planetData)
                {
                    //gets grid location of the planet
                    int row = Grid.GetRow(planetImage);
                    int col = Grid.GetColumn(planetImage);

                    //gets diameters of the cell, and the center of which grid cell the planet is in
                    double width = CelestialMap.Width / NumCols;
                    double height = CelestialMap.Height / NumRows;
                    double planetCenterX = (col + 0.5) * width;
                    double planetCenterY = (row + 0.5) * height;

                    //calculating the distance between the players center and center of planet
                    double dx = planetCenterX - playerCenterX;
                    double dy = planetCenterY - playerCenterY;
                    double distance = Math.Sqrt(dx * dx + dy * dy);

                    //then if player is close enough to the planet it will allow them to land on it
                    if (distance <= hoverDistanceThreshold)
                    {
                        hoveredPlanet = planetData;
                        break;
                    }
                }

            }

            //displays land button
            if (hoveredPlanet != null)
            {
                LandButton.IsVisible = true; 
                planetToLand = hoveredPlanet;
            }
            else
            {
                LandButton.IsVisible = false;
                planetToLand = null;
            }

        }

        //when land is clicked displays info page 
        private async void LandButton_Clicked(object sender, EventArgs e)
        {
            if (planetToLand != null)
            {
                var detailsPage = new PlanetDetails(planetToLand);
                await Navigation.PushModalAsync(detailsPage, true);
            }
        }

    }
}
