using UnityEngine;

namespace SaveSystem
{
    public class SavePrefs : MonoBehaviour
    {

        public static SavePrefs instance = null;
        GameManager gm;

        // Awake 
        void Awake()
        {
            // Singleton pattern here!
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != null)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(this); // This game object will pass from scene to scene
            Init();
        }

        void Init() { }

        void Start()
        {
            gm = GameManager.instance;
            LoadData();
        }


        public void LoadData()
        {
            //gm.cubeColor = (CubeColor)PlayerPrefs.GetInt("CubeColor", 0);
            Debug.Log("Data Loaded");
        }

        public void SaveData()
        {
            // PlayerPrefs.SetInt("CubeColor", (int)gm.cubeColor);
            Debug.Log("Data Saved");
        }
    }
}

