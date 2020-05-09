using EasySave.Model;
using EasySave.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Controller
{
    public interface IMainController
    {   
        List<IBackup> backup { get; set; }
        View.View View { get; set; }
        string[] blacklisted_apps { get; set; }

        string Add_save(string name, string source_folder, string target_folder, string backuptype);
        IBackup Last_backup();
        string Remove_task(int indextask);
        string Remove_alltasks();
        string Save_alltasks();
        string Save_task(int indextask);
        string Save_diff(bool fulldiff, int indextask);
        string Informations_items(int index);
        Dictionary<string, Dictionary<string, string>> getLanguageDict();
        void Close();
        bool IsAPriorityTaskRunning();
        void Update_progressbar();
        void Play_Pause(string name);
        void Stop(string name);
        void KillThread(string name);
    }
}
