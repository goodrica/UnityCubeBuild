using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChromaCube.Core
{
    public class BootstrapSceneLoader : MonoBehaviour
    {
        public string gameSceneName = "Game";

        private void Start()
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
