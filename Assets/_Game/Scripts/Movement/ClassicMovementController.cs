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

        private Func<Direction, bool> canMove;
        private Func<Direction, Vector3> getTargetWorldPosition;
        private Action<Direction> commitMove;
        private bool isMoving;

        public bool IsMoving => isMoving;

        public void Configure(Func<Direction, bool> canMoveDelegate, Func<Direction, Vector3> targetDelegate, Action<Direction> commitDelegate)
        {
            canMove = canMoveDelegate;
            getTargetWorldPosition = targetDelegate;
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
            var axis = GetRotationAxis(direction);
            var elapsed = 0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / moveDuration);
                var eased = Mathf.SmoothStep(0f, 1f, t);
                transform.position = Vector3.Lerp(startPosition, targetPosition, eased);
                transform.rotation = Quaternion.AngleAxis(90f * eased, axis) * startRotation;
                yield return null;
            }

            transform.position = targetPosition;
            transform.rotation = Quaternion.AngleAxis(90f, axis) * startRotation;
            commitMove(direction);
            OnMoveCompleted?.Invoke(direction);
            isMoving = false;
        }

        private static Vector3 GetRotationAxis(Direction direction)
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
