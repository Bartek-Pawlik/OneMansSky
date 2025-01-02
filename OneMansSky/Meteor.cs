using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System.Timers;

//a good amount of code here was based on the collision part we did in the Dodge game lab
//the collision is still be buggy sometimes, but it works most of the time

namespace OneMansSky
{
    public class Meteor
    {
        //fields for all meteors
        private static double size;
        private static double height;
        private static double width;
        private static Player? playerRef;
        private static AbsoluteLayout? mainLayout;
        private static IDispatcher? mainDispatcher;
        private static MainPage? mainPage; //to end game when player is hit
        private static readonly Random rand = new();
        private Grid box;
        private Image image;
        private System.Timers.Timer timer;
        private Task<bool>? animation;
        private bool top;

        //setting static properties for all meteors
        public static void SetStaticProperties(double s, Player p, AbsoluteLayout layout, IDispatcher dispatcher, MainPage page)
        {
            size = s;
            playerRef = p;
            mainLayout = layout;
            mainDispatcher = dispatcher;
            mainPage = page;
            height = layout.Height;
            width = layout.Width;
        }

        //making sure no meteors spawn after page closes
        public static void RemoveLayoutRef()
        {
            mainLayout = null;
        }

        //constructor for meteors
        public Meteor()
        {
            //ensure these are not null before creating meteors
            if (mainLayout == null || playerRef == null)
            {
                return;
            }

            //randomly choose if meteor will come from top or right, 50% chance for either
            top = (rand.Next(2) == 0);

            //grid and image for meteors
            box = new Grid
            {
                WidthRequest = size,
                HeightRequest = size,
                BackgroundColor = Colors.Transparent
            };
            image = new Image
            {
                WidthRequest = size + size / 2,
                HeightRequest = size + size / 2,
                Source = ImageSource.FromFile("meteor.png")
            };
            box.Add(image);

            mainLayout.Add(box);

            double x, y;
            //if meteor comes from top, set rotation to -45 and random position, same for if coming from the side
            if (top)
            {
                image.Rotation = -45; 
                x = rand.NextDouble() * (width - size);
                y = 0;
            }
            else
            {
                image.Rotation = 45;
                x = width - size;
                y = rand.NextDouble() * (height - size);
            }
            box.TranslationX = x;
            box.TranslationY = y;

            //timer for collision checking every 16ms (60fps)
            timer = new System.Timers.Timer { Interval = 16 };
            timer.Elapsed += (s, e) => UpdateCollision();

            //start moving meteor
            Task.Run(() => StartMove());
        }

       
        private async Task StartMove()
        {
            //delay before the meteors starts moving, between 200ms and 800ms
            int randDelay = rand.Next(200, 800);
            await Task.Delay(randDelay);

            //random speed between 2000 and 3000ms for meteor to move across screen
            uint randSpeed = (uint)rand.Next(2000, 3000);

            //move meteor depending where it spawns, top goes down, side goes left
            if (top)
            {
                animation = box.TranslateTo(box.TranslationX, height, randSpeed);
            }
            else
            {
                animation = box.TranslateTo(-size, box.TranslationY, randSpeed);
            }

            //check for collisions with ship
            timer.Start();
            await animation;

            //if meteor goes off screen, removes it
            if (box.TranslationY >= height || box.TranslationX <= -size)
            {
                FullyRemoveMeteor();
            }
        }

        //func to check for collisions, collisions are still slightly buggy, i dont really know why
        private void UpdateCollision()
        {
            Rect playerRect = playerRef.Position;

            if (playerRef == null || mainLayout == null)
            {
                return;
            }

            //get meteor size
            double meteorW = box.WidthRequest;  
            double meteorH = box.HeightRequest;

            //i tried to make the meteor hitbox smaller but its still kind of buggy sometimes
            double shrink = 0.8; 
            double shrinkWidth = meteorW * shrink;
            double shrinkHeight = meteorH * shrink;

            //meteor hitbox
            Rect meteorRect = new Rect(
                box.TranslationX + shrinkWidth / 2,
                box.TranslationY + shrinkHeight / 2, 
                size - shrinkWidth, 
                size - shrinkHeight
                );

            //checks for collision
            if (meteorRect.IntersectsWith(playerRect))
            {
                
                box.CancelAnimations();

                //if theyre hit, call GotHit(), and end the game
                mainDispatcher?.Dispatch(async () =>
                {
                    playerRef.GotHit();

                    if (mainPage != null)
                    {
                        await mainPage.EndGame();
                    }
                });

                FullyRemoveMeteor();
            }
        }

        //func to remove meteors
        public void FullyRemoveMeteor()
        {
            //remove meteor from screen
            mainDispatcher?.Dispatch(() =>
            {
                mainLayout?.Remove(box);
            });

            timer.Stop();
            timer.Dispose();
        }
    }
}

