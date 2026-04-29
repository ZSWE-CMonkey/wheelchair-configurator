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
        new() { Id = "CAT-001", Name = "Rám"     },
        new() { Id = "CAT-002", Name = "Motor"   },
        new() { Id = "CAT-003", Name = "Baterie" },
        new() { Id = "CAT-004", Name = "Pohon"   },
        new() { Id = "CAT-005", Name = "Sedák"   },
        new() { Id = "CAT-006", Name = "Opěrka"  },
    ];

    private readonly List<ComponentItemMock> _components =
    [
        new() { Id = "RAM-001", Name = "Rám Standard",      Description = "Základní ocelový rám",        CategoryId = "CAT-001" },
        new() { Id = "RAM-002", Name = "Rám Sport",          Description = "Lehký hliníkový rám",         CategoryId = "CAT-001" },
        new() { Id = "MOT-001", Name = "Motor 250W",         Description = "Úsporný motor do interiéru",  CategoryId = "CAT-002" },
        new() { Id = "MOT-002", Name = "Motor 500W",         Description = "Výkonný motor do terénu",     CategoryId = "CAT-002" },
        new() { Id = "BAT-001", Name = "Baterie 10Ah",       Description = "Kompaktní baterie",           CategoryId = "CAT-003" },
        new() { Id = "BAT-002", Name = "Baterie 20Ah",       Description = "Standardní baterie",          CategoryId = "CAT-003" },
        new() { Id = "POH-001", Name = "Pohon Přímý",        Description = "Jednoduchý přímý pohon",      CategoryId = "CAT-004" },
        new() { Id = "SED-001", Name = "Sedák Základní",     Description = "Standardní sedák",            CategoryId = "CAT-005" },
        new() { Id = "SED-002", Name = "Sedák Ortopedický",  Description = "Tvarovaný ortopedický sedák", CategoryId = "CAT-005" },
        new() { Id = "OPE-001", Name = "Opěrka Pevná",       Description = "Pevná opěrka zad",            CategoryId = "CAT-006" },
    ];

    private ComponentItemMock? _selectedComponent;
    private bool _isLandscape;

    private readonly Picker _categoryPicker;
    private readonly Entry _nameEntry;
    private readonly Editor _descriptionEditor;
    private readonly CollectionView _categoryList;
    private readonly CollectionView _componentList;
    private readonly Label _componentListTitle;
    private readonly Button _removeBtn;

    public ComponentManagerPage()
    {
        InitializeComponent();
        _categoryPicker = new Picker { Title = "Vyberte kategorii", HorizontalOptions = LayoutOptions.Fill };
        _nameEntry = new Entry { Placeholder = "Zadejte název", HorizontalOptions = LayoutOptions.Fill };
        _descriptionEditor = new Editor { Placeholder = "Zadejte popis", HeightRequest = 60, HorizontalOptions = LayoutOptions.Fill };

        _categoryList = new CollectionView { SelectionMode = SelectionMode.Single };
        _categoryList.SelectionChanged += OnCategorySelected;
        _categoryList.ItemTemplate = CategoryItemTemplate();

        _componentListTitle = new Label { Text = "Vyberte kategorii", FontSize = 13, FontAttributes = FontAttributes.Bold };

        _componentList = new CollectionView
        {
            SelectionMode = SelectionMode.Single,
            EmptyView = new Label
            {
                Text = "Žádné komponenty",
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
        _componentList.SelectionChanged += OnComponentSelected;
        _componentList.ItemTemplate = ComponentItemTemplate();

        _removeBtn = new Button
        {
            Text = "Odstranit",
            IsEnabled = false,
            BackgroundColor = Colors.Red,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Fill
        };
        _removeBtn.Clicked += OnRemoveComponentClicked;

        _categoryList.ItemsSource = _categories;
        foreach (var cat in _categories)
            _categoryPicker.Items.Add(cat.Name);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0 || height <= 0) return;

        bool landscape = width > height;
        if (landscape == _isLandscape && MainContent.Content != null) return;

        _isLandscape = landscape;

        DetachSharedViews();

        MainContent.Content = landscape ? BuildLandscapeLayout() : BuildPortraitLayout();
    }

    private void DetachSharedViews()
    {
        View[] shared = [_categoryPicker, _nameEntry, _descriptionEditor, _categoryList, _componentListTitle, _componentList, _removeBtn];

        foreach (var view in shared)
        {
            if (view.Parent is Layout layout)
                layout.Remove(view);
            else if (view.Parent is Grid grid)
                grid.Children.Remove(view);
            else if (view.Parent is ContentView cv)
                cv.Content = null;
        }
    }


    private View BuildLandscapeLayout()
    {
        var grid = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(new GridLength(2, GridUnitType.Star)) },
            ColumnSpacing = 20
        };

        Grid.SetColumn(AddSection(), 0);
        var add = AddSection();
        var remove = RemoveSection(collectionHeight: 0); 

        Grid.SetColumn(add, 0);
        Grid.SetColumn(remove, 1);
        grid.Children.Add(add);
        grid.Children.Add(remove);

        return grid;
    }


    private View BuildPortraitLayout()
    {
        var stack = new VerticalStackLayout { Spacing = 20 };
        stack.Children.Add(AddSection());
        stack.Children.Add(RemoveSection(collectionHeight: 180));
        return new ScrollView { Content = stack };
    }

    private View AddSection()
    {
        var addBtn = new Button { Text = "Přidat", HorizontalOptions = LayoutOptions.Fill, Margin = new Thickness(0, 8, 0, 0) };
        addBtn.Clicked += OnAddComponentClicked;

        var content = new VerticalStackLayout { Spacing = 12 };
        content.Children.Add(new Label { Text = "Přidat komponentu", FontAttributes = FontAttributes.Bold, FontSize = 20, Margin = new Thickness(0, 0, 0, 8) });
        content.Children.Add(new Label { Text = "Kategorie", FontSize = 13 });
        content.Children.Add(_categoryPicker);
        content.Children.Add(new Label { Text = "Název", FontSize = 13 });
        content.Children.Add(_nameEntry);
        content.Children.Add(new Label { Text = "Popis", FontSize = 13 });
        content.Children.Add(_descriptionEditor);
        content.Children.Add(addBtn);

        return Bordered(content);
    }

    private View RemoveSection(double collectionHeight)
    {
        if (collectionHeight > 0)
        {
            _categoryList.HeightRequest = collectionHeight;
            _componentList.HeightRequest = collectionHeight;
        }
        else
        {
            _categoryList.HeightRequest = -1;
            _componentList.HeightRequest = -1;
        }

        var inner = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
            ColumnSpacing = 15
        };

        var leftGrid = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) } };
        leftGrid.Children.Add(new Label { Text = "Kategorie", FontSize = 13, FontAttributes = FontAttributes.Bold });
        Grid.SetRow(_categoryList, 1);
        leftGrid.Children.Add(_categoryList);

        var rightGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };
        rightGrid.Children.Add(_componentListTitle);
        Grid.SetRow(_componentList, 1);
        rightGrid.Children.Add(_componentList);
        Grid.SetRow(_removeBtn, 2);
        rightGrid.Children.Add(_removeBtn);

        Grid.SetColumn(rightGrid, 1);
        inner.Children.Add(leftGrid);
        inner.Children.Add(rightGrid);

        var wrapper = new VerticalStackLayout { Spacing = 12 };
        wrapper.Children.Add(new Label { Text = "Odstranit komponentu", FontSize = 20, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 0, 0, 10) });
        wrapper.Children.Add(inner);

        return Bordered(wrapper);
    }

    private static Border Bordered(View content) => new()
    {
        Padding = new Thickness(20),
        StrokeThickness = 1,
        Stroke = new SolidColorBrush(Color.FromArgb("#E0E0E0")),
        Content = content
    };

    private static DataTemplate CategoryItemTemplate() => new(() =>
    {
        var label = new Label { FontAttributes = FontAttributes.Bold, FontSize = 14 };
        label.SetBinding(Label.TextProperty, "Name");
        return new Border { Margin = new Thickness(0, 0, 0, 8), Padding = new Thickness(12), StrokeThickness = 1, Stroke = new SolidColorBrush(Color.FromArgb("#E0E0E0")), Content = label };
    });

    private static DataTemplate ComponentItemTemplate() => new(() =>
    {
        var name = new Label { FontAttributes = FontAttributes.Bold, FontSize = 14 };
        name.SetBinding(Label.TextProperty, "Name");
        var desc = new Label { FontSize = 12, TextColor = Colors.Gray };
        desc.SetBinding(Label.TextProperty, "Description");
        var stack = new VerticalStackLayout();
        stack.Children.Add(name);
        stack.Children.Add(desc);
        return new Border { Margin = new Thickness(0, 0, 0, 8), Padding = new Thickness(12), StrokeThickness = 1, Stroke = new SolidColorBrush(Color.FromArgb("#E0E0E0")), Content = stack };
    });

    private void OnCategorySelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not CategoryMock selected) return;
        _selectedComponent = null;
        _removeBtn.IsEnabled = false;
        _componentListTitle.Text = selected.Name;
        _componentList.ItemsSource = _components.Where(c => c.CategoryId == selected.Id).ToList();
    }

    private void OnComponentSelected(object? sender, SelectionChangedEventArgs e)
    {
        _selectedComponent = e.CurrentSelection.FirstOrDefault() as ComponentItemMock;
        _removeBtn.IsEnabled = _selectedComponent is not null;
    }

    private void OnAddComponentClicked(object? sender, EventArgs e)
    {
        // TODO: Saving function
    }

    private void OnRemoveComponentClicked(object? sender, EventArgs e)
    {
        // TODO: remove function
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("mainPage");
    }
}