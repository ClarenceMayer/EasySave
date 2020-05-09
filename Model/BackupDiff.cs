using EasySave.Controller;
using System;
using System.Configuration;
using System.IO;

namespace EasySave.Model
{
    class BackupDiff : IBackup

    {
        public BackupDiff(string _name, string _source_folder, string _target_folder, IMainController c)
        {
            //check if source directory exist if it is not an extern storage
            if (_source_folder[0] != '\\'){
                DirectoryInfo diSource = new DirectoryInfo(_source_folder);
                if (!diSource.Exists)
                {
                    Console.WriteLine("Source file not found");
                }
            }

            name = _name;
            source_folder = _source_folder;
            target_folder = _target_folder;
            first_save = true;
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
        private bool m_first_save;
        private bool m_priority_work_in_progress = false;
        private bool m_is_on_break = false;

        public string name { get => m_name; set => m_name = value; }
        public string source_folder { get => m_source_folder; set => m_source_folder = value; }
        public string target_folder { get => m_target_folder; set => m_target_folder = value; }
        public bool first_save { get => m_first_save; set => m_first_save = value; }
        public bool is_on_break { get => m_is_on_break; set => m_is_on_break = value; }
        public bool priority_work_in_progress { get => m_priority_work_in_progress; set => m_priority_work_in_progress = value; }


        //launch save, check if it is the first save, if it is do a full save, else do an incremental save
        public void LaunchSave()
        {
            current_file = 0;
            DirectoryInfo di = new DirectoryInfo(m_source_folder);
            if (first_save)
            {
                string target_path = target_folder + '/' + name + "/completeBackup/";
                first_save = false;
                FullSavePrio(di, target_path);
                FullSave(di, target_path);
            }
            else
            {
                string complete_path = target_folder + '/' + name + "/completeBackup/";
                string target_path = target_folder + '/' + name + "/tempBackup/";

                IncrementSavePrio(di, target_path, complete_path);
                IncrementSave(di, target_path, complete_path);
            }
            m_realTimeMonitoring.GenerateFinalLog();
            controller.Update_progressbar();

            controller.KillThread(name);
        }
        //launch save, user choose if it is a full or incremental save, if it is the first save he can't force incremental save
        public void LaunchSaveInc(object full_save)
        {
            current_file = 0;
            DirectoryInfo di = new DirectoryInfo(m_source_folder);
            if (!(bool)full_save && !first_save)
            {
                string complete_path = target_folder + '/' + name + "/completeBackup/";
                string target_path = target_folder + '/' + name + "/tempBackup/";
                IncrementSavePrio(di, target_path, complete_path);
                IncrementSave(di, target_path, complete_path);
            }
            else
            {
                string complete_path = target_folder + '/' + name + "/completeBackup/";
                first_save = false;
                FullSavePrio(di, complete_path);
                FullSave(di, complete_path);
            }
            m_realTimeMonitoring.GenerateFinalLog();
            controller.Update_progressbar();

            controller.KillThread(name);
        }

        //Mirror save
        public void FullSave(DirectoryInfo di, string target_path)
        {
            priority_work_in_progress = false;
            DirectoryInfo diTarget = new DirectoryInfo(target_path);
            if (!diTarget.Exists)
            {
                diTarget.Create();
            }
            foreach (FileInfo fi in di.GetFiles())
            {
                while (is_on_break || Utils.checkBusinessSoft(controller.blacklisted_apps) || controller.IsAPriorityTaskRunning()) { }
                if (!Utils.IsPriority(fi.Extension))
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
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                target_path += '/' + subdir.Name;
                FullSave(subdir, target_path);
            } 
        }

        public void FullSavePrio(DirectoryInfo di, string target_path)
        {
            priority_work_in_progress = true;
            DirectoryInfo diTarget = new DirectoryInfo(target_path);
            if (!diTarget.Exists)
            {
                diTarget.Create();
            }
            foreach (FileInfo fi in di.GetFiles())
            {
                while (is_on_break || Utils.checkBusinessSoft(controller.blacklisted_apps)) { }
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
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                target_path += '/' + subdir.Name;
                FullSavePrio(subdir, target_path);
            }
        }

        //IncrementalSave
        public void IncrementSave(DirectoryInfo di, string target_path, string complete_path)
        {
            priority_work_in_progress = false;
            DirectoryInfo diTarget = new DirectoryInfo(target_path);
            if (!diTarget.Exists)
            {
                diTarget.Create();
            }
            // for each file in the directory, check if it was modified
            DirectoryInfo dirComplete = new DirectoryInfo(complete_path);
            foreach (FileInfo fi in di.GetFiles())
            {
                if (Utils.checkBusinessSoft(controller.blacklisted_apps))
                {
                    is_on_break = true;
                }
                while (is_on_break || Utils.checkBusinessSoft(controller.blacklisted_apps) || controller.IsAPriorityTaskRunning()) { }
                if (!Utils.IsPriority(fi.Extension))
                {
                    if (fi.Length > Convert.ToInt16(ConfigurationSettings.AppSettings["MaxSizeFile"])){
                        lock (controller)
                        {
                            SaveInc(fi, target_path,dirComplete);
                        }
                    }
                    else
                    {
                        SaveInc(fi, target_path, dirComplete);
                    }
                }
            }
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                target_path += '/' + subdir.Name;
                complete_path += '/' + subdir.Name;
                IncrementSave(subdir, target_path,complete_path);
            }
            DeleteEmptyFolder(diTarget);
        }

        public void IncrementSavePrio(DirectoryInfo di, string target_path, string complete_path)
        {
            priority_work_in_progress = true;
            DirectoryInfo diTarget = new DirectoryInfo(target_path);
            if (!diTarget.Exists)
            {
                diTarget.Create();
            }
            // for each file in the directory, check if it was modified
            DirectoryInfo dirComplete = new DirectoryInfo(complete_path);
            foreach (FileInfo fi in di.GetFiles())
            {
                while (is_on_break || Utils.checkBusinessSoft(controller.blacklisted_apps)) { }
                if (Utils.IsPriority(fi.Extension))
                {
                    if (fi.Length > Convert.ToInt16(ConfigurationSettings.AppSettings["MaxSizeFile"])){
                        lock (controller)
                        {
                            SaveInc(fi, target_path, dirComplete);
                        }
                    }
                    else
                    {
                        SaveInc(fi, target_path, dirComplete);
                    }
                }
            }
            DirectoryInfo[] dirs = di.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                target_path += '/' + subdir.Name;
                complete_path += '/' + subdir.Name;
                IncrementSavePrio(subdir, target_path, complete_path);
            }
        }

        //recursive method that delete all empty folder
        public void DeleteEmptyFolder(DirectoryInfo dir)
        {
            DirectoryInfo[] subdirs = dir.GetDirectories();
            foreach (DirectoryInfo subdir in subdirs)
            {
                DeleteEmptyFolder(subdir);
            }
            if (dir.GetFiles("*", SearchOption.AllDirectories).Length == 0)
            {
                dir.Delete();
            } 
        }

        //check if a file don t exist in a given directory
        private bool CheckNewFile(FileInfo fiBase, DirectoryInfo dirTarget)
        {
            bool new_file = true;
            foreach (FileInfo fi in dirTarget.GetFiles())
            {
                if (fi.Name == fiBase.Name)
                {
                    new_file = false;
                }
            }
            return new_file;
        }
        //check if a file got a modified version on an other target
        private bool CheckModification(FileInfo fiBase, DirectoryInfo dirTarget)
        {
            bool update_file = false;
            foreach (FileInfo fi in dirTarget.GetFiles())
            {
                if (fiBase.Name == fi.Name)
                {
                    if (fiBase.Length != fi.Length)
                    {
                        update_file = true;
                    }
                }
            }
            return update_file;
        }

        public void SaveInc(FileInfo fi, string target_path, DirectoryInfo dirComplete)
        {
            //Copy the current file in a temp folder and crypt it if necessary
            if (Utils.IsToCrypt(fi.Extension))
            {
                m_daily_log.Crypt_time = Utils.Crypt(fi.FullName, fi.Name);
            }
            else
            {
                fi.CopyTo(fi.Name);
            }
            FileInfo fiTemp = new FileInfo(fi.Name);

            //check if it is a new file or if the file was modified based on the full save
            if (CheckNewFile(fiTemp, dirComplete) || CheckModification(fiTemp, dirComplete))
            {
                m_daily_log = DailyLog.Instance;
                m_daily_log.SetPaths(fi.FullName);
                m_daily_log.millisecondEarly();

                m_realTimeMonitoring.GenerateLog(current_file);
                this.controller.Update_progressbar();
                current_file++;
                string temp_path = target_path + '/' + fi.Name;
                fiTemp.CopyTo(temp_path, true);

                m_daily_log.millisecondFinal();
                lock (m_daily_log)
                {
                    m_daily_log.generateDailylog(target_folder, source_folder);
                }
            }
            //delete the temporary file
            fiTemp.Delete();
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
    }
}
