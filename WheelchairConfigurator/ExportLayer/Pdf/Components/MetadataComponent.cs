using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using WheelchairConfigurator.Export.ExportModel;

namespace WheelchairConfigurator.Export.Pdf.Components;

public class MetadataComponent : IComponent
{
    private readonly ConfigurationExportModel _model;

    public MetadataComponent(ConfigurationExportModel model)
    {
        _model = model;
    }

    public void Compose(IContainer container)
    {
        container.PaddingVertical(15).Column(column =>
        {
            column.Spacing(5);

            column.Item().Text(text =>
            {
                text.Span("Konfigurace: ").SemiBold();
                text.Span(_model.ConfigurationName);
            });

            column.Item().Text(text =>
            {
                text.Span("Specialista: ").SemiBold();
                text.Span(_model.SpecialistName);
            });

            column.Item().Text(text =>
            {
                text.Span("Vytvořeno: ").SemiBold();
                text.Span(_model.CreatedAt.ToString("g"));
            });
        });
    }
}