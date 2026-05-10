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
            Routing.RegisterRoute("wheelchairConfiguratorPage", typeof(WheelchairConfiguratorPage));
            Routing.RegisterRoute("summaryPage", typeof(SummaryPage));
            Routing.RegisterRoute("componentManagerPage", typeof(ComponentManagerPage));
            Routing.RegisterRoute("therapistManagerPage", typeof(TherapistManagerPage));
            Routing.RegisterRoute("patientManagerPage", typeof(PatientManagerPage));
            Routing.RegisterRoute("settingsPage", typeof(SettingsPage));
            Routing.RegisterRoute("activityLogPage", typeof(ActivityLogPage));



        }
    }
}
