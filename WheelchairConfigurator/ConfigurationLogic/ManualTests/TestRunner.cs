using ConfigurationLogic.DTOs;
using ConfigurationLogic.Enums;

namespace ConfigurationLogic.ManualTests;

// Simple manual test runner for the configuration engine.
// Use to seed DB, instantiate MainServices and call evaluation methods.
public static class TestRunner
{
    // Run quick checks. Call from debugger or a temporary console entrypoint.
    public static async Task RunAsync(bool resetDatabase = true)
    {
        Console.WriteLine("[TestRunner] Starting manual tests...");

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "wheelchair-configurator-test.db3");
        Console.WriteLine($"[TestRunner] DB path: {dbPath}");

        var dbService = new WheelchairConfigurator.Data.DbService(dbPath);

        // Seed data loader and initializer
        var dataService = new WheelchairConfigurator.Service.DataService(
            new WheelchairConfigurator.Data.Providers.LocalFileProvider(),
            new WheelchairConfigurator.Data.JsonDataLoader());

        var initializer = new WheelchairConfigurator.Data.DbInitializer(dbService, dataService);
        initializer.Initialize(resetDatabase);

        var asyncConn = dbService.GetAsyncConnection();

        // Create repositories
        var categoryRepo = new WheelchairConfigurator.Data.Repositories.CategoryRepository(asyncConn);
        var componentRepo = new WheelchairConfigurator.Data.Repositories.ComponentRepository(asyncConn);
        var specsRepo = new WheelchairConfigurator.Data.Repositories.ComponentSpecsRepository(asyncConn);
        var compatRepo = new WheelchairConfigurator.Data.Repositories.CompatibilityRuleRepository(asyncConn);
        var configurationRepo = new WheelchairConfigurator.Data.Repositories.ConfigurationRepository(asyncConn);
        var configurationItemRepo = new WheelchairConfigurator.Data.Repositories.ConfigurationItemRepository(asyncConn);

        var services = new MainServices(categoryRepo, componentRepo, specsRepo, compatRepo, configurationRepo, configurationItemRepo);

        // Sample profile for testing
        var profile = new UserProfileDto
        {
            WeightKg = 85,
            PelvisWidthCm = 49,
            ThighLengthCm = 48,
            TrunkHeightCm = 55,
            HeadControl = HeadControlLevel.Yes,
            TrunkStability = TrunkStabilityLevel.Medium,
            PressureInjuryRisk = PressureInjuryRiskLevel.Medium,
            HandFunction = HandFunctionLevel.Full,
            LowerLimbCondition = LowerLimbConditionLevel.None,
            Pain = SymptomSeverityLevel.None,
            Fatigue = SymptomSeverityLevel.None,
            Environment = UsageEnvironment.Indoor
        };

        Console.WriteLine("[TestRunner] Evaluating profile...");
        var eval = await services.EvaluateProfileAsync(profile);
        Console.WriteLine($"[TestRunner] Requirements ready. Eligible components: {eval.EligibleComponents.Count}");

        foreach (var comp in eval.EligibleComponents.Take(30))
        {
            Console.WriteLine($" - {comp.Id}: {comp.Name} ({comp.CategoryName})");
        }

        Console.WriteLine($"[TestRunner] Issues: {eval.Issues.Count}");
        foreach (var issue in eval.Issues.Take(30))
        {
            Console.WriteLine($" - [{issue.Severity}] {issue.ComponentName}: {issue.Message}");
        }

        Console.WriteLine("[TestRunner] Initializing configuration state (no selection)...");
        var state = await services.InitializeConfigurationAsync(profile);
        PrintState("Initial state", state);

        var selectionPlan = new[]
        {
            "Battery, 73Ah, 12V, group 24",
            "JOYSTICK, CJSM2, R-net",
            "Headrest, ergo, black KN T, w/short backrest mount",
            "Battery, 40Ah, 12V, group 40"
        };

        var selectedIds = new List<int>();
        foreach (var componentName in selectionPlan)
        {
            var component = state.Components.FirstOrDefault(c =>
                string.Equals(c.Component.Name, componentName, StringComparison.OrdinalIgnoreCase));

            if (component is null)
            {
                Console.WriteLine($"[TestRunner] Skipping missing component: {componentName}");
                continue;
            }

            Console.WriteLine($"[TestRunner] Simulating click: {componentName}");
            state = await services.ToggleComponentSelectionAsync(profile, selectedIds, component.Component.Id);
            selectedIds = state.SelectedComponentIds.ToList();
            PrintState($"After click: {componentName}", state);
        }

        Console.WriteLine("[TestRunner] Manual tests completed.");
    }

    private static void PrintState(string title, ConfigurationStateResponseDto state)
    {
        Console.WriteLine($"[TestRunner] {title}");
        Console.WriteLine($"[TestRunner] Components in state: {state.Components.Count}");
        Console.WriteLine("[TestRunner] Selected: " + string.Join(", ", state.SelectedComponentIds.Select(id => id.ToString())));
        Console.WriteLine("[TestRunner] Eligible IDs: " + string.Join(", ", state.EligibleComponentIds.Take(20).Select(id => id.ToString())));

        foreach (var s in state.Components)
        {
            var reasons = s.DisableReasons.Count > 0
                ? string.Join("; ", s.DisableReasons.Select(x => x ?? string.Empty))
                : string.Empty;

            Console.WriteLine(" - " + s.Component.Id + ": " + s.Component.Name +
                              " | Enabled=" + s.IsEnabled +
                              " | Selected=" + s.IsSelected +
                              " | Reasons=" + reasons);
        }
    }
}
