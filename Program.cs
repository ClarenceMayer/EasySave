using System;

using EasySave.Controller;

namespace EasySave
{
    class Program
    {
        static void Main(string[] args)
        {
            MainController controller = new MainController();
            controller.Run();
            
            // Console.Read();
        }
    }
}
