
namespace OneMansSky
{
    public partial class MainPage : ContentPage
    {
        //fields
        double charHeight = 0;
        bool startGame = false;
        Player? player;

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
        protected override void OnAppearing()
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
        }

        private void Start_Button_Clicked(object sender, EventArgs e)
        {
            //initializes game when start is clicked, by creating new player, and sets data binding
            if (!startGame)
            {
                charHeight = GameLayout.Height / 12.0;
                player = new Player(charHeight, GameLayout, GameLayout.Height, GameLayout.Width);
                BindingContext = player;
                this.SetBinding(TitleProperty, "TopInformatiion");
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
