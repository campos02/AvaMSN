namespace AvaMSN.MSNP.XML.SerializableClasses;

// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class msnobj
{

    private string creatorField;

    private byte typeField;

    private string sHA1DField;

    private ushort sizeField;

    private byte locationField;

    private string friendlyField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Creator
    {
        get
        {
            return this.creatorField;
        }
        set
        {
            this.creatorField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte Type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string SHA1D
    {
        get
        {
            return this.sHA1DField;
        }
        set
        {
            this.sHA1DField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public ushort Size
    {
        get
        {
            return this.sizeField;
        }
        set
        {
            this.sizeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte Location
    {
        get
        {
            return this.locationField;
        }
        set
        {
            this.locationField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Friendly
    {
        get
        {
            return this.friendlyField;
        }
        set
        {
            this.friendlyField = value;
        }
    }
}

