using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem
{
    [System.Serializable]
    public class SaveData
    {
        /*
          List<CompetingEmployee> competingEmployees = null;
          CompetingEmployee playerAsCompetingEmployee = null;
          public List<bool> completedDays = new List<bool>();
          public List<investigationStates> firstTryResult = new List<investigationStates>();
          public List<int> pointsPerDay = new List<int>();
          */
        // public int points;    
        public int currentDay = 0;
        public List<investigationStates> firstTryResult = new List<investigationStates>();
        public List<investigationStates> result = new List<investigationStates>();

        public List<int> pointsPerDay = new List<int>();
        public List<SaveableEmployee> competingEmployees = new List<SaveableEmployee>();

        public float mouseSensitivity;

        public SaveData()
        {

        }
    }
    [System.Serializable]
    public class SaveableEmployee
    {
        public string name;
        public bool isPlayer;
        public int basePoints;
        public int skill;
        public List<int> pointsPerDay = new List<int>();
        public SaveableEmployee(string name, int basePoints, int skill, List<int> pointsPerDay)
        {
            this.name = name;
            this.basePoints = basePoints;
            this.pointsPerDay = pointsPerDay;
            this.skill = skill;
        }
    }
}


