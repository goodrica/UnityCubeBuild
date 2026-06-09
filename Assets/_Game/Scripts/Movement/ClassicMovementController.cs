using System;
using System.Collections;
using ChromaCube.Core;
using UnityEngine;

namespace ChromaCube.Movement
{
    public class ClassicMovementController : MonoBehaviour
    {
        public event Action<Direction> OnMoveStarted;
        public event Action<Direction> OnMoveCompleted;

        [SerializeField] private float moveDuration = 0.22f;

        private void OnValidate()
        {
            // Prevent divide-by-zero in RollRoutine if moveDuration is set to 0 or negative in the Inspector
            moveDuration = Mathf.Max(0.01f, moveDuration);
        }

        private void OnDisable()
        {
            // If the GameObject is disabled mid-roll (e.g. level restart), stop all coroutines
            // and release the isMoving lock so future moves aren't permanently blocked.
            StopAllCoroutines();
            isMoving = false;
        }

        private Func<Direction, bool> canMove;
        private Func<Direction, Vector3> getTargetWorldPosition;
        private Func<Direction, Vector3> getRotationAxis;
        private Action<Direction> commitMove;
        private bool isMoving;

        public bool IsMoving => isMoving;

        public void Configure(Func<Direction, bool> canMoveDelegate, Func<Direction, Vector3> targetDelegate, Action<Direction> commitDelegate)
        {
            canMove = canMoveDelegate;
            getTargetWorldPosition = targetDelegate;
            commitMove = commitDelegate;
            getRotationAxis = GetClassicRotationAxis;
        }

        public void Configure(Func<Direction, bool> canMoveDelegate, Func<Direction, Vector3> targetDelegate, Func<Direction, Vector3> axisDelegate, Action<Direction> commitDelegate)
        {
            canMove = canMoveDelegate;
            getTargetWorldPosition = targetDelegate;
            getRotationAxis = axisDelegate;
            commitMove = commitDelegate;
        }

        public bool TryMove(Direction direction)
        {
            if (isMoving || canMove == null || getTargetWorldPosition == null || commitMove == null)
            {
                return false;
            }

            if (!canMove(direction))
            {
                return false;
            }

            StartCoroutine(RollRoutine(direction));
            return true;
        }

        private IEnumerator RollRoutine(Direction direction)
        {
            isMoving = true;
            OnMoveStarted?.Invoke(direction);

            var startPosition = transform.position;
            var targetPosition = getTargetWorldPosition(direction);
            var startRotation = transform.rotation;
            var axis = getRotationAxis != null ? getRotationAxis(direction) : GetClassicRotationAxis(direction);
            var elapsed = 0f;

            while (elapsed < moveDuration)
            {
                // Guard: if this object is destroyed or deactivated mid-roll, bail out cleanly.
                // isMoving is already reset by OnDisable, so no cleanup needed here.
                if (this == null || !gameObject.activeInHierarchy) yield break;

                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / moveDuration);
                var eased = Mathf.SmoothStep(0f, 1f, t);
                transform.position = Vector3.Lerp(startPosition, targetPosition, eased);
                transform.rotation = Quaternion.AngleAxis(90f * eased, axis) * startRotation;
                yield return null;
            }

            transform.position = targetPosition;
            transform.rotation = Quaternion.AngleAxis(90f, axis) * startRotation;

            // Release the lock BEFORE commitMove so that any level-load triggered inside
            // commitMove (which may call Configure/TryMove) doesn't see isMoving=true
            // and silently drop the first input of the new level.
            isMoving = false;
            commitMove(direction);
            OnMoveCompleted?.Invoke(direction);
        }

        private static Vector3 GetClassicRotationAxis(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return Vector3.right;
                case Direction.South:
                    return Vector3.left;
                case Direction.East:
                    return Vector3.back;
                case Direction.West:
                    return Vector3.forward;
                default:
                    return Vector3.right;
            }
        }
    }
}
