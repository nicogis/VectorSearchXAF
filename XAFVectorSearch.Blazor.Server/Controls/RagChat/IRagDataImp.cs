﻿using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XAFVectorSearch.Blazor.Server.Controls.RagChat;

[DomainComponent]
public class IRagDataImp :IRagData, IXafEntityObject, INotifyPropertyChanged
{
    //private IObjectSpace objectSpace;
    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public IRagDataImp()
    {
        Oid = Guid.NewGuid();
    }

    [DevExpress.ExpressApp.Data.Key]
    [Browsable(false)]  // Hide the entity identifier from UI.
    public Guid Oid { get; set; }



    //string fileName;
    //Stream fileContent;

    //public Stream FileContent
    //{
    //    get => fileContent;
    //    set
    //    {
    //        if (fileContent == value)
    //            return;
    //        fileContent = value;
    //        OnPropertyChanged();
    //    }
    //}

    //public string FileName
    //{
    //    get => fileName;
    //    set
    //    {
    //        if (fileName == value)
    //            return;
    //        fileName = value;
    //        OnPropertyChanged();
    //    }
    //}

    string prompt;
    public string Prompt
    {
        get => prompt;
        set
        {
            if (prompt == value)
                return;
            prompt = value;
            OnPropertyChanged();
        }
    }
    


    //[Action(Caption = "My UI Action", ConfirmationMessage = "Are you sure?", ImageName = "Attention", AutoCommit = true)]
    //public void ActionMethod() {
    //    // Trigger custom business logic for the current record in the UI (https://docs.devexpress.com/eXpressAppFramework/112619/ui-construction/controllers-and-actions/actions/how-to-create-an-action-using-the-action-attribute).
    //    this.SampleProperty = "Paid";
    //}

    #region IXafEntityObject members (see https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.IXafEntityObject)
    void IXafEntityObject.OnCreated()
    {
        // Place the entity initialization code here.
        // You can initialize reference properties using Object Space methods; e.g.:
        // this.Address = objectSpace.CreateObject<Address>();
    }
    void IXafEntityObject.OnLoaded()
    {
        // Place the code that is executed each time the entity is loaded here.
    }
    void IXafEntityObject.OnSaving()
    {
        // Place the code that is executed each time the entity is saved here.
    }
    #endregion

    #region IObjectSpaceLink members (see https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.IObjectSpaceLink)
    // If you implement this interface, handle the NonPersistentObjectSpace.ObjectGetting event and find or create a copy of the source object in the current Object Space.
    // Use the Object Space to access other entities (see https://docs.devexpress.com/eXpressAppFramework/113707/data-manipulation-and-business-logic/object-space).
    //IObjectSpace IObjectSpaceLink.ObjectSpace {
    //    get { return objectSpace; }
    //    set { objectSpace = value; }
    //}
    #endregion

    #region INotifyPropertyChanged members (see https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-8.0&redirectedfrom=MSDN)
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion
}