using Microsoft.Extensions.Logging;
using ConfigurationLogic;
using ConfigurationLogic.ManualTests;
using WheelchairConfigurator.Data;
using WheelchairConfigurator.Data.Repositories;
using WheelchairConfigurator.ServiceLayer;
using WheelchairConfigurator.ServiceLayer.Interfaces;

namespace WheelchairConfigurator
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "wheelchair-configurator.db3");

            /*
            try
            {
                var seedDestPath = Path.Combine(FileSystem.AppDataDirectory, "seed_data.json");
                if (!File.Exists(seedDestPath))
                {
                    using var stream = FileSystem.OpenAppPackageFileAsync("seed_data.json").Result;
                    using var fileStream = File.Create(seedDestPath);
                    stream.CopyTo(fileStream);
                    Console.WriteLine("[MauiProgram] seed_data.json copied to AppDataDirectory");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[MauiProgram] Warning: Could not copy seed_data.json: " + ex.Message);
            }
            */

            builder.Services.AddSingleton(_ => new DbService(dbPath));
            builder.Services.AddSingleton(sp => sp.GetRequiredService<DbService>().GetAsyncConnection());

            builder.Services.AddSingleton<CategoryRepository>();
            builder.Services.AddSingleton<ComponentRepository>();
            builder.Services.AddSingleton<ComponentSpecsRepository>();
            builder.Services.AddSingleton<CompatibilityRuleRepository>();
            builder.Services.AddSingleton<ConfigurationRepository>();
            builder.Services.AddSingleton<ConfigurationItemRepository>();
            builder.Services.AddSingleton<SpecialistRepository>();

            builder.Services.AddSingleton<MainServices>();
            builder.Services.AddSingleton<IConfigurationEngine, ConfigurationEngineAdapter>();
            builder.Services.AddSingleton<IAppService, AppService>();

            var app = builder.Build();

/*    
#if DEBUG
             // Run full profile evaluation test at startup (non-blocking).
             try
             {
                 Console.WriteLine("!!!--------------------------------------------------------------------------");
                 _ = TestRunner.RunAsync(resetDatabase: false)
                     .ContinueWith(task =>
                     {
                         Console.WriteLine("-----------------------------------------------------------------------------");
                         if (task.IsFaulted)
                         {
                             Console.WriteLine("[StartupDebug] TestRunner failed: " + task.Exception?.GetBaseException().Message);
                         }
                         else
                         {
                             Console.WriteLine("[StartupDebug] TestRunner completed successfully.");
                         }
                     });
             }
             catch (Exception ex)
             {
                 Console.WriteLine("[StartupDebug] Scheduling debug task failed: " + ex.Message);
             }
 #endif
*/
            return app;
        }
    }
}
