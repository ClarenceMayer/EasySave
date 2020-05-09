using EasySave.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EasySave.View
{
    public class Frame : Window

    {

        public Window window = new Window();
        public static Grid RootGrid;

        public void InitFrame(IMainController c)
        {
            window.Width = 700;
            window.Height = 800;
            window.Title = "EasySave";
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            RootGrid = new Grid();
            RootGrid.Height = window.Width;
            RootGrid.Width = window.Height;
            RootGrid.Background = Brushes.White;
            GridFrame.CreateWorkList(c);
            window.Content = RootGrid;
            window.Show();
            System.Windows.Threading.Dispatcher.Run();
        }
    }
}
