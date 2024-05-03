using System.IO;
using UnityEngine;

namespace SaveSystem
{
    public class SaveFileHandler
    {
        string filePath;
        string fileName;

        public SaveFileHandler(string fileName)
        {
            this.fileName = fileName;
            filePath = Path.Combine(Application.persistentDataPath, this.fileName);
            //filePath = Application.persistentDataPath + Path.DirectorySeparatorChar + this.fileName; // Alt method (probably not as good)
        }

        public void DeleteSave()
        {
            if (File.Exists(filePath))
            {
                Debug.Log("deleting save" + filePath);
                File.Delete(filePath);
            }
        }
        public SaveData Load()
        {
            if (File.Exists(filePath))
            {
                // read the object data from file
                string retrievedData = File.ReadAllText(filePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(retrievedData);
                Debug.Log("Save Data Loaded from: " + filePath);
                return saveData;
            }
            else
            {
                Debug.Log("No save file named '" + filePath + "' found");
                return null;
            }
        }

        public void Save(SaveData saveData)
        {
            // Save your saveData object to a json file.
            string data = JsonUtility.ToJson(saveData, true); // 'true' makes it look pretty
            File.WriteAllText(filePath, data);
            Debug.Log("Game Saved to: " + filePath);
        }
    }
}

