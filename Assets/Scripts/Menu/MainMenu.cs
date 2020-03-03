using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject canvas;

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void LoadOrbitScene()
    {
        SceneManager.LoadScene("Orbit", LoadSceneMode.Single);
        canvas.SetActive(false);
    }

    public void LoadRopesScene()
    {
        SceneManager.LoadScene("Ropes", LoadSceneMode.Single);
        canvas.SetActive(false);
    }

    public void QuitApp()
    {
        Application.Quit();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)
            && SceneManager.GetActiveScene().name != "MainMenu")
        {
            canvas.SetActive(!canvas.activeSelf);
        }
    }
}