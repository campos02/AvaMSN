﻿namespace AvaMSN.Models;

public class Profile : Contact
{
    public int UserID { get; set; }
    public byte[]? DisplayPictureData { get; set; }
}
