using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem
{
    [System.Serializable]
    public class SaveData
    {
        List<CompetingEmployee> competingEmployees = null;
        CompetingEmployee playerAsCompetingEmployee = null;
        public List<bool> completedDays = new List<bool>();
        public List<investigationStates> firstTryResult = new List<investigationStates>();
        public List<int> pointsPerDay = new List<int>();
        public int points;
        public int currentDay = 0;




        public SaveData()
        {

        }
    }
}


