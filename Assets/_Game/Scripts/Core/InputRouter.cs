using ChromaCube.Level;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ChromaCube.Core
{
    public class InputRouter : MonoBehaviour
    {
        private LevelManager levelManager;
        private UnityEngine.Camera inputCamera;

        public void Configure(LevelManager manager)
        {
            levelManager = manager;
            inputCamera = UnityEngine.Camera.main;
        }

        private void Update()
        {
            if (levelManager == null || !levelManager.AcceptsInput || Keyboard.current == null)
            {
                return;
            }

            var keyboard = Keyboard.current;
            var intent = Vector2.zero;

            if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)
            {
                intent = Vector2.up;
            }
            else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            {
                intent = Vector2.down;
            }
            else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                intent = Vector2.right;
            }
            else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                intent = Vector2.left;
            }

            if (intent == Vector2.zero)
            {
                return;
            }

            if (inputCamera == null)
            {
                inputCamera = UnityEngine.Camera.main;
            }

            levelManager.TryMoveFromScreenIntent(intent, inputCamera);
        }
    }
}
