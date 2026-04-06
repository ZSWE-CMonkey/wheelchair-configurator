using WheelchairConfigurator.Pages;

namespace WheelchairConfigurator
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("configurator", typeof(ConfiguratorPage1));

        }
    }
}
