using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public Button toRegisterButton;
    public Button toLoginButton;

    void Start()
    {
        toRegisterButton.onClick.AddListener(() => SceneManager.LoadScene("register"));
        toLoginButton.onClick.AddListener(() => SceneManager.LoadScene("login"));
    }
}