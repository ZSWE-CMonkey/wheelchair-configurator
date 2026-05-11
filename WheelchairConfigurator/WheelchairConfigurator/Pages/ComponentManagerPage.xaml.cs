using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.Pages;

public partial class ComponentManagerPage : ContentPage
{
    private readonly IAppService _appService;
    private List<CategoryModel> _categories = new();
    private ComponentModel? _selectedComponent;
    private bool _isLandscape;

    private static Color ThemeColor(Color light, Color dark) =>
        Application.Current?.RequestedTheme == AppTheme.Dark ? dark : light;

    private readonly Picker _categoryPicker;
    private readonly Entry _nameEntry;
    private readonly Entry _manufacturerEntry;
    private readonly Entry _manufacturerCodeEntry;
    private readonly Entry _catalogUrlEntry;
    private readonly CollectionView _categoryList;
    private readonly CollectionView _componentList;
    private readonly Label _componentListTitle;
    private readonly Button _removeBtn;
    private readonly Button _addBtn;
    private readonly Button _cancelEditBtn;
    private int _editingComponentId = 0;

    public ComponentManagerPage(IAppService appService)
    {
        _appService = appService;
        InitializeComponent();

        _categoryPicker = new Picker { Title = "Vyberte kategorii", HorizontalOptions = LayoutOptions.Fill };
        _nameEntry = new Entry { Placeholder = "Zadejte název", HorizontalOptions = LayoutOptions.Fill };
        _manufacturerEntry = new Entry { Placeholder = "Výrobce", HorizontalOptions = LayoutOptions.Fill };
        _manufacturerCodeEntry = new Entry { Placeholder = "Kód výrobce (ManufacturerCode)", HorizontalOptions = LayoutOptions.Fill };
        _catalogUrlEntry = new Entry { Placeholder = "URL katalogu (volitelné)", HorizontalOptions = LayoutOptions.Fill, Keyboard = Keyboard.Url };

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

        _addBtn = new Button
        {
            Text = "Přidat",
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(0, 8, 0, 0)
        };
        _addBtn.Clicked += OnSaveFormClicked;

        _cancelEditBtn = new Button
        {
            Text = "Zrušit úpravy",
            HorizontalOptions = LayoutOptions.Fill,
            BackgroundColor = Colors.Gray,
            TextColor = Colors.White,
            IsVisible = false
        };
        _cancelEditBtn.Clicked += OnCancelEditClicked;

        Dispatcher.Dispatch(async () => await LoadData());
    }

    private async Task LoadData()
    {
        _categories = await _appService.GetCategoriesAsync();
        _categoryList.ItemsSource = _categories;

        _categoryPicker.Items.Clear();
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
        View[] shared = [_categoryPicker, _nameEntry, _manufacturerEntry, _manufacturerCodeEntry, _catalogUrlEntry, _categoryList, _componentListTitle, _componentList, _removeBtn, _addBtn, _cancelEditBtn];

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
        var titleLabel = new Label { Text = "Přidat komponentu", FontAttributes = FontAttributes.Bold, FontSize = 20, Margin = new Thickness(0, 0, 0, 8) };

        var content = new VerticalStackLayout { Spacing = 12 };
        content.Children.Add(titleLabel);
        content.Children.Add(new Label { Text = "Kategorie", FontSize = 13 });
        content.Children.Add(_categoryPicker);
        content.Children.Add(new Label { Text = "Název", FontSize = 13 });
        content.Children.Add(_nameEntry);
        content.Children.Add(new Label { Text = "Výrobce", FontSize = 13 });
        content.Children.Add(_manufacturerEntry);
        content.Children.Add(new Label { Text = "Kód výrobce", FontSize = 13 });
        content.Children.Add(_manufacturerCodeEntry);
        content.Children.Add(new Label { Text = "URL katalogu", FontSize = 13 });
        content.Children.Add(_catalogUrlEntry);
        content.Children.Add(_addBtn);
        content.Children.Add(_cancelEditBtn);

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
        Stroke = new SolidColorBrush(ThemeColor(Color.FromArgb("#E0E0E0"), Color.FromArgb("#3D3D3D"))),
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
        var idLabel = new Label { FontSize = 11, TextColor = Colors.Gray };
        idLabel.SetBinding(Label.TextProperty, new Binding("Id", stringFormat: "ID: {0}"));

        var name = new Label { FontAttributes = FontAttributes.Bold, FontSize = 14 };
        name.SetBinding(Label.TextProperty, "Name");

        var manufacturer = new Label { FontSize = 12 };
        manufacturer.SetBinding(Label.TextProperty, new Binding("Manufacturer", stringFormat: "Výrobce: {0}"));

        var mfrCode = new Label { FontSize = 12, TextColor = Colors.Gray };
        mfrCode.SetBinding(Label.TextProperty, new Binding("ManufacturerCode", stringFormat: "Kód: {0}"));

        var stack = new VerticalStackLayout { Spacing = 2 };
        stack.Children.Add(idLabel);
        stack.Children.Add(name);
        stack.Children.Add(manufacturer);
        stack.Children.Add(mfrCode);

        return new Border
        {
            Margin = new Thickness(0, 0, 0, 8),
            Padding = new Thickness(12),
            StrokeThickness = 1,
            Stroke = new SolidColorBrush(ThemeColor(Color.FromArgb("#E0E0E0"), Color.FromArgb("#3D3D3D"))),
            Content = stack
        };
    });

    private void OnCategorySelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not CategoryModel selected) return;
        _selectedComponent = null;
        _removeBtn.IsEnabled = false;
        _componentListTitle.Text = selected.Name;
        Dispatcher.Dispatch(async () =>
        {
            var components = await _appService.GetComponentsAsync(selected.Id);
            _componentList.ItemsSource = components;
        });
    }

    private void OnComponentSelected(object? sender, SelectionChangedEventArgs e)
    {
        _selectedComponent = e.CurrentSelection.FirstOrDefault() as ComponentModel;
        _removeBtn.IsEnabled = _selectedComponent is not null;

        if (_selectedComponent is not null)
        {
            _editingComponentId = _selectedComponent.Id;
            _nameEntry.Text = _selectedComponent.Name;
            _manufacturerEntry.Text = _selectedComponent.Manufacturer;
            _manufacturerCodeEntry.Text = _selectedComponent.ManufacturerCode;
            _catalogUrlEntry.Text = _selectedComponent.CatalogUrl ?? "";
            _addBtn.Text = "Uložit změny";
            _cancelEditBtn.IsVisible = true;
        }
    }

    private async void OnSaveFormClicked(object? sender, EventArgs e)
    {
        var selectedCatIndex = _categoryPicker.SelectedIndex;
        bool isEditing = _editingComponentId > 0;

        if (string.IsNullOrWhiteSpace(_nameEntry.Text) || (!isEditing && selectedCatIndex < 0))
        {
            await DisplayAlert("Chyba", "Vyberte kategorii a zadejte název.", "OK");
            return;
        }

        _addBtn.IsEnabled = false;
        var prevText = _addBtn.Text;
        _addBtn.Text = "Ukládám...";

        ConfigurationResult result;

        if (isEditing)
        {
            var model = new ComponentModel
            {
                Id = _editingComponentId,
                Name = _nameEntry.Text.Trim(),
                Manufacturer = _manufacturerEntry.Text?.Trim() ?? "",
                ManufacturerCode = _manufacturerCodeEntry.Text?.Trim() ?? "",
                CatalogUrl = string.IsNullOrWhiteSpace(_catalogUrlEntry.Text) ? null : _catalogUrlEntry.Text.Trim(),
                Price = _selectedComponent?.Price ?? 0,
            };
            result = await _appService.UpdateComponentAsync(model);
        }
        else
        {
            var category = _categories[selectedCatIndex];
            result = await _appService.AddComponentAsync(
                _nameEntry.Text.Trim(),
                category.Id,
                _manufacturerEntry.Text?.Trim() ?? "",
                _manufacturerCodeEntry.Text?.Trim() ?? "",
                _catalogUrlEntry.Text?.Trim() ?? "");
        }

        _addBtn.IsEnabled = true;
        _addBtn.Text = prevText;

        if (result.IsSuccess)
        {
            ResetForm();
            await LoadData();
        }
        else
        {
            await DisplayAlert("Chyba", result.Message, "OK");
        }
    }

    private void OnCancelEditClicked(object? sender, EventArgs e) => ResetForm();

    private void ResetForm()
    {
        _editingComponentId = 0;
        _selectedComponent = null;
        _nameEntry.Text = "";
        _manufacturerEntry.Text = "";
        _manufacturerCodeEntry.Text = "";
        _catalogUrlEntry.Text = "";
        _addBtn.Text = "Přidat";
        _cancelEditBtn.IsVisible = false;
        _removeBtn.IsEnabled = false;
        _componentList.SelectedItem = null;
    }

    private async void OnRemoveComponentClicked(object? sender, EventArgs e)
    {
        if (_selectedComponent is null) return;

        _removeBtn.IsEnabled = false;
        _removeBtn.Text = "Odstraňuji...";

        var result = await _appService.RemoveComponentAsync(_selectedComponent.Id);

        _removeBtn.Text = "Odstranit";

        if (result.IsSuccess)
        {
            ResetForm();
            if (_categoryList.SelectedItem is CategoryModel cat)
            {
                var components = await _appService.GetComponentsAsync(cat.Id);
                _componentList.ItemsSource = components;
            }
        }
        else
        {
            _removeBtn.IsEnabled = true;
            await DisplayAlert("Chyba", result.Message, "OK");
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
