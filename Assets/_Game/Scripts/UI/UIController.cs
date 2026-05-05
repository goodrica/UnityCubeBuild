using System.Collections.Generic;
using ChromaCube.Data;
using ChromaCube.Level;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ChromaCube.UI
{
    public class UIController : MonoBehaviour
    {
        private LevelManager levelManager;
        private GameObject titlePanel;
        private GameObject levelSelectPanel;
        private GameObject hudPanel;
        private GameObject completePanel;
        private Text titleText;
        private Text subtitleText;
        private Text hintText;
        private Text captureText;
        private Text completeText;

        private readonly List<Button> titleButtons = new List<Button>();
        private readonly List<Button> levelSelectButtons = new List<Button>();
        private readonly List<Button> currentButtons = new List<Button>();
        private readonly Dictionary<Button, Text> buttonLabels = new Dictionary<Button, Text>();
        private readonly Dictionary<Button, string> buttonBaseLabels = new Dictionary<Button, string>();
        private int selectedButtonIndex;

        public void Initialize(LevelManager manager)
        {
            levelManager = manager;
            levelManager.OnLevelLoaded += HandleLevelLoaded;
            levelManager.OnCaptureChanged += HandleCaptureChanged;
            levelManager.OnLevelCompleted += HandleLevelCompleted;
            BuildUi();
        }

        public void ShowTitle()
        {
            SetPanel(titlePanel);
            SetMenuButtons(titleButtons);
        }

        private void Update()
        {
            HandleMouseClick();

            if (currentButtons.Count == 0)
            {
                return;
            }

            if (Keyboard.current == null)
            {
                return;
            }

            var keyboard = Keyboard.current;

            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
            {
                SelectButton(selectedButtonIndex - 1);
            }
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                SelectButton(selectedButtonIndex + 1);
            }
            else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                currentButtons[selectedButtonIndex].onClick.Invoke();
            }
        }

        private void BuildUi()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.AddComponent<GraphicRaycaster>();

            titlePanel = CreatePanel("TitlePanel");
            titlePanel.GetComponent<Image>().color = new Color(0.035f, 0.055f, 0.07f, 0.98f);
            CreateTitleBackdrop(titlePanel.transform);
            CreateTitleText(titlePanel.transform, ColorfulTitle(), "CHROMA CUBE", 68, new Vector2(0.5f, 0.68f), new Vector2(900f, 96f));
            CreateText(titlePanel.transform, "A calm color-matching cube puzzle", 23, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.575f), new Vector2(760f, 46f), new Color(0.84f, 0.93f, 0.96f));
            titleButtons.Add(CreateButton(titlePanel.transform, "Start", new Vector2(0.5f, 0.43f), () => StartLevel(0), new Vector2(290f, 58f)));
            titleButtons.Add(CreateButton(titlePanel.transform, "Level Select", new Vector2(0.5f, 0.325f), ShowLevelSelect, new Vector2(290f, 58f)));

            levelSelectPanel = CreatePanel("LevelSelectPanel");
            CreateText(levelSelectPanel.transform, "Select Level", 42, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.84f), new Vector2(520f, 70f));
            for (var i = 0; i < levelManager.Levels.Count; i++)
            {
                var localIndex = i;
                var level = levelManager.Levels[i];
                var column = i < 5 ? 0 : 1;
                var row = column == 0 ? i : i - 5;
                var anchorX = column == 0 ? 0.36f : 0.64f;
                var anchorY = 0.68f - row * 0.115f;
                levelSelectButtons.Add(CreateButton(levelSelectPanel.transform, $"{level.index}. {level.title}", new Vector2(anchorX, anchorY), () => StartLevel(localIndex), new Vector2(260f, 52f)));
            }

            levelSelectButtons.Add(CreateButton(levelSelectPanel.transform, "Back", new Vector2(0.5f, 0.14f), ShowTitle, new Vector2(220f, 52f)));

            hudPanel = CreatePanel("HudPanel");
            hudPanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            titleText = CreateText(hudPanel.transform, string.Empty, 28, TextAnchor.UpperLeft, new Vector2(0.02f, 0.96f), new Vector2(520f, 50f));
            subtitleText = CreateText(hudPanel.transform, string.Empty, 17, TextAnchor.UpperLeft, new Vector2(0.02f, 0.91f), new Vector2(640f, 40f));
            hintText = CreateText(hudPanel.transform, string.Empty, 16, TextAnchor.LowerLeft, new Vector2(0.02f, 0.04f), new Vector2(860f, 52f));
            captureText = CreateText(hudPanel.transform, string.Empty, 22, TextAnchor.UpperRight, new Vector2(0.98f, 0.96f), new Vector2(320f, 48f));
            CreateButton(hudPanel.transform, "Restart", new Vector2(0.87f, 0.06f), () => levelManager.RestartCurrentLevel(), new Vector2(150f, 44f));
            CreateButton(hudPanel.transform, "Levels", new Vector2(0.97f, 0.06f), ShowLevelSelect, new Vector2(130f, 44f));

            completePanel = CreatePanel("CompletePanel");
            completePanel.GetComponent<Image>().color = new Color(0.02f, 0.03f, 0.04f, 0.82f);
            completeText = CreateText(completePanel.transform, "Level Complete", 42, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.58f), new Vector2(640f, 80f));
            CreateButton(completePanel.transform, "Next Level", new Vector2(0.5f, 0.44f), () => levelManager.LoadNextLevel());
            CreateButton(completePanel.transform, "Level Select", new Vector2(0.5f, 0.34f), ShowLevelSelect);
        }

        private void StartLevel(int index)
        {
            currentButtons.Clear();
            ResetButtonLabels();
            SetPanel(hudPanel);
            levelManager.LoadLevel(index);
        }

        private void ShowLevelSelect()
        {
            SetPanel(levelSelectPanel);
            SetMenuButtons(levelSelectButtons);
        }

        private void HandleLevelLoaded(LevelData level, int captured, int required)
        {
            titleText.text = $"Level {level.index}: {level.title}";
            subtitleText.text = level.subtitle;
            hintText.text = $"Hint: {level.hint}";
            HandleCaptureChanged(captured, required);
            SetPanel(hudPanel);
        }

        private void HandleCaptureChanged(int captured, int required)
        {
            captureText.text = $"{captured}/{required}";
        }

        private void HandleLevelCompleted(LevelData level)
        {
            completeText.text = $"Level {level.index} Complete";
            SetPanel(completePanel);
        }

        private void SetPanel(GameObject active)
        {
            titlePanel.SetActive(active == titlePanel);
            levelSelectPanel.SetActive(active == levelSelectPanel);
            hudPanel.SetActive(active == hudPanel);
            completePanel.SetActive(active == completePanel);
        }

        private void SetMenuButtons(List<Button> buttons)
        {
            currentButtons.Clear();
            currentButtons.AddRange(buttons);
            SelectButton(0);
        }

        private void SelectButton(int index)
        {
            if (currentButtons.Count == 0)
            {
                return;
            }

            selectedButtonIndex = (index + currentButtons.Count) % currentButtons.Count;
            ResetButtonLabels();

            var selectedButton = currentButtons[selectedButtonIndex];
            if (buttonLabels.TryGetValue(selectedButton, out var selectedLabel))
            {
                selectedLabel.text = "> " + buttonBaseLabels[selectedButton];
            }

        }

        private void ResetButtonLabels()
        {
            foreach (var pair in buttonLabels)
            {
                pair.Value.text = "  " + buttonBaseLabels[pair.Key];
            }
        }

        private void HandleMouseClick()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            var screenPosition = Mouse.current.position.ReadValue();
            foreach (var pair in buttonLabels)
            {
                var button = pair.Key;
                if (!button.gameObject.activeInHierarchy)
                {
                    continue;
                }

                var rectTransform = button.GetComponent<RectTransform>();
                if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition))
                {
                    button.onClick.Invoke();
                    return;
                }
            }
        }

        private GameObject CreatePanel(string panelName)
        {
            var panel = new GameObject(panelName, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.055f, 0.065f, 0.08f, 0.94f);
            return panel;
        }

        private Text CreateText(Transform parent, string content, int size, TextAnchor anchor, Vector2 anchorPosition, Vector2 dimensions)
        {
            return CreateText(parent, content, size, anchor, anchorPosition, dimensions, Color.white);
        }

        private Text CreateText(Transform parent, string content, int size, TextAnchor anchor, Vector2 anchorPosition, Vector2 dimensions, Color color)
        {
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.sizeDelta = dimensions;
            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition;
            rect.anchoredPosition = Vector2.zero;

            var text = textObject.GetComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private void CreateTitleText(Transform parent, string coloredContent, string shadowContent, int size, Vector2 anchorPosition, Vector2 dimensions)
        {
            var shadow = CreateText(parent, shadowContent, size, TextAnchor.MiddleCenter, anchorPosition, dimensions, new Color(0.05f, 0.08f, 0.10f, 0.9f));
            shadow.GetComponent<RectTransform>().anchoredPosition = new Vector2(4f, -5f);

            var title = CreateText(parent, coloredContent, size, TextAnchor.MiddleCenter, anchorPosition, dimensions, Color.white);
            title.fontStyle = FontStyle.Bold;
            title.supportRichText = true;
        }

        private static string ColorfulTitle()
        {
            return "<color=#33F59E>C</color><color=#61D7FF>H</color><color=#FFD12E>R</color><color=#C27AFF>O</color><color=#FF5C6B>M</color><color=#33F59E>A</color> " +
                   "<color=#61D7FF>C</color><color=#FFD12E>U</color><color=#C27AFF>B</color><color=#FF5C6B>E</color>";
        }

        private void CreateTitleBackdrop(Transform parent)
        {
            CreateAccentBlock(parent, "MintGlow", new Vector2(0.17f, 0.70f), new Vector2(170f, 170f), 18f, new Color(0.20f, 0.96f, 0.62f, 0.34f), 7f);
            CreateAccentBlock(parent, "AmberGlow", new Vector2(0.82f, 0.65f), new Vector2(210f, 130f), -16f, new Color(1.00f, 0.82f, 0.18f, 0.30f), -5f);
            CreateAccentBlock(parent, "LavenderGlow", new Vector2(0.73f, 0.24f), new Vector2(150f, 150f), 32f, new Color(0.76f, 0.48f, 1.00f, 0.26f), 9f);
            CreateAccentBlock(parent, "OceanGlow", new Vector2(0.26f, 0.28f), new Vector2(230f, 82f), -20f, new Color(0.12f, 0.58f, 1.00f, 0.22f), -8f);
            CreateAccentLine(parent, new Vector2(0.5f, 0.515f), new Vector2(520f, 3f), new Color(0.66f, 0.94f, 1.00f, 0.72f));
        }

        private void CreateAccentBlock(Transform parent, string name, Vector2 anchorPosition, Vector2 dimensions, float rotation, Color color, float rotationSpeed)
        {
            var block = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(UIAmbientAnimator));
            block.transform.SetParent(parent, false);
            var rect = block.GetComponent<RectTransform>();
            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition;
            rect.sizeDelta = dimensions;
            rect.anchoredPosition = Vector2.zero;
            rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
            block.GetComponent<Image>().color = color;
            block.GetComponent<UIAmbientAnimator>().Configure(rotationSpeed);
        }

        private void CreateAccentLine(Transform parent, Vector2 anchorPosition, Vector2 dimensions, Color color)
        {
            var line = new GameObject("TitleAccentLine", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(parent, false);
            var rect = line.GetComponent<RectTransform>();
            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition;
            rect.sizeDelta = dimensions;
            rect.anchoredPosition = Vector2.zero;
            line.GetComponent<Image>().color = color;
        }

        private Button CreateButton(Transform parent, string label, Vector2 anchorPosition, UnityEngine.Events.UnityAction action)
        {
            return CreateButton(parent, label, anchorPosition, action, new Vector2(230f, 54f));
        }

        private Button CreateButton(Transform parent, string label, Vector2 anchorPosition, UnityEngine.Events.UnityAction action, Vector2 dimensions)
        {
            var buttonObject = new GameObject($"Button_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = dimensions;
            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition;
            rect.anchoredPosition = Vector2.zero;

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.12f, 0.19f, 0.25f, 0.96f);
            var button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);
            var colors = button.colors;
            colors.normalColor = new Color(0.12f, 0.19f, 0.25f, 0.96f);
            colors.highlightedColor = new Color(0.18f, 0.36f, 0.44f, 1f);
            colors.pressedColor = new Color(0.08f, 0.78f, 0.96f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            var labelText = CreateText(buttonObject.transform, label, 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), dimensions);
            labelText.fontStyle = FontStyle.Bold;
            labelText.color = Color.white;
            buttonLabels[button] = labelText;
            buttonBaseLabels[button] = label;
            return button;
        }
    }
}
