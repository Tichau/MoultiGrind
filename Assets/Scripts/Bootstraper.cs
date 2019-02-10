using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstraper : MonoBehaviour
{
    public string GameScene;

    private bool headlessMode = false;

    private void Start()
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            if (!argument.StartsWith("--"))
            {
                continue;
            }

            switch (argument.Substring(2))
            {
                case "headless-server":
                    this.headlessMode = true;
                    break;
            }
        }
        
        if (this.headlessMode)
        {
            this.StartHeadlessServer();
        }
        else
        {
            this.StartGame();
        }
    }
    
    private void StartHeadlessServer()
    {
        Debug.LogWarning($"Not implemented.");
    }

    private void StartGame()
    {
        SceneManager.LoadScene(this.GameScene, LoadSceneMode.Single);
    }
}
