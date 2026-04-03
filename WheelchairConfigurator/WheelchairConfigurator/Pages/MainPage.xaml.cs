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
         * OnToConfiguratorClicked - toConfiguratorBtn, redirects to ConfiguratorPage1
         */
        private async void OnToConfiguratorClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("configurator");
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
