﻿namespace AvaMSN.MSNP.XML.SerializableClasses;

// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class ml
{

    private mlD[] dField;

    private byte lField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("d")]
    public mlD[] d
    {
        get
        {
            return this.dField;
        }
        set
        {
            this.dField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte l
    {
        get
        {
            return this.lField;
        }
        set
        {
            this.lField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class mlD
{

    private mlDC cField;

    private string nField;

    /// <remarks/>
    public mlDC c
    {
        get
        {
            return this.cField;
        }
        set
        {
            this.cField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string n
    {
        get
        {
            return this.nField;
        }
        set
        {
            this.nField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class mlDC
{

    private string nField;

    private byte lField;

    private byte tField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string n
    {
        get
        {
            return this.nField;
        }
        set
        {
            this.nField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte l
    {
        get
        {
            return this.lField;
        }
        set
        {
            this.lField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte t
    {
        get
        {
            return this.tField;
        }
        set
        {
            this.tField = value;
        }
    }
}

