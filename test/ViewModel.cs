using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

#if AUTOCAD2015_TO_2024
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.GraphicsInterface;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#elif GSTARCAD2024_TO_2025
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Geometry;
using Gssoft.Gscad.GraphicsInterface;
using Application = Gssoft.Gscad.ApplicationServices.Core.Application;
#elif GSTARCAD2017_TO_2023
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using GrxCAD.GraphicsInterface;
using Application = GrxCAD.ApplicationServices.Application;
#endif

namespace DrawableViewer.Test
{
    public class ViewModel : INotifyPropertyChanged, IDisposable
    {
        private Drawable _drawable;

        public Drawable Drawable
        {
            get => _drawable;
            set => SetProperty(ref _drawable, value, nameof(Drawable));
        }

        private ICommand _drawCircleCommand;

        public ICommand DrawCircleCommand => _drawCircleCommand ?? (_drawCircleCommand = new DelegateCommand(() =>
        {
            Dispose();

            try
            {
                Drawable = new Circle(Point3d.Origin, Vector3d.ZAxis, 100);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }));

        private ICommand _drawCurrentSpaceCommand;

        public ICommand DrawCurrentSpaceCommand =>
            _drawCurrentSpaceCommand ?? (_drawCurrentSpaceCommand = new DelegateCommand(() =>
            {
                Dispose();

                var database = Application.DocumentManager.MdiActiveDocument.Database;
                var trans = database.TransactionManager.TopTransaction;
                var currentSpace = (BlockTableRecord)trans.GetObject(database.CurrentSpaceId, OpenMode.ForRead);
                try
                {
                    Drawable = currentSpace;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }));

        private ICommand _clearCommand;

        public ICommand ClearCommand => _clearCommand ?? (_clearCommand = new DelegateCommand(() =>
        {
            Dispose();
            Drawable = null;
        }));

        public void Dispose()
        {
            if (Drawable != null && Drawable.AutoDelete)
            {
                Drawable.Dispose();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}