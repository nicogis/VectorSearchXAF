using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using XAFVectorSearch.Blazor.Server.Services;
using XAFVectorSearch.Module.BusinessObjects;

namespace XAFVectorSearch.Blazor.Server.Controllers; 

public  class EmbeddingController : ViewController {
    readonly SimpleAction embeddingAction;
    readonly VectorSearchService vectorSearchService;
    public EmbeddingController() {

        

        this.TargetObjectType = typeof(Documents);
        this.TargetViewType = ViewType.DetailView;

        embeddingAction = new(this, "embeddingAction", "View")
        {
            Caption = "Embedding document"
        };
        
        embeddingAction.Execute += EmbeddingAction_Execute; ;
    }

    [ActivatorUtilitiesConstructor]
    public EmbeddingController(VectorSearchService vectorSearchService) : this()
    {
        this.vectorSearchService = vectorSearchService;
    }

    private  async void EmbeddingAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var document = View.CurrentObject as Documents;

       
        await vectorSearchService.ImportAsync(document.ID);

        RefreshController refreshController = Frame.GetController<RefreshController>();
        refreshController?.RefreshAction.DoExecute();

        Application.ShowViewStrategy.ShowMessage("Document embedded!");

        
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        ObjectSpace.ModifiedChanged += ObjectSpace_ModifiedChanged;
        UpdateActionState();
    }
    void ObjectSpace_ModifiedChanged(object sender, EventArgs e)
    {
        UpdateActionState();
    }
    protected virtual void UpdateActionState()
    {
        embeddingAction.Enabled["ObjectSpaceIsModified"] = !ObjectSpace.IsModified && ((View.CurrentObject as Documents)?.DocumentChunks?.Count == 0);
    }
    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        ObjectSpace.ModifiedChanged -= ObjectSpace_ModifiedChanged;
    }

    
    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated(); 
        
    }
    
}
