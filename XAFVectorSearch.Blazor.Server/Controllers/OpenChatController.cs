using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using XAFVectorSearch.Blazor.Server.Controls.RagChat;
using XAFVectorSearch.Module.BusinessObjects;

namespace XAFVectorSearch.Blazor.Server.Controllers;

public class OpenChatController : ViewController
{
    
    readonly PopupWindowShowAction Chat;
    const string DefaultPrompt = "You are an analytics assistant specialized in analyzing files. Your role is to assist users by providing accurate answers to their questions about data contained within these files.\n \n### Tasks:\n- Perform various types of data analyses, including summaries, calculations, data filtering, and trend identification.\n- Clearly explain your analysis process to ensure users understand how you arrived at your answers.\n- Always provide precise and accurate information based on the Excel data.\n- If you cannot find an answer based on the provided data, explicitly state: \"The requested information cannot be found in the data provided.\"\n \n### Examples:\n1. **Summarization:**\n   - **User Question:** \"What is the average sales revenue for Q1?\"\n   - **Response:** \"The average sales revenue for Q1 is calculated as $45,000, based on the data in Sheet1, Column C.\"\n \n2. **Data Filtering:**\n   - **User Question:** \"Which products had sales over $10,000 in June?\"\n   - **Response:** \"The products with sales over $10,000 in June are listed in Sheet2, Column D, and they include Product A, Product B, and Product C.\"\n \n3. **Insufficient Data:**\n   - **User Question:** \"What is the market trend for Product Z over the past 5 years?\"\n   - **Response:** \"The requested information cannot be found in the data provided, as the dataset only includes data for the current year.\"\n \n### Additional Instructions:\n- Format your responses to clearly indicate which sheet and column the data was extracted from when necessary.\n- Avoid providing any answers if the data in the file is insufficient for a reliable response.\n- Ask clarifying questions if the user's query is ambiguous or lacks detail.\n \nRemember, your primary goal is to provide helpful, data-driven insights that directly answer the user's questions. Do not assume or infer information not present in the dataset.Always return response well formatted using markdown.";
    public OpenChatController() : base()
    {
        
        this.TargetObjectType = typeof(Documents);
        this.TargetViewType= ViewType.ListView;

        Chat = new(this, "ChatAction", "View")
        {
            Caption = "Chat"
        };
        
        //Chat.Execute += Chat_Execute;
        Chat.CustomizePopupWindowParams += Chat_CustomizePopupWindowParams;


    }
    
    private void Chat_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        
        IObjectSpace os = Application.CreateObjectSpace<ChatView>();
        var ChatView = os.CreateObject<ChatView>();

        // need info in view chat ?
        ChatView.RagData = os.CreateObject<IRagDataImp>();
        ChatView.RagData.Prompt = DefaultPrompt;
        


        DetailView detailView = this.Application.CreateDetailView(os, ChatView);

        detailView.Caption = $"Chat";


        e.DialogController.AcceptAction.Active["NothingToAccept"] = false;
        e.DialogController.CancelAction.Active["NothingToCancel"] = false;
        e.View = detailView;
    }
    protected override void OnActivated()
    {
        base.OnActivated();
        
    }
    protected override void OnDeactivated()
    {
        
        base.OnDeactivated();
    }
    protected override void OnViewControlsCreated()
    {
        base.OnViewControlsCreated();
        
    }
}
