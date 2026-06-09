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
            if (levelManager == null || !levelManager.AcceptsInput)
            {
                return;
            }

            var intent = Vector2.zero;

            // Keyboard input
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
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
            }

            // Gamepad input (only if no keyboard intent)
            if (intent == Vector2.zero)
            {
                var gamepad = Gamepad.current;
                if (gamepad != null)
                {
                    if (gamepad.dpad.up.wasPressedThisFrame)
                    {
                        intent = Vector2.up;
                    }
                    else if (gamepad.dpad.down.wasPressedThisFrame)
                    {
                        intent = Vector2.down;
                    }
                    else if (gamepad.dpad.right.wasPressedThisFrame)
                    {
                        intent = Vector2.right;
                    }
                    else if (gamepad.dpad.left.wasPressedThisFrame)
                    {
                        intent = Vector2.left;
                    }
                    else
                    {
                        // Left stick fallback
                        var stick = gamepad.leftStick.ReadValue();
                        if (stick.magnitude > 0.5f)
                        {
                            if (Mathf.Abs(stick.x) > Mathf.Abs(stick.y))
                                intent = stick.x > 0 ? Vector2.right : Vector2.left;
                            else
                                intent = stick.y > 0 ? Vector2.up : Vector2.down;
                        }
                    }
                }
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