using System;
using System.Windows.Forms;

#if AUTOCAD2015_TO_2024
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#elif GSTARCAD2024_TO_2025
using Gssoft.Gscad.DatabaseServices;
using Gssoft.Gscad.Geometry;
using Application = Gssoft.Gscad.ApplicationServices.Core.Application;
#elif GSTARCAD2017_TO_2023
using GrxCAD.DatabaseServices;
using GrxCAD.Geometry;
using Application = GrxCAD.ApplicationServices.Application;
#endif

namespace DrawableViewer.Test
{
    public partial class FormViewer : Form
    {
        private readonly Circle _circle = new Circle(Point3d.Origin, new Vector3d(1,1,1), 100);

        public FormViewer()
        {
            InitializeComponent();

            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _circle.Dispose();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            viewer1.EraseAll();
            var doc = Application.DocumentManager.MdiActiveDocument;
            var trans = doc.TransactionManager.TopTransaction;
            var currentSpace = (BlockTableRecord)trans.GetObject(doc.Database.CurrentSpaceId, OpenMode.ForRead);
            viewer1.Add(currentSpace);
            viewer1.ZoomExtents();
            viewer1.Invalidate();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            viewer1.EraseAll();

            viewer1.Add(_circle);
            viewer1.ZoomExtents();
            viewer1.Invalidate();
        }
    }
}