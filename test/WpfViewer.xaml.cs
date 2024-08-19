using System;
using System.Windows;

namespace DrawableViewer.Test
{
    /// <summary>
    /// Interaction logic for WpfViewer.xaml
    /// </summary>
    public partial class WpfViewer : Window
    {
        public WpfViewer()
        {
            InitializeComponent();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            Viewer.Dispose();
        }
    }
}
