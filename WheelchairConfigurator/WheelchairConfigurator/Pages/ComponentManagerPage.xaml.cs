using WheelchairConfigurator.ServiceLayer.Interfaces;
using WheelchairConfigurator.ServiceLayer.Models;

namespace WheelchairConfigurator.Pages;

public partial class ComponentManagerPage : ContentPage
{
    private readonly IAppService _appService;
    private List<CategoryModel> _categories = new();
    private ComponentModel? _selectedComponent;
    private bool _isLandscape;

    private readonly Picker _categoryPicker;
    private readonly Entry _nameEntry;
    private readonly Editor _descriptionEditor;
    private readonly CollectionView _categoryList;
    private readonly CollectionView _componentList;
    private readonly Label _componentListTitle;
    private readonly Button _removeBtn;

    public ComponentManagerPage(IAppService appService)
    {
        _appService = appService;
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
        return new Border { Margin = new Thickness(0, 0, 0, 8), Padding = new Thickness(12), StrokeThickness = 1, Stroke = new SolidColorBrush(Color.FromArgb("#E0E0E0")), Content = name };
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
    }

    private async void OnAddComponentClicked(object? sender, EventArgs e)
    {
        var selectedCatIndex = _categoryPicker.SelectedIndex;
        if (selectedCatIndex < 0 || string.IsNullOrWhiteSpace(_nameEntry.Text))
        {
            await DisplayAlert("Chyba", "Vyberte kategorii a zadejte název.", "OK");
            return;
        }
        var category = _categories[selectedCatIndex];
        var result = await _appService.AddComponentAsync(_nameEntry.Text, category.Id);
        if (result.IsSuccess)
        {
            _nameEntry.Text = "";
            _descriptionEditor.Text = "";
            await LoadData();
        }
        else
        {
            await DisplayAlert("Chyba", result.Message, "OK");
        }
    }

    private async void OnRemoveComponentClicked(object? sender, EventArgs e)
    {
        if (_selectedComponent is null) return;
        var result = await _appService.RemoveComponentAsync(_selectedComponent.Id);
        if (result.IsSuccess)
        {
            _selectedComponent = null;
            _removeBtn.IsEnabled = false;
            if (_categoryList.SelectedItem is CategoryModel cat)
            {
                var components = await _appService.GetComponentsAsync(cat.Id);
                _componentList.ItemsSource = components;
            }
        }
        else
        {
            await DisplayAlert("Chyba", result.Message, "OK");
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("mainPage");
    }
}
