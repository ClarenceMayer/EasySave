using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace EasySave.Model
{
    static class Utils
    {
        //method to use CryptoSoft
        //CryptoSoft take two arguments, source file and target file and return crypt time in ms or -1 if an error happend
        //CryproSoft take the source file, crypt it and then copy it in the the taget file
        public static string Crypt(string source_file, string target_file)
        {
            string ret;

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.FileName = ConfigurationSettings.AppSettings["CryptoSoftPath"];

            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.Arguments = source_file + " " + target_file;


            try 
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                   ret = exeProcess.StandardOutput.ReadToEnd() + " ms";
                    exeProcess.WaitForExit();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ret = null;
            }
            return ret;
        }

        //read the json file that list all extension to crypt and return an array of string with the extension to crypt
        private static string[] getCryptList()
        {
            using (StreamReader r = new StreamReader(ConfigurationSettings.AppSettings["ExtensionList"]))
            {
                CryptListFormat[] item_cryptlist;
                string[] cryptlist_extensions_array;
                string json = r.ReadToEnd();
                List<CryptListFormat> items = JsonConvert.DeserializeObject<List<CryptListFormat>>(json);
                item_cryptlist = items.ToArray();
                cryptlist_extensions_array = item_cryptlist[0].extension_to_crypt.Split(',');

                return cryptlist_extensions_array;
            }
        }

        //check if a given extension is the list to crypt
        public static bool IsToCrypt(string extension)
        {
                foreach (string crypt_ext in getCryptList())
                {
                    if(crypt_ext == extension)
                    {
                    return true;
                    }
                }
                return false;
        }

        //read the json file of blacklisted apps and return the array list of blacklisted apps
        public static string[] getBlacklist()
        {
            using (StreamReader r = new StreamReader(ConfigurationSettings.AppSettings["softwareBlacklist"]))
            {
                BlacklistFormat[] item_blacklist;
                string[] blacklisted_apps_array;
                string json = r.ReadToEnd();
                List<BlacklistFormat> items = JsonConvert.DeserializeObject<List<BlacklistFormat>>(json);
                item_blacklist = items.ToArray();
                blacklisted_apps_array = item_blacklist[0].blacklisted_items.Split(',');

                return blacklisted_apps_array;
            }
        }

        //check if process in blacklist apps are currently running
        public static bool checkBusinessSoft(string[] blacklisted_apps)
        {
            foreach (string process in blacklisted_apps)
            {
                if (Process.GetProcessesByName(process).Length > 0)
                {
                    return true;
                }
            }
            return false;
        }
        //read the json file that list all extension to crypt and return an array of string with the extension to crypt
        private static string[] getPriorityList()
        {
            using (StreamReader r = new StreamReader(ConfigurationSettings.AppSettings["PriorityList"]))
            {
                PrioListFormat[] item_Priolist;
                string[] cryptlist_extensions_array;
                string json = r.ReadToEnd();
                List<PrioListFormat> items = JsonConvert.DeserializeObject<List<PrioListFormat>>(json);
                item_Priolist = items.ToArray();
                cryptlist_extensions_array = item_Priolist[0].priority_extension.Split(',');

                return cryptlist_extensions_array;
            }
        }

        //check if a given extension is the list to crypt
        public static bool IsPriority(string extension)
        {
            foreach (string priority_ext in getPriorityList())
            {
                if (priority_ext == extension)
                {
                    return true;
                }
            }
            return false;
        }


        //read Json file and return the value of the propretie put in the argument.
        public static string JsonReader(string path, string propertie)
        {
            try{
                path = path.Replace(@"\\", @"/");

                string text = File.ReadAllText(path);
                text = text.Replace("]", "");
                string[] Objects = text.Split('[');
                string obj = Objects[1];
                string[] lines = obj.Split(',');
                foreach (string line in lines)
                {
                    if (line.Contains(propertie))
                    {
                        return line.Split(':')[1].Replace("\"", "");
                    }
                }
            }
            catch
            {
                return "";
            }
            return "";
        }

        public static Dictionary<string, Dictionary<string, string>> loadLanguage()
        {
            using (StreamReader r = new StreamReader(ConfigurationSettings.AppSettings["LanguageDict"]))
            {
                Dictionary<string, Dictionary<string, string>> language_dict = new Dictionary<string, Dictionary<string, string>>();
                string json = r.ReadToEnd();

                dynamic items = JsonConvert.DeserializeObject<dynamic>(json);
                Dictionary<string, string> english_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(items.english.ToString());
                Dictionary<string, string> french_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(items.french.ToString());
                language_dict.Add("english", english_dict);
                language_dict.Add("french", french_dict);

                return language_dict;
            }
        }
    }
}

           

        
