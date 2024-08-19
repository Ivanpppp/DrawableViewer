using System.Windows;
using System.Windows.Forms;

#if AUTOCAD2015_TO_2024
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#elif GSTARCAD2024_TO_2025
using Gssoft.Gscad.Runtime;
using Application = Gssoft.Gscad.ApplicationServices.Application;
#elif GSTARCAD2017_TO_2023
using GrxCAD.Runtime;
using Application = GrxCAD.ApplicationServices.Application;
#endif

namespace DrawableViewer.Test
{
    public class Command
    {
        [CommandMethod(nameof(TestFormViewer)
#if DEBUG
            , CommandFlags.Session
#endif
        )]
        public static void TestFormViewer()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                using (doc.TransactionManager.StartTransaction())
                using (var form = new FormViewer
                       {
                           StartPosition = FormStartPosition.CenterParent, ShowInTaskbar = false
                       })
                {
                    Application.ShowModalDialog(Application.MainWindow.Handle, form, false);
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);
            }
        }

        [CommandMethod(nameof(TestWpfViewer)
#if DEBUG
            , CommandFlags.Session
#endif
        )]
        public static void TestWpfViewer()
        {
            try
            {
                var database = Application.DocumentManager.MdiActiveDocument.Database;
                using (database.TransactionManager.StartTransaction())
                using (var viewModel = new ViewModel())
                {
                    var window = new WpfViewer
                    {
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        ShowInTaskbar = false,
                        DataContext = viewModel
                    };
                    Application.ShowModalWindow(Application.MainWindow.Handle, window, false);
                }
            }
            catch (System.Exception ex)
            {
                Application.ShowAlertDialog(ex.Message);
            }
        }
    }
}