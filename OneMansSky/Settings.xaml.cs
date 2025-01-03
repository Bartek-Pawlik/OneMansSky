using Microsoft.Maui.Storage; // for Preferences
using System.ComponentModel;

namespace OneMansSky
{
    public partial class Settings : ContentPage
    {

        public Settings()
        {
            InitializeComponent();

            //load the saved difficulty
            string savedDifficulty = Preferences.Get("GameDifficulty", "Normal");

            //set the picker to the saved difficulty
            Difficulty.SelectedItem = savedDifficulty;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            //gets the selected difficulty
            string selectedDifficulty = Difficulty.SelectedItem?.ToString() ?? "Normal";

            //saves the difficulty to preferences
            Preferences.Set("GameDifficulty", selectedDifficulty);

            //sets the difficulty for the meteor class
            Meteor.SetDifficulty(selectedDifficulty);

            await Navigation.PopModalAsync();
        }
    }
}
