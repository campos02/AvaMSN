namespace AvaMSN.MSNP.XML.SerializableClasses;

// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class Data
{

    private string currentMediaField;

    private string pSMField;

    private string machineGuidField;

    private string dDPField;

    /// <remarks/>
    public string CurrentMedia
    {
        get
        {
            return this.currentMediaField;
        }
        set
        {
            this.currentMediaField = value;
        }
    }

    /// <remarks/>
    public string PSM
    {
        get
        {
            return this.pSMField;
        }
        set
        {
            this.pSMField = value;
        }
    }

    /// <remarks/>
    public string MachineGuid
    {
        get
        {
            return this.machineGuidField;
        }
        set
        {
            this.machineGuidField = value;
        }
    }

    /// <remarks/>
    public string DDP
    {
        get
        {
            return this.dDPField;
        }
        set
        {
            this.dDPField = value;
        }
    }
}

