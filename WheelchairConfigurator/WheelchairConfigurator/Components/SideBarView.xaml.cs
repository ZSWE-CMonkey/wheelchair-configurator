namespace WheelchairConfigurator.Components;

public partial class SideBarView : ContentView, ISideBar
{
    private readonly List<SideBarField> _fields;
    private readonly List<View> _inputs = [];

    public SideBarView(string title, List<SideBarField> fields)
    {
        _fields = fields;
        BuildUI(title);
    }

    /*
     * BuildUI - dynamically creates labels and inputs based on fields
     */
    private void BuildUI(string title)
    {
        var layout = new VerticalStackLayout { Padding = new Thickness(20), Spacing = 10 };

        layout.Add(new Label
        {
            Text = title,
            FontAttributes = FontAttributes.Bold,
            FontSize = 18
        });

        foreach (var field in _fields)
        {
            layout.Add(new Label { Text = field.Label });

            if (field.Type == FieldType.Entry)
            {
                var entry = new Entry
                {
                    Placeholder = "Zadejte",
                    PlaceholderColor = Colors.Gray,
                    Keyboard = Keyboard.Numeric,
                    BackgroundColor = Color.FromArgb("F5F5F5"),
                    MaxLength = 3
                };
                entry.TextChanged += (_, _) => field.OnSave(entry.Text ?? String.Empty);
                _inputs.Add(entry);
                layout.Add(entry);
            }
            else
            {
                var picker = new Picker { Title = "Vyberte" };
                foreach (var option in field.Options)
                    picker.Items.Add(option);

                picker.SelectedIndexChanged += (_, _) =>
                    field.OnSave(picker.SelectedItem as string ?? String.Empty);

                _inputs.Add(picker);
                layout.Add(picker);
            }
        }

        Content = new Border
        {
            BackgroundColor = Color.FromArgb("F8F8F8"),
            StrokeThickness = 0,
            WidthRequest = 250,
            Content = layout
        };
    }

    /*
     * Save - saves all fields
     */
    public void Save()
    {
        for (int i = 0; i < _fields.Count; i++)
        {
            if (_inputs[i] is Entry entry)
                _fields[i].OnSave(entry.Text ?? "");
            else if (_inputs[i] is Picker picker)
                _fields[i].OnSave(picker.SelectedItem as string ?? String.Empty);
        }
    }
}
