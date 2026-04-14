using ConfigurationLogic;

namespace WheelchairConfigurator.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        /*
         * OnToNewPatientClicked - toNewPatientBtn, redirects to NewPatientPage
         */
        private async void OnToNewPatientClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("newPatientPage");
        }

        /*
         * OnToPatientSelectClicked - toPatientSelectBtn, redirects to PatientSelectPage
         */
        private async void OnToPatientSelectClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("patientSelectPage");
        }

        /*
         * OnToWheelchairSelectClicked - toWheelchairSelectBtn, redirects to WheelchairSelectPage
         */

        private async void OnToWheelchairSelectClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("wheelchairSelectPage");
        }

        /*
         * OnExitClicked - ExitBtn handler, closes application
         */
        private void OnExitClicked(object sender, EventArgs e)
        {
            Application.Current?.CloseWindow(Application.Current?.MainPage?.Window!);
        }
    }

}
