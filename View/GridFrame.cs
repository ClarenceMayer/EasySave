using EasySave.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EasySave.View
{
    public class GridFrame
    {
        static Grid Grid = new Grid();
        public static void CreateWorkList(IMainController c)
        {
            Grid.Width = 700;
            Grid.Height = 800;
            Grid.HorizontalAlignment = HorizontalAlignment.Left;
            Grid.VerticalAlignment = VerticalAlignment.Center;
            Grid.Background = Brushes.Transparent;
            View userControl = new View(c);
            Frame.RootGrid.Children.Add(Grid);
            Grid.Children.Add(userControl);
        }
    }
}
