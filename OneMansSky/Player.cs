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

        public Command ShootCommand { get; }
        private AbsoluteLayout mainLayout;

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
        public Player(double size, AbsoluteLayout mainl, double maxh, double maxw)
        {

            box = new Grid()
            {
                WidthRequest = size,
                HeightRequest = size,
                BackgroundColor = Colors.Transparent,
            };

            //creating players image
            image = new Image()
            {
                WidthRequest = size + size / 20,
                HeightRequest = size + size / 20,
                Source = ImageSource.FromFile("rocket.png"),
                Rotation = 360
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

            speed = size / 300.0;
            Allowmoves = false;

            ShootCommand = new Command(async () => await ShootLaser());

            StartPlayer();
        }

        //func to shoot gun / laser, collision will be added later
        private async Task ShootLaser()
        {
            if (!Allowmoves)
            {
                return;
            }
            Image laser = new Image
            {
                Source = ImageSource.FromFile("laser.png"),
                //laser size
                WidthRequest = size * 2,   
                HeightRequest = size, 
            };

            //calculating lasers positions, x centering on players center, y placing it just above player, finalY setting destination
            double laserX = box.TranslationX + (size / 2) - (laser.WidthRequest / 2);
            double laserY = box.TranslationY - laser.HeightRequest;
            double finalY = -laser.HeightRequest;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                mainLayout.Add(laser);
                AbsoluteLayout.SetLayoutFlags(laser, AbsoluteLayoutFlags.None);
                AbsoluteLayout.SetLayoutBounds(laser, new Rect(laserX, laserY, laser.WidthRequest, laser.HeightRequest));
            });

            uint duration = 1000;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                //set x to 0 so no horizontal movement with laser
                await laser.TranslateTo(0, finalY - laserY, duration);
            });

            MainThread.BeginInvokeOnMainThread(() =>
            {
                mainLayout.Remove(laser);
            });
        }

        public async void StartPlayer()
        {
            if (!allowmoves)
            {
                //starting player in bottom center of screen
                box.TranslationX = (maxWidth - size) / 2;
                box.TranslationY = (maxHeight - size);
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
            pos.Y = box.TranslationY;
            pos.X = box.TranslationX;
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task MovePlayer(Point p)
        {
            if (allowmoves)
            {
                //canceling any current animations to before new one
                if (animation != null)
                {
                    box.CancelAnimations();
                }
                //ensures the players center goes to target point
                p.X -= size / 2;
                p.Y -= size / 2;

                //calculate distance and animation time
                double distance = p.Distance(new Point(pos.X, pos.Y));
                uint time = (uint)(distance / speed);

                animation = box.TranslateTo(p.X, p.Y, time);
                await animation;
                animation = null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
