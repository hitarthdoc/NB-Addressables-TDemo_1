using System.Collections;
using System.Collections.Generic;

using UnityEditor.SearchService;

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField]
    public int scene;

    [ContextMenu("Change Scene")]
    protected void ChangeScene ()
    {
        SceneManager.LoadSceneAsync (scene);
    }
}
