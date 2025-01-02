using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using System.ComponentModel;


/* most code here and in MainPage.cs was used from my DodgeGame lab,
 * with changes to it to create a basic layout for the space
 * exploration game.
 */ 

namespace OneMansSky
{
    //player class to allow user move around map
    public class Player : INotifyPropertyChanged
    {
        //fields
        Task<bool>? animation = null;
        private double size;
        private Grid box;
        private Image image;
        private System.Timers.Timer timer;
        private double maxHeight, maxWidth;
        private bool overedge = false;
        private double speed;
        private AbsoluteLayout mainLayout;
        private MainPage mainPage;
        public event PropertyChangedEventHandler? PropertyChanged;

        //properties
        private Rect pos;
        public Rect Position
        {
            get
            {
                return pos;
            }
        }

        private bool allowmoves = false;

        public bool Allowmoves
        {
            get => allowmoves;
            set
            {
                allowmoves = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EnableButton));
            }
        }
        public bool EnableButton
        {
            get
            {
                return !allowmoves;
            }
        }

        // constructor, initializing new player.
        public Player(double size, AbsoluteLayout mainl, double maxh, double maxw, MainPage mainPage)
        {

            box = new Grid()
            {
                WidthRequest = size / 3,
                HeightRequest = size / 3,
                BackgroundColor = Colors.Transparent,
            };

            //creating players image
            image = new Image()
            {
                WidthRequest = size + size / 20,
                HeightRequest = size + size / 20,
                Source = ImageSource.FromFile("rocket.png"),
            };
            box.Add(image);

            maxHeight = maxh;
            maxWidth = maxw;
            this.size = size;

            mainLayout = mainl;
            mainl.Add(box);
            //tracks players x, y on the layout
            pos = new Rect(0, 0, size, size);
            //timer to update every 16ms
            timer = new System.Timers.Timer
            {
                Interval = 16
            };
            timer.Elapsed += (s, e) =>
            {
                Update();
            };

            speed = size / 200.0;
            Allowmoves = false;

            StartPlayer();
        }


        public async void StartPlayer()
        {
            if (!allowmoves)
            {
                //starting player in center of screen
                box.TranslationX = (maxWidth - size) / 2;
                box.TranslationY = (maxHeight - size) / 2;
                pos.X = box.TranslationX;
                pos.Y = box.TranslationY;
                Allowmoves = true;
                timer.Start();
            }
        }

        //updating players position every 16ms
        private void Update()
        {
            if (box.TranslationY < 0 || box.TranslationX < 0 || box.TranslationY > maxHeight - size || box.TranslationX > maxWidth - size)
            {
                if (animation != null)
                {
                    overedge = true;
                    box.CancelAnimations();
                }
            }
        }

        //func to change player image when they're hit to an explosion
        public async void GotHit()
        {
            Image explode = new Image()
            {
                WidthRequest = size * 2,
                HeightRequest = size * 2,
                Source = ImageSource.FromFile("explosion.png"),
            };

            box.Add(explode);
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //moving the ship can still be buggy, and you can get faster or slower speeds depending where you click.
        public async Task MovePlayer(Point p)
        {
            if (allowmoves)
            {
                //canceling any current animations to before new one
                if (animation != null)
                    box.CancelAnimations();

                //ensures the players center goes to target point
                p.X -= size / 2;
                p.Y -= size / 2;

                //calculate distance and animation time
                double distance = p.Distance(new Point(pos.X, pos.Y));
                uint time = (uint)(distance / speed);

                //avoiding instant moving of the spaceship if clicked fast
                time = Math.Max(time, 1000);

                animation = box.TranslateTo(p.X, p.Y, time);
                await animation;

                pos.X = p.X;
                pos.Y = p.Y;

                animation = null;
            }
        }
    }
}
