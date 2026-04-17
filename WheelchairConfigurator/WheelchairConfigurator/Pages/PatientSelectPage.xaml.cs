namespace WheelchairConfigurator.Pages;
public class PatientMock
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}


public class WheelchairMock
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
}

public partial class PatientSelectPage : ContentPage
{
    // Testovací data
    private readonly List<PatientMock> _patients =
    [
        new PatientMock { Id = "PAT-001", CreatedAt = new DateTime(2024, 1, 15) },
        new PatientMock { Id = "PAT-002", CreatedAt = new DateTime(2024, 3, 22) },
        new PatientMock { Id = "PAT-003", CreatedAt = new DateTime(2024, 6, 8)  },
        new PatientMock { Id = "PAT-004", CreatedAt = new DateTime(2025, 1, 3)  },
    ];

    private readonly List<WheelchairMock> _wheelchairs =
    [
        new WheelchairMock { Id = "WC-001", Name="WC-001", CreatedAt = new DateTime(2024, 2, 10), PatientId = "PAT-001" },
        new WheelchairMock { Id = "WC-002", Name="WC-002", CreatedAt = new DateTime(2024, 2, 28), PatientId = "PAT-001" },
        new WheelchairMock { Id = "WC-003", Name="WC-003", CreatedAt = new DateTime(2024, 4, 5),  PatientId = "PAT-002" },
        new WheelchairMock { Id = "WC-004", Name="WC-004", CreatedAt = new DateTime(2024, 7, 19), PatientId = "PAT-003" },
    ];

    public PatientSelectPage()
    {
        InitializeComponent();
        PatientList.ItemsSource = _patients;
    }

    /*
     * OnPatientSelected - naète vozíky vybraného pacienta
     */
    private WheelchairMock? _selectedWheelchair = null;

    private void OnWheelchairSelected(object sender, SelectionChangedEventArgs e)
    {
        _selectedWheelchair = e.CurrentSelection.FirstOrDefault() as WheelchairMock;
        ContinueBtn.IsEnabled = _selectedWheelchair is not null;
    }

    private void OnPatientSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not PatientMock selected)
            return;

        _selectedWheelchair = null;
        ContinueBtn.IsEnabled = false;

        WheelchairListTitle.Text = $"Vozíky pacienta {selected.Id}";

        var items = _wheelchairs
            .Where(w => w.PatientId == selected.Id)
            .ToList();

        items.Add(new WheelchairMock { Id = "new", Name = "Vytvoøit nový vozík", PatientId = selected.Id });

        WheelchairList.ItemsSource = items;
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        if (_selectedWheelchair is null)
            return;

        if (_selectedWheelchair.Name == "new")
            await Shell.Current.GoToAsync("wheelchairConfiguratorPage"); 
        else
            await Shell.Current.GoToAsync($"wheelchairConfiguratorPage?wheelchairId={_selectedWheelchair.Id}");
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("mainPage");
    }



}