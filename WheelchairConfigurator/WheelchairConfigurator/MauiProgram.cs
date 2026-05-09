using Microsoft.Extensions.Logging;
using ConfigurationLogic;
using WheelchairConfigurator.Data;
using WheelchairConfigurator.Data.Providers;
using WheelchairConfigurator.Data.Repositories;
using WheelchairConfigurator.Service;
using WheelchairConfigurator.ServiceLayer;
using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.Services;
using WheelchairConfigurator.Pages;
using SkiaSharp.Views.Maui.Controls.Hosting;
using WheelchairConfigurator.Export;
using WheelchairConfigurator.Export.Pdf;
using PdfSharp.Fonts;

namespace WheelchairConfigurator
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "wheelchair-configurator.db3");

            // ── DataLayer ─────────────────────────────────────────────────────────────
            var asyncDb = new DbService(dbPath).GetAsyncConnection();

            builder.Services.AddSingleton(_ => new DbService(dbPath));
            builder.Services.AddSingleton(_ => asyncDb);

            builder.Services.AddSingleton<ILocalFileProvider, LocalFileProvider>();
            builder.Services.AddSingleton<JsonDataLoader>();
            builder.Services.AddSingleton<DataService>();
            builder.Services.AddSingleton<DbInitializer>();

            // Repositories registered under their interfaces (required by AppService)
            builder.Services.AddSingleton<ICategoryRepository>(_ => new CategoryRepository(asyncDb));
            builder.Services.AddSingleton<IComponentRepository>(_ => new ComponentRepository(asyncDb));
            builder.Services.AddSingleton<IConfigurationRepository>(_ => new ConfigurationRepository(asyncDb));
            builder.Services.AddSingleton<IConfigurationItemRepository>(_ => new ConfigurationItemRepository(asyncDb));
            builder.Services.AddSingleton<ISpecialistRepository>(_ => new SpecialistRepository(asyncDb));

            // Repositories without interfaces (used directly by pages, not by AppService)
            builder.Services.AddSingleton(_ => new ComponentSpecsRepository(asyncDb));
            builder.Services.AddSingleton(_ => new CompatibilityRuleRepository(asyncDb));

            // ── ExportLayer ───────────────────────────────────────────────────────────
            builder.Services.AddSingleton<IExportFileBuilder>(sp =>
            {
                byte[] logo = Array.Empty<byte>();
                try
                {
                    using var s = FileSystem.OpenAppPackageFileAsync("logo.jpg").GetAwaiter().GetResult();
                    using var ms = new MemoryStream();
                    s.CopyTo(ms);
                    logo = ms.ToArray();
                }
                catch { }
                return new PdfBuilder(logo);
            });

            // ── NavigationState ───────────────────────────────────────────────────────
            builder.Services.AddSingleton<NavigationState>();

            // ── ConfigurationLogic + ServiceLayer ─────────────────────────────────────
            builder.Services.AddSingleton<MainServices>();
            builder.Services.AddSingleton<IConfigurationEngine, ConfigurationEngineAdapter>();
            builder.Services.AddSingleton<IAppService, AppService>();

            // ── Pages (Transient — Shell navigates via DI) ────────────────────────────
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<NewPatientPage>();
            builder.Services.AddTransient<WheelchairConfiguratorPage>();
            builder.Services.AddTransient<SummaryPage>();
            builder.Services.AddTransient<PatientSelectPage>();
            builder.Services.AddTransient<ComponentManagerPage>();

            var app = builder.Build();

            // ── Copy seed_data.json to AppDataDirectory ───────────────────────────────
            var seedDestPath = Path.Combine(FileSystem.AppDataDirectory, "seed_data.json");
            if (!File.Exists(seedDestPath))
            {
                try
                {
                    using var stream = FileSystem.OpenAppPackageFileAsync("seed_data.json").GetAwaiter().GetResult();
                    using var fileStream = File.Create(seedDestPath);
                    stream.CopyTo(fileStream);
                    Console.WriteLine("[MauiProgram] seed_data.json copied to AppDataDirectory");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[MauiProgram] Warning: Could not copy seed_data.json: " + ex.Message);
                }
            }

            // ── Initialize database (seed if empty) ───────────────────────────────────
            try
            {
                app.Services.GetRequiredService<DbInitializer>().Initialize();
                Console.WriteLine("[MauiProgram] Database initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[MauiProgram] DB init failed: " + ex.Message);
            }

            return app;
        }
    }
}
