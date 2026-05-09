using PdfSharp.Fonts;
using WheelchairConfigurator.Data.Repositories;
using WheelchairConfigurator.Domain.Models;
using WheelchairConfigurator.Export.Pdf;

namespace WheelchairConfigurator
{
    public partial class App : Application
    {
        public App(ISpecialistRepository specialistRepo)
        {
            InitializeComponent();
            MainPage = new AppShell();
            _ = InitAsync(specialistRepo);
        }

        private static async Task InitAsync(ISpecialistRepository specialistRepo)
        {
            // Register PDF font if available
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("Roboto-Regular.ttf");
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                PdfFontResolver.Instance.RegisterFont("Roboto", ms.ToArray());
                GlobalFontSettings.FontResolver = PdfFontResolver.Instance;
            }
            catch { }

            // Ensure a default specialist exists (SpecialistId = 1)
            try
            {
                var existing = await specialistRepo.GetAllAsync();
                if (!existing.Any())
                {
                    await specialistRepo.InsertAsync(new Specialist
                    {
                        FirstName = "Terapeut",
                        LastName = "",
                        Email = "terapeut@local"
                    });
                    Console.WriteLine("[App] Default specialist created.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[App] Could not create default specialist: " + ex.Message);
            }
        }
    }
}
