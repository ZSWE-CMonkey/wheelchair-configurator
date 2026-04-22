using Microsoft.Extensions.Logging;
using ConfigurationLogic;
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

            return builder.Build();
        }
    }
}
