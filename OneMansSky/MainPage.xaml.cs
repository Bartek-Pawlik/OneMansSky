using OneMansSky;
using System;
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
        private int planetCount = 0;
        private static List<string> discoveredPlanets = new List<string>();
        private bool isShowerActive = false;

        private const int NumRows = 10;
        private const int NumCols = 10;


        //constructor to set binding and load difficulty
        public MainPage()
        {
            InitializeComponent();
           
            BindingContext = this;

            string savedDifficulty = Preferences.Get("Difficulty", "Normal");
            Meteor.SetDifficulty(savedDifficulty);
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
            planetCount = 0;

            //clears all rows and cols first
            CelestialMap.RowDefinitions.Clear();
            CelestialMap.ColumnDefinitions.Clear();
            CelestialMap.Children.Clear();

            //creates row and col definitions for the grid
            for (int r = 0; r < NumRows; r++)
            {
                CelestialMap.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            for (int c = 0; c < NumCols; c++)
            {
                CelestialMap.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            Random random = new Random();

            //loops through each row and col
            for (int row = 0; row < NumRows; row++)
            {
                for (int col = 0; col < NumCols; col++)
                {
                    double chance2 = random.NextDouble();
                    //random value from 0 to 1
                    double chance = random.NextDouble();
                    //5% chance for a cell to have something in it (may be tweaked later)
                    if (chance > 0.95)
                    {
                        //selects random body from bodies list
                        int index = random.Next(bodies.Count);
                        var body = bodies[index];

                        //random number from 1-20 to choose planet image
                        int randNum = random.Next(1, 21);

                        //planet image depends on random num
                        Image planetImage = new Image
                        {
                            Source = ImageSource.FromFile("planet" + randNum + ".png"),
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center,
                            WidthRequest = 60,
                            HeightRequest = 60,

                            BindingContext = body
                        };
                        planetCount++;
                        //adding body to the map

                        CelestialMap.Add(planetImage, col, row);
                    }
                }
            }
            CheckIfPlanetsDiscovered();
        }

        //func to check if all planets visible have been discovered
        public void CheckIfPlanetsDiscovered()
        {
            int discoveredCount = 0;

            //goes through every cell to check if cells are an image and if binding is celesialbodies
            foreach (var child in CelestialMap.Children)
            {
                if (child is Image planetImage && planetImage.BindingContext is CelestialBodiesAPI.CelestialBody planetInfo)
                {
                    //if already discoverd, increase count
                    if (PlanetDetails.IsDiscovered(planetInfo.englishName))
                    {
                        discoveredCount++;
                    }
                }
            }
            //when all planets are discovered display travel button
            TravelButton.IsVisible = (discoveredCount == planetCount && planetCount > 0);
        }


        private void Start_Button_Clicked(object sender, EventArgs e)
        {
            //initializes game when start is clicked, by creating new player, and sets data binding
            if (!startGame)
            {
                charHeight = GameLayout.Height / 12.0;
                player = new Player(charHeight, GameLayout, GameLayout.Height, GameLayout.Width, this);
                BindingContext = player;
                startGame = true;
                player.Score = 0;//reset this games score to 0

                //sets static properties for all meteors, general idea from: https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments
                Meteor.SetStaticProperties(s: charHeight, p: player, layout: GameLayout, dispatcher: Dispatcher, page: this);
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
            if (player != null)
            {
                //get center coordinates of the player
                double playerCenterX = player.Position.X + player.Position.Width / 2;
                double playerCenterY = player.Position.Y + player.Position.Height / 2;

                CelestialBody hoveredPlanet = null;
                double hoverDistanceThreshold = 50; //how close the player needs to be to be considered hovering over the planet

                //loop to go through all the children in the map
                foreach (var child in CelestialMap.Children)
                {
                    if (child is Image planetImage && planetImage.BindingContext is CelestialBody planetInfo)
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
                            hoveredPlanet = planetInfo;
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

        }

        //when land is clicked displays info page 
        private async void LandButton_Clicked(object sender, EventArgs e)
        {
            if (planetToLand != null)
            {
                var detailsPage = new PlanetDetails(planetToLand);
                await Navigation.PushModalAsync(detailsPage, true);

                //for counting player score, if theyre the same, both will go up else only this games score goes up
                if(player.Score < player.HighScore)
                {
                    player.Score++;
                }
                else if(player.Score == player.HighScore)
                {
                    player.HighScore++;
                    player.Score++;
                }

                Random r = new Random();
                if (r.NextDouble() < 0.90)
                {
                    //90% chance for now (will be much less in final version)
                    StartMeteorShower();
                }

                CheckIfPlanetsDiscovered();
            }
        }

        //func to start meteor shower
        private async void StartMeteorShower()
        {
            //will spawn meteors depending on diffuculty selected
            int showerDuration = Meteor.GetShowerDuration();
            int spawnInterval = Meteor.GetSpawnInterval(); 
            isShowerActive = true;

            int elapsed = 0;
            while (elapsed < showerDuration && isShowerActive)
            {
                //discard used from: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/discards 
                //as i wont be using the meteor object
                _ = new Meteor();

                //waits for the spawn interval
                await Task.Delay(spawnInterval);
                elapsed += spawnInterval;
            }
        }

        //func to end the game
        public async Task EndGame()
        {
            //sets game to false, doesnt allow moves, and displays game over message
            isShowerActive = false;
            if (player != null)
            {
                startGame = false;
                player.Allowmoves = false;
                await DisplayAlert("Game Over", "Your ship was destroyed by a meteor!", "OK");
                GameLayout.Children.Clear();
                LandButton.IsVisible = false;
                TravelButton.IsVisible = false;
                player = null;
            }
        }

        //func to "travel" to another part of space to explore more planets
        private async void TravelButton_Clicked(object sender, EventArgs e)
        {
            await GameLayout.FadeTo(0, 500);
            var bodies = await CelestialBodiesAPI.GetCelestialInfo();
            BuildCelestialMap(bodies);

            //moves the player back to the middle of the screen, after "travelling"
            if (player != null)
            {
                await player.MovePlayer(new Point(GameLayout.Width / 2, GameLayout.Height / 2));
            }

            TravelButton.IsVisible = false;

            await GameLayout.FadeTo(1, 500);
        }

        //button that opens settings page
        private async void Settings_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new Settings(), true);
        }
    }
}
