using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CalendarDay : MonoBehaviour
{
    [SerializeField] private string sceneName;
    private GameManager gm;

    private void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }
    public void LoadDay()
    {
        gm.LoadNewDay(int.Parse(gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text));
        SceneManager.LoadScene(sceneName);
    }
}
