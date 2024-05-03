namespace SaveSystem {
    public interface ISavable
    {
        void LoadData(SaveData data);
        void SaveData(SaveData data);
    }
}
