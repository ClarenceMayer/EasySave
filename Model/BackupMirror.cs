using EasySave.Controller;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;

namespace EasySave.Model
{
    class BackupMirror : IBackup
    {
        public BackupMirror(string _name, string _source_folder, string _target_folder, IMainController c)
        {
            //check if source directory exist 
            if (_source_folder[0] != '\\')
            {
                DirectoryInfo diSource = new DirectoryInfo(_source_folder);
                if (!diSource.Exists)
                {
                    Console.WriteLine("source folder not found");
                }
            }

            name = _name;
            source_folder = _source_folder;
            target_folder = _target_folder;
            m_realTimeMonitoring = new RealTimeMonitoring(name);
            m_realTimeMonitoring.SetPaths(source_folder, target_folder);
            controller = c;
        }

        private RealTimeMonitoring m_realTimeMonitoring;
        private DailyLog m_daily_log;
        private IMainController controller;

        private int current_file;
        private string m_name;
        private string m_source_folder;
        private string m_target_folder;
        private bool m_priority_work_in_progress = false;
        private bool m_is_on_break = false;

        public string name { get => m_name; set => m_name = value; }
        public string source_folder { get => m_source_folder; set => m_source_folder = value; }
        public string target_folder { get => m_target_folder; set => m_target_folder = value; }
        public bool priority_work_in_progress { get => m_priority_work_in_progress; set => m_priority_work_in_progress = value; }
        public bool is_on_break { get => m_is_on_break; set => m_is_on_break = value; }


        //Launching save, setting directory to copy and create save path
        public void LaunchSave()
        {
           
            current_file = 0;
            DirectoryInfo di = new DirectoryInfo(m_source_folder);
            string path = target_folder + '/' + name;
            FullSavePrio(di, path);
            FullSave(di, path);

            m_realTimeMonitoring.GenerateFinalLog();
            controller.Update_progressbar();

            controller.KillThread(name);
            
        }

        //Mirror save
        public void FullSave(DirectoryInfo di, string target_path)
        {
            priority_work_in_progress = false;
            //check if target directory exist, in case he doesn't create the directory
            DirectoryInfo diTarget = new DirectoryInfo(target_path);
            if (!diTarget.Exists)
            {
                diTarget.Create();
            }
            //foreach file in source directory, copy it in target directory
            foreach (FileInfo fi in di.GetFiles())
            {
                while (is_on_break || Utils.checkBusinessSoft(controller.blacklisted_apps) || controller.IsAPriorityTaskRunning()) { }
                if (!Utils.IsPriority(fi.Extension))
                {
                    if(fi.Length> Convert.ToInt16(ConfigurationSettings.AppSettings["MaxSizeFile"])){
                        lock (controller)
                        {
                            Save(fi, target_path);
                        }
                    }
                    else
                    {
                        Save(fi, target_path);
                    }
                }
            }
            //get all sub-directory and foreach call the save function(recursive)
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                string temp_path = target_path +'/'+ subdir.Name;
                FullSave(subdir, temp_path);
            }
            m_realTimeMonitoring.GenerateFinalLog();
        }

        private void FullSavePrio(DirectoryInfo di, string target_path)
        {
            priority_work_in_progress = true;
            //check if target directory exist, in case he doesn't create the directory
            DirectoryInfo diTarget = new DirectoryInfo(target_path);
            if (!diTarget.Exists)
            {
                diTarget.Create();
            }
            //foreach file in source directory, copy it in target directory
            foreach (FileInfo fi in di.GetFiles())
            {
                while ( is_on_break || Utils.checkBusinessSoft(controller.blacklisted_apps)){}
                if (Utils.IsPriority(fi.Extension))
                {
                    if (fi.Length > Convert.ToInt16(ConfigurationSettings.AppSettings["MaxSizeFile"])){
                        lock (controller)
                        {
                            Save(fi, target_path);
                        }
                    }
                    else
                    {
                        Save(fi, target_path);
                    }
                }
            }
            //get all sub-directory and foreach call the save function(recursive)
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                string temp_path = target_path + '/' + subdir.Name;
                FullSavePrio(subdir, temp_path);
            }
            
        }

        private void Save(FileInfo fi, string target_path)
        {
            
            m_daily_log = DailyLog.Instance;
            m_daily_log.SetPaths(fi.FullName);
            m_daily_log.millisecondEarly();

            m_realTimeMonitoring.GenerateLog(current_file);
            controller.Update_progressbar();
            current_file++;
            string temp_path = target_path + '/' + fi.Name;
            //check if the extension is the list to encrypt
            if (Utils.IsToCrypt(fi.Extension))
            {
                m_daily_log.Crypt_time = Utils.Crypt(fi.FullName, temp_path);
            }
            else
            {
                fi.CopyTo(temp_path, true);
                m_daily_log.Crypt_time = "0";
            }


            m_daily_log.millisecondFinal();
            lock (m_daily_log)
            {
                m_daily_log.generateDailylog(target_folder, source_folder);
            }
            
        }
        public void LaunchSaveInc(object state)
        {
            LaunchSave();
        }
        
    }
}
