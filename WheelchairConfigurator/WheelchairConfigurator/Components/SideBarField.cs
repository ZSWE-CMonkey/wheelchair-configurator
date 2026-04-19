namespace WheelchairConfigurator.Components;

public enum FieldType
{
    Entry,
    Picker
}

public class SideBarField
{
    public string Label { get; set; } = String.Empty;
    public FieldType Type { get; set; }
    public List<string> Options { get; set; } = [];
    public Action<string> OnSave { get; set; } = _ => { };
    public Keyboard Keyboard { get; set; } = Keyboard.Numeric;
    public int MaxLength { get; set; } = 3;
}
