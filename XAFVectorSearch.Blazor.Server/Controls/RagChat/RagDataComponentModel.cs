using DevExpress.ExpressApp.Blazor.Components.Models;
using Microsoft.AspNetCore.Components;

namespace XAFVectorSearch.Blazor.Server.Controls.RagChat;

public class RagDataComponentModel : ComponentModelBase
{
    public IRagData Value
    {
        get => GetPropertyValue<IRagData>();
        set => SetPropertyValue(value);
    }

    public EventCallback<IRagData> ValueChanged
    {
        get => GetPropertyValue<EventCallback<IRagData>>();
        set => SetPropertyValue(value);
    }


    public override Type ComponentType => typeof(RagChat);
}
