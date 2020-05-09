using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace EasySave.View
{
    public class Display : IDisplay
    {
       //Constructor of Display
        public Display()
        {
            Help();

        }
        //method that display the different cmd
        public void Help()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Enter -help to view all commands console");
            Console.WriteLine("Enter -add <name> <mir|diff> <source folder> <target folder> to add file in queue");
            Console.WriteLine("Enter -remove <id> or <all> to remove file in queue");
            Console.WriteLine("Enter -save <id> or <all> to save your files");
            Console.WriteLine("Enter -show to show all files");
            Console.ResetColor();

        }
        //method that contains the different error message
        public void Error(string error)
        {
            switch(error)
            {
                case "-save":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("please enter <id> or <all>");
                    Console.ResetColor();
                    break;
                case "-add":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("please enter <name> <mir|diff> <source folder> <target folder>");
                    Console.ResetColor();
                    break;
                case "-add fullqueue":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("please remove one file in queue to add one");
                    Console.ResetColor();
                    break;
                case "-remove":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("please enter <id> or <all>");
                    Console.ResetColor();
                    break;
                case "-businesswarerunning":
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("business software running, check the blacklist file for more infos");
                    Console.ResetColor();
                    break;


            }
        }
        //method that contains the different Success message
        public void Success(string success, string name)
        {
            switch (success)
            {
                case "-add":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Task"+" "+name+" "+"successfully added");
                    Console.ResetColor();
                    break;
                case "-remove":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Task successfully removed");
                    Console.ResetColor();
                    break;
                case "-remove all":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Tasks successfully removed");
                    Console.ResetColor();
                    break;
                
                case "-save":
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Task" + " " + name + " " + " successfully saved");
                    Console.ResetColor();
                    break;
            }
        }
        //method that ask if the user want to make a full save
        public void AskSave(string ask, string name)
        {
            switch (ask)
            {
                case "diff full":
                    Console.WriteLine("Do you want to do a full save for"+" " + name+ " " +"? [y/n]");
                    break;
                
            }
        }
        //method that read user text
        public string Readline()
        {
            return Console.ReadLine();
        }
        //method that writeline
        public void Writeline(string write)
        {
            Console.WriteLine(write);
        }
    }
}
