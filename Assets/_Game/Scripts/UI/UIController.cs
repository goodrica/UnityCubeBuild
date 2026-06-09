using System.Collections;
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

        // ── Chroma palette ──────────────────────────────────────────────────────
        private static readonly Color CMint    = new Color(0.20f, 0.96f, 0.62f, 1f);
        private static readonly Color CBlue    = new Color(0.38f, 0.85f, 1.00f, 1f);
        private static readonly Color CAmber   = new Color(1.00f, 0.82f, 0.18f, 1f);
        private static readonly Color CPurple  = new Color(0.76f, 0.48f, 1.00f, 1f);
        private static readonly Color CRed     = new Color(1.00f, 0.36f, 0.42f, 1f);
        private static readonly Color CDark    = new Color(0.04f, 0.06f, 0.09f, 1f);
        private static readonly Color CDarker  = new Color(0.02f, 0.03f, 0.05f, 1f);
        private static readonly Color CPanel   = new Color(0.07f, 0.10f, 0.14f, 0.97f);

        public void Initialize(LevelManager manager)
        {
            levelManager = manager;
            levelManager.OnLevelLoaded    += HandleLevelLoaded;
            levelManager.OnCaptureChanged += HandleCaptureChanged;
            levelManager.OnLevelCompleted += HandleLevelCompleted;
            BuildUi();
        }

        public void ShowTitle()
        {
            SetPanel(titlePanel);
            SetMenuButtons(titleButtons);
        }

        // ── Input (keyboard + gamepad menu navigation) ───────────────────────
        private void Update()
        {
            HandleMouseClick();

            if (currentButtons.Count == 0) return;

            var kb  = Keyboard.current;
            var gp  = Gamepad.current;

            bool navUp   = (kb != null && (kb.upArrowKey.wasPressedThisFrame   || kb.wKey.wasPressedThisFrame))
                        || (gp != null && (gp.dpad.up.wasPressedThisFrame      || gp.leftStick.up.wasPressedThisFrame));
            bool navDown = (kb != null && (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame))
                        || (gp != null && (gp.dpad.down.wasPressedThisFrame    || gp.leftStick.down.wasPressedThisFrame));
            bool confirm = (kb != null && (kb.enterKey.wasPressedThisFrame     || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
                        || (gp != null && (gp.buttonSouth.wasPressedThisFrame));

            if      (navUp)   SelectButton(selectedButtonIndex - 1);
            else if (navDown) SelectButton(selectedButtonIndex + 1);
            else if (confirm) currentButtons[selectedButtonIndex].onClick.Invoke();
        }

        // ── Build all panels ─────────────────────────────────────────────────
        private void BuildUi()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode             = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution     = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight      = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            BuildTitlePanel();
            BuildLevelSelectPanel();
            BuildHudPanel();
            BuildCompletePanel();
        }

        // ════════════════════════════════════════════════════════════════════
        //  TITLE PANEL – modern redesign
        // ════════════════════════════════════════════════════════════════════
        private void BuildTitlePanel()
        {
            titlePanel = CreateFullPanel("TitlePanel", CDarker);

            // ── deep gradient overlay (two stacked rects) ───────────────────
            CreateRect(titlePanel.transform, "GradTop",
                new Vector2(0f, 0.5f), new Vector2(1f, 1f),
                new Color(0.05f, 0.08f, 0.18f, 0.70f));
            CreateRect(titlePanel.transform, "GradBottom",
                new Vector2(0f, 0f), new Vector2(1f, 0.5f),
                new Color(0.02f, 0.03f, 0.07f, 0.85f));

            // ── large blurred orbs (ambient glow) ───────────────────────────
            CreateOrb(titlePanel.transform, "OrbMint",   new Vector2(0.18f, 0.72f), 420f, new Color(CMint.r,   CMint.g,   CMint.b,   0.18f));
            CreateOrb(titlePanel.transform, "OrbBlue",   new Vector2(0.80f, 0.60f), 500f, new Color(CBlue.r,   CBlue.g,   CBlue.b,   0.14f));
            CreateOrb(titlePanel.transform, "OrbPurple", new Vector2(0.65f, 0.22f), 360f, new Color(CPurple.r, CPurple.g, CPurple.b, 0.16f));
            CreateOrb(titlePanel.transform, "OrbAmber",  new Vector2(0.30f, 0.20f), 300f, new Color(CAmber.r,  CAmber.g,  CAmber.b,  0.12f));

            // ── top accent bar (full-width neon gradient line) ───────────────
            CreateHorizBar(titlePanel.transform, "TopBar",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Color(CBlue.r, CBlue.g, CBlue.b, 0.85f), 3f);

            // ── bottom accent bar ────────────────────────────────────────────
            CreateHorizBar(titlePanel.transform, "BottomBar",
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Color(CMint.r, CMint.g, CMint.b, 0.60f), 2f);

            // ── rotating cube-face accent shapes ────────────────────────────
            CreateSpinBox(titlePanel.transform, "SpinMint",   new Vector2(0.12f, 0.78f), 90f,  90f,  new Color(CMint.r,   CMint.g,   CMint.b,   0.22f),  6f);
            CreateSpinBox(titlePanel.transform, "SpinAmber",  new Vector2(0.88f, 0.72f), 110f, 70f,  new Color(CAmber.r,  CAmber.g,  CAmber.b,  0.20f), -8f);
            CreateSpinBox(titlePanel.transform, "SpinPurple", new Vector2(0.78f, 0.18f), 80f,  80f,  new Color(CPurple.r, CPurple.g, CPurple.b, 0.20f), 12f);
            CreateSpinBox(titlePanel.transform, "SpinBlue",   new Vector2(0.22f, 0.22f), 120f, 50f,  new Color(CBlue.r,   CBlue.g,   CBlue.b,   0.18f), -5f);

            // ── logo card (frosted glass backing behind title text) ──────────
            var logoCard = CreateRect(titlePanel.transform, "LogoCard",
                new Vector2(0.15f, 0.58f), new Vector2(0.85f, 0.78f),
                new Color(0.06f, 0.09f, 0.13f, 0.72f));
            // thin coloured left-edge strip on logo card
            CreateVertStrip(logoCard.transform, "LogoStrip", new Color(CMint.r, CMint.g, CMint.b, 0.90f));

            // ── main title ───────────────────────────────────────────────────
            CreateTitleText(titlePanel.transform, ColorfulTitle(), "CHROMA CUBE",
                76, new Vector2(0.5f, 0.675f), new Vector2(1000f, 110f));

            // ── tagline ──────────────────────────────────────────────────────
            var tagline = CreateText(titlePanel.transform,
                "A COLOR-MATCHING CUBE PUZZLE",
                18, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.565f), new Vector2(900f, 34f),
                new Color(CBlue.r, CBlue.g, CBlue.b, 0.85f));
            tagline.fontStyle = FontStyle.Normal;

            // ── thin separator under tagline ────────────────────────────────
            CreateHorizBar(titlePanel.transform, "TitleSep",
                new Vector2(0.3f, 0.528f), new Vector2(0.7f, 0.528f),
                new Color(CBlue.r, CBlue.g, CBlue.b, 0.45f), 1f);

            // ── buttons (pill style) ─────────────────────────────────────────
            titleButtons.Add(CreatePillButton(titlePanel.transform,
                "PLAY",        new Vector2(0.5f, 0.415f), () => StartLevel(0),    new Vector2(320f, 62f), CMint));
            titleButtons.Add(CreatePillButton(titlePanel.transform,
                "LEVEL SELECT", new Vector2(0.5f, 0.315f), ShowLevelSelect,       new Vector2(320f, 62f), CBlue));

            // ── controller hint (bottom center) ─────────────────────────────
            CreateText(titlePanel.transform,
                "Keyboard / Controller supported  |  WASD · Arrow Keys · D-Pad · Stick",
                14, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.055f), new Vector2(1100f, 28f),
                new Color(1f, 1f, 1f, 0.28f));

            // ── version stamp ────────────────────────────────────────────────
            CreateText(titlePanel.transform,
                "v1.3",
                13, TextAnchor.MiddleRight,
                new Vector2(0.985f, 0.04f), new Vector2(120f, 24f),
                new Color(1f, 1f, 1f, 0.20f));
        }

        // ════════════════════════════════════════════════════════════════════
        //  LEVEL SELECT PANEL
        // ════════════════════════════════════════════════════════════════════
        private void BuildLevelSelectPanel()
        {
            levelSelectPanel = CreateFullPanel("LevelSelectPanel", CDarker);
            CreateRect(levelSelectPanel.transform, "BgGrad",
                new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Color(0.03f, 0.05f, 0.09f, 0.80f));

            CreateHorizBar(levelSelectPanel.transform, "TopBar",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Color(CBlue.r, CBlue.g, CBlue.b, 0.70f), 3f);

            // header
            CreateText(levelSelectPanel.transform, "SELECT LEVEL",
                38, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.88f), new Vector2(600f, 64f),
                CBlue);
            CreateHorizBar(levelSelectPanel.transform, "HeadSep",
                new Vector2(0.25f, 0.824f), new Vector2(0.75f, 0.824f),
                new Color(CBlue.r, CBlue.g, CBlue.b, 0.40f), 1f);

            const int maxRowsPerColumn = 6;
            var columnCount = Mathf.Max(1, Mathf.CeilToInt(levelManager.Levels.Count / (float)maxRowsPerColumn));
            for (var i = 0; i < levelManager.Levels.Count; i++)
            {
                var localIndex = i;
                var level      = levelManager.Levels[i];
                var column     = i / maxRowsPerColumn;
                var row        = i % maxRowsPerColumn;
                var anchorX    = columnCount == 1 ? 0.5f : Mathf.Lerp(0.22f, 0.78f, column / (float)(columnCount - 1));
                var anchorY    = 0.745f - row * 0.098f;
                // alternate accent colour per column
                var accentCol  = (column % 3 == 0) ? CMint : (column % 3 == 1) ? CBlue : CPurple;
                levelSelectButtons.Add(CreatePillButton(levelSelectPanel.transform,
                    $"{level.index}.  {level.title}",
                    new Vector2(anchorX, anchorY),
                    () => StartLevel(localIndex),
                    new Vector2(280f, 50f),
                    accentCol));
            }

            levelSelectButtons.Add(CreatePillButton(levelSelectPanel.transform,
                "← BACK", new Vector2(0.5f, 0.095f), ShowTitle,
                new Vector2(220f, 50f), CRed));
        }

        // ════════════════════════════════════════════════════════════════════
        //  HUD PANEL
        // ════════════════════════════════════════════════════════════════════
        private void BuildHudPanel()
        {
            hudPanel = CreateFullPanel("HudPanel", Color.clear);

            // top-left level info pill
            var infoBar = CreateRect(hudPanel.transform, "InfoBar",
                new Vector2(0f, 0.90f), new Vector2(0.42f, 1.00f),
                new Color(0.04f, 0.06f, 0.10f, 0.80f));
            titleText    = CreateText(infoBar.transform, string.Empty, 26, TextAnchor.MiddleLeft,
                new Vector2(0.04f, 0.62f), new Vector2(600f, 40f));
            subtitleText = CreateText(infoBar.transform, string.Empty, 15, TextAnchor.MiddleLeft,
                new Vector2(0.04f, 0.22f), new Vector2(700f, 30f),
                new Color(CBlue.r, CBlue.g, CBlue.b, 0.80f));

            // top-right score pill
            var scorePill = CreateRect(hudPanel.transform, "ScorePill",
                new Vector2(0.80f, 0.90f), new Vector2(1.00f, 1.00f),
                new Color(0.04f, 0.06f, 0.10f, 0.80f));
            captureText = CreateText(scorePill.transform, string.Empty, 28, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(260f, 50f), CMint);

            hintText = CreateText(hudPanel.transform, string.Empty, 15, TextAnchor.MiddleLeft,
                new Vector2(0.02f, 0.03f), new Vector2(900f, 36f),
                new Color(1f, 1f, 1f, 0.38f));

            // bottom-right buttons
            CreatePillButton(hudPanel.transform, "RESTART",
                new Vector2(0.84f, 0.045f), () => levelManager.RestartCurrentLevel(),
                new Vector2(160f, 42f), CAmber);
            CreatePillButton(hudPanel.transform, "LEVELS",
                new Vector2(0.955f, 0.045f), ShowLevelSelect,
                new Vector2(130f, 42f), CBlue);
        }

        // ════════════════════════════════════════════════════════════════════
        //  LEVEL COMPLETE PANEL
        // ════════════════════════════════════════════════════════════════════
        private void BuildCompletePanel()
        {
            completePanel = CreateFullPanel("CompletePanel", new Color(0.02f, 0.03f, 0.05f, 0.88f));

            CreateOrb(completePanel.transform, "OrbComplete", new Vector2(0.5f, 0.5f), 600f,
                new Color(CMint.r, CMint.g, CMint.b, 0.10f));

            completeText = CreateText(completePanel.transform, "LEVEL COMPLETE",
                52, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.60f), new Vector2(800f, 80f), CMint);
            completeText.fontStyle = FontStyle.Bold;

            CreateHorizBar(completePanel.transform, "CompleteSep",
                new Vector2(0.3f, 0.545f), new Vector2(0.7f, 0.545f),
                new Color(CMint.r, CMint.g, CMint.b, 0.40f), 1f);

            CreatePillButton(completePanel.transform, "NEXT LEVEL",
                new Vector2(0.5f, 0.44f), () => levelManager.LoadNextLevel(),
                new Vector2(300f, 62f), CMint);
            CreatePillButton(completePanel.transform, "LEVEL SELECT",
                new Vector2(0.5f, 0.335f), ShowLevelSelect,
                new Vector2(300f, 62f), CBlue);
        }

        // ── Panel / level helpers ────────────────────────────────────────────
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
            titleText.text    = $"Level {level.index}:  {level.title}";
            subtitleText.text = level.subtitle;
            hintText.text     = $"Hint: {level.hint}";
            HandleCaptureChanged(captured, required);
            SetPanel(hudPanel);
        }

        private void HandleCaptureChanged(int captured, int required)
        {
            captureText.text = $"{captured} / {required}";
        }

        private void HandleLevelCompleted(LevelData level)
        {
            completeText.text = $"Level {level.index}  Complete";
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
            if (currentButtons.Count == 0) return;
            selectedButtonIndex = (index + currentButtons.Count) % currentButtons.Count;
            ResetButtonLabels();
            var sel = currentButtons[selectedButtonIndex];
            if (buttonLabels.TryGetValue(sel, out var lbl))
                lbl.text = "▶  " + buttonBaseLabels[sel];
        }

        private void ResetButtonLabels()
        {
            foreach (var p in buttonLabels)
                p.Value.text = "   " + buttonBaseLabels[p.Key];
        }

        private void HandleMouseClick()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
            var pos = Mouse.current.position.ReadValue();
            foreach (var pair in buttonLabels)
            {
                var btn = pair.Key;
                if (!btn.gameObject.activeInHierarchy) continue;
                if (RectTransformUtility.RectangleContainsScreenPoint(btn.GetComponent<RectTransform>(), pos))
                {
                    btn.onClick.Invoke();
                    return;
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  FACTORY HELPERS
        // ════════════════════════════════════════════════════════════════════

        /// Full-screen panel
        private GameObject CreateFullPanel(string name, Color bg)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = bg;
            return go;
        }

        /// Anchored rect (stretches between anchorMin / anchorMax in parent)
        private GameObject CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        /// Circular glow orb (square image that reads as soft circle)
        private void CreateOrb(Transform parent, string name, Vector2 anchor, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin        = anchor;
            r.anchorMax        = anchor;
            r.sizeDelta        = new Vector2(size, size);
            r.anchoredPosition = Vector2.zero;
            go.GetComponent<Image>().color = color;
        }

        /// Full-width (or partial) horizontal line bar
        private void CreateHorizBar(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;
            r.offsetMin = new Vector2(0f, -height * 0.5f);
            r.offsetMax = new Vector2(0f,  height * 0.5f);
            go.GetComponent<Image>().color = color;
        }

        /// Small left-edge vertical colour strip (useful on cards)
        private void CreateVertStrip(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = new Vector2(0f, 0f);
            r.anchorMax = new Vector2(0f, 1f);
            r.offsetMin = Vector2.zero;
            r.offsetMax = new Vector2(4f, 0f);
            go.GetComponent<Image>().color = color;
        }

        /// Spinning rect accent (replaces old accent blocks)
        private void CreateSpinBox(Transform parent, string name, Vector2 anchor,
            float w, float h, Color color, float speed)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(UIAmbientAnimator));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin        = anchor;
            r.anchorMax        = anchor;
            r.sizeDelta        = new Vector2(w, h);
            r.anchoredPosition = Vector2.zero;
            r.localRotation    = Quaternion.Euler(0f, 0f, speed * 5f);
            go.GetComponent<Image>().color = color;
            go.GetComponent<UIAmbientAnimator>().Configure(speed);
        }

        // ── Text ─────────────────────────────────────────────────────────────
        private Text CreateText(Transform parent, string content, int size,
            TextAnchor anchor, Vector2 anchorPos, Vector2 dims)
            => CreateText(parent, content, size, anchor, anchorPos, dims, Color.white);

        private Text CreateText(Transform parent, string content, int size,
            TextAnchor anchor, Vector2 anchorPos, Vector2 dims, Color color)
        {
            var go = new GameObject("Text_" + content.Substring(0, Mathf.Min(12, content.Length)),
                typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.sizeDelta        = dims;
            r.anchorMin        = anchorPos;
            r.anchorMax        = anchorPos;
            r.anchoredPosition = Vector2.zero;

            var t = go.GetComponent<Text>();
            t.text             = content;
            t.font             = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize         = size;
            t.alignment        = anchor;
            t.color            = color;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow   = VerticalWrapMode.Truncate;
            t.raycastTarget    = false;
            return t;
        }

        // ── Double-layer title text (shadow + coloured) ───────────────────────
        private void CreateTitleText(Transform parent, string coloredContent,
            string shadowContent, int size, Vector2 anchorPos, Vector2 dims)
        {
            // drop-shadow
            var shadow = CreateText(parent, shadowContent, size,
                TextAnchor.MiddleCenter, anchorPos, dims,
                new Color(0f, 0f, 0f, 0.75f));
            shadow.GetComponent<RectTransform>().anchoredPosition = new Vector2(5f, -6f);

            // coloured text on top
            var title = CreateText(parent, coloredContent, size,
                TextAnchor.MiddleCenter, anchorPos, dims, Color.white);
            title.fontStyle      = FontStyle.Bold;
            title.supportRichText = true;

            // outer glow: slightly larger, low-alpha, same anchor
            var glow = CreateText(parent, coloredContent, size + 2,
                TextAnchor.MiddleCenter, anchorPos,
                new Vector2(dims.x + 20f, dims.y + 20f),
                new Color(CBlue.r, CBlue.g, CBlue.b, 0.22f));
            glow.fontStyle       = FontStyle.Bold;
            glow.supportRichText  = true;
        }

        private static string ColorfulTitle()
        {
            return "<color=#33F59E>C</color>" +
                   "<color=#61D7FF>H</color>" +
                   "<color=#FFD12E>R</color>" +
                   "<color=#C27AFF>O</color>" +
                   "<color=#FF5C6B>M</color>" +
                   "<color=#33F59E>A</color>" +
                   "  " +
                   "<color=#61D7FF>C</color>" +
                   "<color=#FFD12E>U</color>" +
                   "<color=#C27AFF>B</color>" +
                   "<color=#FF5C6B>E</color>";
        }

        // ── Pill-style button ─────────────────────────────────────────────────
        private Button CreatePillButton(Transform parent, string label,
            Vector2 anchorPos, UnityEngine.Events.UnityAction action,
            Vector2 dims, Color accent)
        {
            // outer container
            var go = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.sizeDelta        = dims;
            r.anchorMin        = anchorPos;
            r.anchorMax        = anchorPos;
            r.anchoredPosition = Vector2.zero;

            // base colour: very dark, slight accent tint
            var baseCol = new Color(
                Mathf.Lerp(0.06f, accent.r, 0.10f),
                Mathf.Lerp(0.09f, accent.g, 0.10f),
                Mathf.Lerp(0.13f, accent.b, 0.10f),
                0.94f);
            var hoverCol = new Color(
                Mathf.Lerp(0.10f, accent.r, 0.22f),
                Mathf.Lerp(0.14f, accent.g, 0.22f),
                Mathf.Lerp(0.20f, accent.b, 0.22f),
                1f);
            var pressCol = new Color(
                Mathf.Lerp(0.04f, accent.r, 0.55f),
                Mathf.Lerp(0.06f, accent.g, 0.55f),
                Mathf.Lerp(0.08f, accent.b, 0.55f),
                1f);

            var img = go.GetComponent<Image>();
            img.color = baseCol;

            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(action);
            var cols = btn.colors;
            cols.normalColor      = baseCol;
            cols.highlightedColor = hoverCol;
            cols.pressedColor     = pressCol;
            cols.selectedColor    = hoverCol;
            cols.colorMultiplier  = 1f;
            cols.fadeDuration     = 0.08f;
            btn.colors = cols;

            // left accent strip
            var strip = new GameObject("Strip", typeof(RectTransform), typeof(Image));
            strip.transform.SetParent(go.transform, false);
            var sr = strip.GetComponent<RectTransform>();
            sr.anchorMin = new Vector2(0f, 0f);
            sr.anchorMax = new Vector2(0f, 1f);
            sr.offsetMin = Vector2.zero;
            sr.offsetMax = new Vector2(4f, 0f);
            strip.GetComponent<Image>().color = new Color(accent.r, accent.g, accent.b, 0.90f);

            // label
            var lbl = CreateText(go.transform, "   " + label, 20,
                TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), dims);
            lbl.fontStyle = FontStyle.Bold;
            lbl.color     = Color.white;

            buttonLabels[btn]     = lbl;
            buttonBaseLabels[btn] = label;
            return btn;
        }
    }
}
