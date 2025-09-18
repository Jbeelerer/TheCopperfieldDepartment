using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace SaveSystem
{

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager instance;

        private string saveFile = "0";
        public SaveFileHandler saveFileHandler;

        List<ISavable> savables;

        public GameObject[] targets;
        string filePath;
        public SaveData saveData;

        public string GetSaveFile()
        {
            return this.saveFile;
        }
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                SetupSaveFile(saveFile);
                /*
               if (!Utility.CheckSaveFileExists(saveFile))
               {
                   SetupSaveFile(saveFile);
               }
               else
               {
                   instance.LoadGame();
               }*/
            }
            else
            {
                if (SceneManager.GetActiveScene().name != "00Start")
                {
                    /* instance.SetupSaveFile(instance.saveFile);
                     if(Utility.CheckSaveFileExists(instance.saveFile)){
                         print("loading.........");
                         instance.LoadGame();
                     } */
                }
                Destroy(gameObject);
            }
            DontDestroyOnLoad(instance);
        }

        void Start()
        {
        }

        /// <summary>
        /// This function will setup the file handler with the given filename
        /// </summary>
        /// <param name="saveFile"></param>
        /// <returns>true if successful</returns>
        public bool SetupSaveFile(string saveFile)
        {
            print("setup " + saveFile);
            instance.saveFile = saveFile;
            saveFileHandler = new SaveFileHandler(instance.saveFile);
            if (saveFileHandler == null) return false;
            else return true;
        }

        public void SaveGame()
        {
            if (saveData == null)
                saveData = new SaveData();

            savables = FindAllISavables();
            foreach (ISavable s in savables)
            {
                s.SaveData(saveData);
            }
            saveFileHandler.Save(saveData);

        }
        public void DeleteSave()
        {
            print("deleting");
            //print(GameManager.instance.GetSaveFile().ToString());
            // if(Utility.CheckSaveFileExists(GameManager.instance.saveFile.ToString())){
            saveFileHandler.DeleteSave();
            //}
        }

        public void LoadGame()
        {
            print("loading");
            // Grab the save data file 
            saveData = saveFileHandler.Load();

            // if the savaData file is null, stop 
            if (saveData == null) return;

            // Get all the iSavables (iSavables) 
            List<ISavable> iSavables = FindAllISavables();

            foreach (ISavable s in iSavables)
            {
                s.LoadData(saveData);
            }
        }

        public bool GetSaveExists()
        {
            return saveFileHandler.Load() != null;
        }

        List<ISavable> FindAllISavables()
        {
            IEnumerable<ISavable> savables = FindObjectsOfType<MonoBehaviour>().OfType<ISavable>();
            return new List<ISavable>(savables);
        }
    }
}
