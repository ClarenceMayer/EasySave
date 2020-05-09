using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.View
{
    interface IDisplay
    {
        void Help();
        void Error(string error);
        string Readline();
        void Success(string success, string name);
        void AskSave(string ask, string name);
        void Writeline(string write);

    }
}
