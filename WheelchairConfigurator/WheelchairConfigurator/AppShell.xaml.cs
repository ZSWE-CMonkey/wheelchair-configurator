using WheelchairConfigurator.Pages;

namespace WheelchairConfigurator
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("newPatientPage", typeof(NewPatientPage));
            Routing.RegisterRoute("mainPage", typeof(MainPage));
            Routing.RegisterRoute("patientSelectPage", typeof(PatientSelectPage));

        }
    }
}
