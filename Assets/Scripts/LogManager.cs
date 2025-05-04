using System;
using System.IO;
using UnityEngine;

namespace CompasXR.Systems
{
    /*
    * CompasXR.Systems : A namespace to define and controll various system
    * level settings and configurations.
    */

    public class LogManager : MonoBehaviour
    {
        private string logDirectoryPath;
        private string logFilePath;

    //////////////////////////// Monobehaviour Methods //////////////////////////////
        void Awake()
        {
            DontDestroyOnLoad(gameObject);

                // 1) Get the path to the Assets folder
                string assetsPath = Application.dataPath;  // .../compas_xr_unity_dafne/Assets

                // 2) Go up one level to the project root
                string projectRoot = Directory.GetParent(assetsPath).FullName;  
                // .../compas_xr_unity_dafne

                // 3) Point at your Logs folder in the root
                logDirectoryPath = Path.Combine(projectRoot, "Logs", "CompasXRLogStorage");
                
                // 4) Build your filename exactly as before
                string dateString  = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                logFilePath        = Path.Combine(logDirectoryPath,
                                                $"{dateString}_{SystemInfo.deviceUniqueIdentifier}_log.txt");

            ManageLogDirectory(logDirectoryPath);

            Application.logMessageReceived += HandleLogMessage;
        }
        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLogMessage;
            Debug.Log($"OnDestroy: LogManager saved log file to {logFilePath}");    
        }

    //////////////////////////// Log Management Methods //////////////////////////////
        public void ManageLogDirectory(string directoryPath)
        {
            /*
            * ManageLogDirectory : Method is used to manage the log directory
            * storage by deleting old log files.
            */

            if (!Directory.Exists(logDirectoryPath))
            {
                Directory.CreateDirectory(logDirectoryPath);
            }

            string [] files = Directory.GetFiles(directoryPath);
            if(files.Length >= 250)
            {
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
        }
        private void HandleLogMessage(string logString, string stackTrace, LogType type)
        {
            /*
            * HandleLogMessage : Method write log information to the log file.
            * format: [DateTime]: SceneName: LogType: LogMessage
            */
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"[{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}]: {sceneName}: {type}: {logString}");
                //writer.WriteLine(stackTrace);
                //writer.WriteLine();
            }
        }
    }
}
