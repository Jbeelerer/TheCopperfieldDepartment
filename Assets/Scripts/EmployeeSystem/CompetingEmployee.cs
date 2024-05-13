using System.Collections;
using System.Collections.Generic;
using SaveSystem;
using UnityEngine;

public class CompetingEmployee : ISavable
{
    private string employeeName;
    private int basePoints;
    private List<int> pointsPerDay = new List<int>();
    private int skill;
    private Sprite img;
    private string[] sillables1 = { "Jo", "Da", "Pe", "Ke", "Ma" };
    private string[] sillables2 = { "vi", "ni", "na", "ri", "ti" };
    private string[] sillables3 = { "d", "u", "a", "o" };
    private string[] lastName = { "Miller", "Meyer", "Parker", "Sink", "Douglas", "Thornefield", "Ravenscroft", "Holloway", "Whitlock", "Stirling", "Everhart", "Lockwood", "Fairbourne", "Lopez", "Jenkins", "Keller", "Pierce", "Wilson", "Hart", "Finch" };
    private void generateName()
    {
        employeeName = sillables1[Random.Range(0, sillables1.Length)] + sillables2[Random.Range(0, sillables2.Length)] + sillables3[Random.Range(0, sillables3.Length)] + " " + lastName[Random.Range(0, lastName.Length)];
    }

    public int GetSkill()
    {
        return skill;
    }
    public int GetTotalPoints(int day)
    {
        int totalPoints = basePoints;
        int j = 0;
        foreach (int i in pointsPerDay)
        {
            j++;
            //only get points until the current day
            if (j >= day)
            {
                break;
            }
            totalPoints += i;
        }
        return totalPoints;
    }
    public List<int> GetPointsPerDay()
    {
        return pointsPerDay;
    }
    public void SetPointsPerDay(List<int> pointsPerDay)
    {
        this.pointsPerDay = pointsPerDay;
    }

    public string GetEmployeeName()
    {
        return employeeName;
    }
    public int GetPoints()
    {
        return basePoints;
    }

    public string GetListString(int day)
    {
        return employeeName + ": " + GetTotalPoints(day);
    }
    public CompetingEmployee()
    {
        this.generateName();
        this.basePoints = Random.Range(0, 100);
        this.skill = Random.Range(-50, 50);
    }
    public CompetingEmployee(string name, int points, int skill)
    {
        this.employeeName = name;
        this.basePoints = points;
        this.skill = skill;
    }

    public void addNewPoints(int newPoints, int day)
    {
        if (pointsPerDay.Count <= day)
        {
            pointsPerDay.Add(newPoints);
            // level up employee if points increased, but only the first time
            if (newPoints > 0)
            {
                skill += Random.Range(1, 5);
            }
        }
        else
        {
            pointsPerDay[day] = newPoints;
        }
        //points += newPoints;
    }
    public void addNewPointsRandomly(int day, int min = -100, int max = 100)
    {
        int newPoints = Random.Range(min, max + skill);
        this.addNewPoints(newPoints, day);
    }

    public void LoadData(SaveData data)
    {
        pointsPerDay = data.pointsPerDay;
    }

    public void SaveData(SaveData data)
    {
        data.pointsPerDay = pointsPerDay;
    }
}
