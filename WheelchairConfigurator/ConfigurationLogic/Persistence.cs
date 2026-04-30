using WheelchairConfigurator.Data.Repositories;
using WheelchairConfigurator.Domain.Models;

namespace ConfigurationLogic;

// Configuration persistence logic
public class Persistence
{
    private readonly ConfigurationRepository _configurationRepository;
    private readonly ConfigurationItemRepository _configurationItemRepository;

    // Initialize persistence repositories
    public Persistence(
        ConfigurationRepository configurationRepository,
        ConfigurationItemRepository configurationItemRepository)
    {
        _configurationRepository = configurationRepository;
        _configurationItemRepository = configurationItemRepository;
    }

    // Save configuration and items
    public async Task<int> SaveConfigurationAsync(int specialistId, IEnumerable<int> componentIds)
    {
        var configuration = new Configuration
        {
            SpecialistId = specialistId,
            CreatedAt = DateTime.Now
        };

        await _configurationRepository.InsertAsync(configuration);

        foreach (var group in componentIds.GroupBy(id => id))
        {
            await _configurationItemRepository.InsertAsync(new ConfigurationItem
            {
                ConfigurationId = configuration.Id,
                ComponentId = group.Key,
                Quantity = group.Count()
            });
        }

        return configuration.Id;
    }

    // Load specialist configurations
    public Task<List<Configuration>> GetConfigurationsBySpecialistAsync(int specialistId)
    {
        return _configurationRepository.GetBySpecialistIdAsync(specialistId);
    }

    // Load configuration items
    public Task<List<ConfigurationItem>> GetConfigurationItemsAsync(int configurationId)
    {
        return _configurationItemRepository.GetByConfigurationIdAsync(configurationId);
    }
}
