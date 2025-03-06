using DevExpress.Blazor.Reporting.Models;
using DevExpress.ExpressApp.Blazor.Components.Models;
using DevExpress.ExpressApp.Blazor.Editors;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using Microsoft.AspNetCore.Components;
using XAFVectorSearch.Blazor.Server.ComponentModels;

namespace XAFVectorSearch.Blazor.Server.Editors;

[PropertyEditor(typeof(byte[]), "PdfViewerEditor")]
public class PdfViewerPropertyEditor(Type objectType, IModelMemberViewItem model) : BlazorPropertyEditorBase(objectType, model)
{
    public override PdfViewerModel ComponentModel => (PdfViewerModel)base.ComponentModel;
    protected override IComponentModel CreateComponentModel() => new PdfViewerModel
    {
        CssClass = "pe-pdf-viewer",
        CustomizeToolbar = EventCallback.Factory.Create<ToolbarModel>(this, (m) => { m.AllItems.Clear(); })
    };
    protected override void ReadValueCore()
    {
        base.ReadValueCore();
        ComponentModel.DocumentContent = (byte[])PropertyValue;
    }
}


