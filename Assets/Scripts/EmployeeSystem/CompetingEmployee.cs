using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompetingEmployee : MonoBehaviour
{
    private string employeeName;
    private int points;
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

    public string GetEmployeeName()
    {
        return employeeName;
    }
    public int GetPoints()
    {
        return points;
    }

    public string GetListString()
    {
        return employeeName + ": " + points;
    }
    public CompetingEmployee()
    {
        this.generateName();
        this.points = 0;
        this.skill = Random.Range(-50, 50);
        addNewPointsRandomly(50, 150);
    }
    public CompetingEmployee(string name, int points, int skill)
    {
        this.employeeName = name;
        this.points = points;
        this.skill = skill;
    }

    public void addNewPoints(int newPoints)
    {
        points += newPoints;
    }
    public void addNewPointsRandomly(int min = -100, int max = 100)
    {
        int newPoints = Random.Range(min, max + skill);
        points += newPoints;
        // level up employee if points increased
        if (newPoints > 0)
        {
            skill += Random.Range(1, 5);
        }
    }
}
