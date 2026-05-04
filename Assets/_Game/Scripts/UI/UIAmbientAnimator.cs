using UnityEngine;
using UnityEngine.UI;

namespace ChromaCube.UI
{
    public class UIAmbientAnimator : MonoBehaviour
    {
        [SerializeField] private float bobAmount = 10f;
        [SerializeField] private float bobSpeed = 0.6f;
        [SerializeField] private float rotationSpeed = 8f;
        [SerializeField] private float alphaPulse = 0.08f;

        private RectTransform rectTransform;
        private Image image;
        private Vector2 startPosition;
        private Color startColor;
        private float phase;

        public void Configure(float newRotationSpeed)
        {
            rotationSpeed = newRotationSpeed;
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
            startPosition = rectTransform.anchoredPosition;
            startColor = image.color;
            phase = Random.Range(0f, 10f);
        }

        private void Update()
        {
            var wave = Mathf.Sin(Time.unscaledTime * bobSpeed + phase);
            rectTransform.anchoredPosition = startPosition + Vector2.up * wave * bobAmount;
            rectTransform.Rotate(Vector3.forward, rotationSpeed * Time.unscaledDeltaTime);

            var color = startColor;
            color.a = Mathf.Clamp01(startColor.a + wave * alphaPulse);
            image.color = color;
        }
    }
}
