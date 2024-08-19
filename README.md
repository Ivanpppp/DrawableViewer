# Drawable object viewer
### GstarCAD Extension Library

This is an unofficial extension library enhanced for GstarCAD .NET SDK.

> You can get GstarCAD SDK on the official website https://www.gstarcad.com/download/, or on the [Nuget Gallery](https://www.nuget.org/packages/GstarCADNET).

### Windows Forms Sample
Windows forms sample can be written:
```csharp
using (var circle = new Circle(Point3d.Origin, Vector3d.ZAxis, 100))
using (var form = new Form { Size = new Size(300, 350) })
{
    var viewer = new DrawableViewer() { Dock = DockStyle.Fill };
    viewer.Add(circle);
    viewer.ZoomExtents();
    form.Controls.Add(viewer);
    form.ShowDialog();
}
```

### WPF MVVM Sample
The xaml file can be written:
```xaml
<Window x:Class="DrawableViewer.Test.WpfViewer"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
      xmlns:dv="http://shaprer.gstarcad.extensions/xaml"
      Height="450" Width="800">
  <Grid>
    <WindowsFormsHost Grid.Row="0" Grid.Column="0" Margin="10"
        dv:DrawableViewerBehavior.DrawableObject="{Binding Drawable}"
        dv:DrawableViewerBehavior.AutoZoomingWhenDrawableChanged="True">
      <dv:DrawableViewer />
    </WindowsFormsHost>
  </Grid>
</Window>
```
### Build the project
```cmd
> build.bat
```

> If 'msbuild' is not recognized, please set compilation environment firstly.

### Deployment

This library is published as a [nuget package](https://www.nuget.org/packages/DrawableViewer) in nuget.org.
