using WheelchairConfigurator.Data.Repositories;

namespace ConfigurationLogic;

// Service facade
public class MainServices
{
    public Catalog Catalog { get; }
    public Configurator Configurator { get; }
    public Persistence Persistence { get; }

    // Build services from repositories
    public MainServices(
        CategoryRepository categoryRepository,
        ComponentRepository componentRepository,
        ComponentSpecsRepository componentSpecsRepository,
        CompatibilityRuleRepository compatibilityRuleRepository,
        ConfigurationRepository configurationRepository,
        ConfigurationItemRepository configurationItemRepository)
    {
        Catalog = new Catalog(categoryRepository, componentRepository, componentSpecsRepository);
        Configurator = new Configurator(Catalog, compatibilityRuleRepository);
        Persistence = new Persistence(configurationRepository, configurationItemRepository);
    }

    // Inject ready service instances
    public MainServices(Catalog catalog, Configurator configurator, Persistence persistence)
    {
        Catalog = catalog;
        Configurator = configurator;
        Persistence = persistence;
    }
}
