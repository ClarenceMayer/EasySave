using EasySave.Controller;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Windows;

namespace EasySave.Model
{
    class BackupMirror : IBackup
    {
        public BackupMirror(string _name, string _source_folder, string _target_folder, IMainController c)
        {
                name = _name;
                source_folder = _source_folder;
                target_folder = _target_folder;
                m_realTimeMonitoring = new RealTimeMonitoring(name);
                m_realTimeMonitoring.SetPaths(source_folder);
                controller = c;
        }

        private RealTimeMonitoring m_realTimeMonitoring;
        private DailyLog m_daily_log;
        private IMainController controller;

        private int current_file;
        private string m_name;
        private string m_source_folder;
        private string m_target_folder;

        public string name { get => m_name; set => m_name = value; }
        public string source_folder { get => m_source_folder; set => m_source_folder = value; }
        public string target_folder { get => m_target_folder; set => m_target_folder = value; }

        //Launching save, setting directory to copy and create save path
        public void LaunchSave()
        {
            try
            {
                current_file = 0;
                DirectoryInfo di = new DirectoryInfo(m_source_folder);
                string path = target_folder + '/' + name;
                FullSavePrio(di, path);
                FullSave(di, path);
                lock (m_realTimeMonitoring)
                {
                    m_realTimeMonitoring.GenerateFinalLog();
                }
                lock (m_realTimeMonitoring)
                {
                    controller.Update_progressbar();
                }
            }
            catch (Exception ex)
            {
                if (!ex.ToString().Contains("System.Threading.ThreadAbortException"))
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            controller.KillThread(name);
           
        }

        //Mirror save
        public void FullSave(DirectoryInfo di, string target_path)
        {
            //check if target directory exist, in case he doesn't create the directory
            DirectoryInfo diTarget = new DirectoryInfo(target_path);
            if (!diTarget.Exists)
            {
                diTarget.Create();
            }
            //foreach file in source directory, copy it in target directory
            foreach (FileInfo fi in di.GetFiles())
            {
                //while (is_on_break || Utils.checkBusinessSoft(controller.blacklisted_apps) || controller.IsAPriorityTaskRunning()) { }
                if (Utils.checkBusinessSoft(controller.blacklisted_apps))
                {
                    MessageBox.Show("A business software has been detected, task will be canceled !");
                    controller.KillThread(name);
                }
                if (!Utils.IsPriority(fi.Extension))
                {
                    if(fi.Length> Convert.ToInt16(ConfigurationSettings.AppSettings["MaxSizeFile"])){
                        lock (controller.bigFileLock)
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
            //check if target directory exist, in case he doesn't create the directory
            DirectoryInfo diTarget = new DirectoryInfo(target_path);
            if (!diTarget.Exists)
            {
                diTarget.Create();
            }
            //foreach file in source directory, copy it in target directory
            foreach (FileInfo fi in di.GetFiles())
            {
                //while ( is_on_break || Utils.checkBusinessSoft(controller.blacklisted_apps)){}
                if (Utils.checkBusinessSoft(controller.blacklisted_apps))
                {
                    MessageBox.Show("A business software has been detected, task will be canceled !");
                    controller.KillThread(name);
                }
                if (Utils.IsPriority(fi.Extension))
                {
                    if (fi.Length > Convert.ToInt16(ConfigurationSettings.AppSettings["MaxSizeFile"])){
                        lock (controller.bigFileLock)
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

            lock (m_realTimeMonitoring)
            {
                m_realTimeMonitoring.GenerateLog(current_file);
            }
            lock (m_realTimeMonitoring)
            {
                controller.Update_progressbar();
            }
            
                        
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
