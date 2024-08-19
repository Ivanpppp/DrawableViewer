using System.Runtime.InteropServices;
using System.Windows.Markup;

// In SDK-style projects such as this one, several assembly attributes that were historically
// defined in this file are now automatically added during build and populated with
// values defined in project properties. For details of which attributes are included
// and how to customise this process see: https://aka.ms/assembly-info-properties


// Setting ComVisible to false makes the types in this assembly not visible to COM
// components.  If you need to access a type in this assembly from COM, set the ComVisible
// attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM.

[assembly: Guid("3d750545-b489-4747-a3f6-b7e734a8e9f0")]
[assembly: XmlnsDefinition("http://shaprer.gstarcad.extensions/xaml",
#if AUTOCAD2015_TO_2024
    "Sharper.AutoCAD.DrawableViewer"
#else
    "Sharper.GstarCAD.Extensions"
#endif
)]