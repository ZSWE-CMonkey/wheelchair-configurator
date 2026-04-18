namespace WheelchairConfigurator.Pages;

public class CategoryMock
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class ComponentItemMock
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
}

public partial class ComponentManagerPage : ContentPage
{
    private readonly List<CategoryMock> _categories =
    [
        new CategoryMock { Id = "CAT-001", Name = "Rám"     },
        new CategoryMock { Id = "CAT-002", Name = "Motor"   },
        new CategoryMock { Id = "CAT-003", Name = "Baterie" },
        new CategoryMock { Id = "CAT-004", Name = "Pohon"   },
        new CategoryMock { Id = "CAT-005", Name = "Sedák"   },
        new CategoryMock { Id = "CAT-006", Name = "Opìrka"  },
    ];

    private readonly List<ComponentItemMock> _components =
    [
        new ComponentItemMock { Id = "RAM-001", Name = "Rám Standard",        Description = "Základní ocelový rám",         CategoryId = "CAT-001" },
        new ComponentItemMock { Id = "RAM-002", Name = "Rám Sport",           Description = "Lehký hliníkový rám",          CategoryId = "CAT-001" },
        new ComponentItemMock { Id = "MOT-001", Name = "Motor 250W",          Description = "Úsporný motor do interiéru",   CategoryId = "CAT-002" },
        new ComponentItemMock { Id = "MOT-002", Name = "Motor 500W",          Description = "Výkonný motor do terénu",      CategoryId = "CAT-002" },
        new ComponentItemMock { Id = "BAT-001", Name = "Baterie 10Ah",        Description = "Kompaktní baterie",            CategoryId = "CAT-003" },
        new ComponentItemMock { Id = "BAT-002", Name = "Baterie 20Ah",        Description = "Standardní baterie",           CategoryId = "CAT-003" },
        new ComponentItemMock { Id = "POH-001", Name = "Pohon Pøímý",         Description = "Jednoduchý pøímý pohon",       CategoryId = "CAT-004" },
        new ComponentItemMock { Id = "SED-001", Name = "Sedák Základní",      Description = "Standardní sedák",             CategoryId = "CAT-005" },
        new ComponentItemMock { Id = "SED-002", Name = "Sedák Ortopedický",   Description = "Tvarovaný ortopedický sedák",  CategoryId = "CAT-005" },
        new ComponentItemMock { Id = "OPE-001", Name = "Opìrka Pevná",        Description = "Pevná opìrka zad",             CategoryId = "CAT-006" },
    ];

    private ComponentItemMock? _selectedComponent = null;

    public ComponentManagerPage()
    {
        InitializeComponent();

        CategoryList.ItemsSource = _categories;

        foreach (var category in _categories)
            CategoryPicker.Items.Add(category.Name);
    }

    /*
     * OnCategorySelected - loads all componets in category
     */
    private void OnCategorySelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not CategoryMock selected)
            return;

        _selectedComponent = null;
        RemoveBtn.IsEnabled = false;
        ComponentListTitle.Text = selected.Name;
        ComponentList.ItemsSource = _components
            .Where(c => c.CategoryId == selected.Id)
            .ToList();
    }

    /*
     * OnComponentSelected - actives remove button
     */
    private void OnComponentSelected(object sender, SelectionChangedEventArgs e)
    {
        _selectedComponent = e.CurrentSelection.FirstOrDefault() as ComponentItemMock;
        RemoveBtn.IsEnabled = _selectedComponent is not null;
    }

    /*
     * OnAddComponentClicked - adds new component
     */
    private void OnAddComponentClicked(object sender, EventArgs e)
    {
        // TODO: Saving function
    }

    /*
     * OnRemoveComponentClicked - removes selected component
     */
    private void OnRemoveComponentClicked(object sender, EventArgs e)
    {
        // TODO: remove function (not total remove from db because it could still be used in older wheelchair, it's meaned like unavailable
    }

    /*
     * OnBackClicked - redirects to main page
     */
    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("mainPage");
    }
}