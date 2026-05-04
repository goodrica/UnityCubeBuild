using System.Collections.Generic;
using ChromaCube.Audio;
using ChromaCube.Cameras;
using ChromaCube.Data;
using ChromaCube.Level;
using ChromaCube.Movement;
using ChromaCube.Rendering;
using ChromaCube.UI;
using UnityEngine;

namespace ChromaCube.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private InputRouter inputRouter;
        [SerializeField] private UIController uiController;
        [SerializeField] private BoardRenderer boardRenderer;
        [SerializeField] private CubeRenderer cubeRenderer;
        [SerializeField] private ClassicMovementController movementController;
        [SerializeField] private CameraRigController cameraRig;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private List<LevelData> levels = new List<LevelData>();

        private void Awake()
        {
            EnsureReferences();
            levelManager.Initialize(boardRenderer, cubeRenderer, movementController, cameraRig, levels);
            inputRouter.Configure(levelManager);
            uiController.Initialize(levelManager);
            audioManager.Initialize(levelManager);
        }

        private void Start()
        {
            uiController.ShowTitle();
        }

        public void SetLevels(List<LevelData> availableLevels)
        {
            levels = availableLevels;
        }

        private void EnsureReferences()
        {
            if (levelManager == null)
            {
                levelManager = GetComponentInChildren<LevelManager>();
            }

            if (inputRouter == null)
            {
                inputRouter = GetComponentInChildren<InputRouter>();
            }

            if (uiController == null)
            {
                uiController = GetComponentInChildren<UIController>();
            }

            if (boardRenderer == null)
            {
                boardRenderer = GetComponentInChildren<BoardRenderer>();
            }

            if (cubeRenderer == null)
            {
                cubeRenderer = GetComponentInChildren<CubeRenderer>();
            }

            if (movementController == null && cubeRenderer != null)
            {
                movementController = cubeRenderer.GetComponent<ClassicMovementController>();
            }

            if (cameraRig == null)
            {
                cameraRig = GetComponentInChildren<CameraRigController>();
            }

            if (audioManager == null)
            {
                audioManager = GetComponentInChildren<AudioManager>();
            }

            if (audioManager == null)
            {
                audioManager = gameObject.AddComponent<AudioManager>();
            }
        }
    }
}
