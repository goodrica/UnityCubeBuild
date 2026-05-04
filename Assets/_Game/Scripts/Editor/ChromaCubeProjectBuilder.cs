using System.Collections.Generic;
using ChromaCube.Audio;
using ChromaCube.Cameras;
using ChromaCube.Core;
using ChromaCube.Data;
using ChromaCube.Level;
using ChromaCube.Movement;
using ChromaCube.Rendering;
using ChromaCube.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ChromaCube.EditorTools
{
    public static class ChromaCubeProjectBuilder
    {
        private const string LevelFolder = "Assets/_Game/ScriptableObjects/Levels";
        private const string SceneFolder = "Assets/_Game/Scenes";

        [MenuItem("Tools/Chroma Cube/Build Starter Project")]
        public static void BuildStarterProject()
        {
            EnsureFolders();
            var levels = CreateLevels();
            CreateGameScene(levels);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Chroma Cube", "Starter project generated. Open Assets/_Game/Scenes/Game.unity and press Play.", "OK");
        }

        private static void EnsureFolders()
        {
            CreateFolder("Assets", "_Game");
            CreateFolder("Assets/_Game", "Scenes");
            CreateFolder("Assets/_Game", "ScriptableObjects");
            CreateFolder("Assets/_Game/ScriptableObjects", "Levels");
        }

        private static void CreateFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static List<LevelData> CreateLevels()
        {
            var levels = new List<LevelData>
            {
                Level(
                    "level-1",
                    1,
                    "Quiet Start",
                    "One move, one match.",
                    "Roll east. The cube's bottom face becomes lavender.",
                    4,
                    3,
                    new Vector2Int(1, 1),
                    new[]
                    {
                        Tile("l1-a", 1, 0, "stone", false),
                        Tile("l1-b", 2, 0, "stone", false),
                        Tile("l1-c", 0, 1, "stone", false),
                        Tile("l1-start", 1, 1, "stone", false),
                        Tile("l1-goal", 2, 1, "lavender", true),
                        Tile("l1-d", 1, 2, "stone", false),
                        Tile("l1-e", 2, 2, "stone", false)
                    }),
                Level(
                    "level-2",
                    2,
                    "Turn To See",
                    "Orientation matters.",
                    "The nearest tile is not always the right tile. Set the bottom face first.",
                    4,
                    4,
                    new Vector2Int(1, 1),
                    new[]
                    {
                        Tile("l2-entry", 0, 0, "stone", false),
                        Tile("l2-a", 1, 1, "stone", false),
                        Tile("l2-b", 1, 0, "ocean", true),
                        Tile("l2-c", 2, 0, "coral", true),
                        Tile("l2-d", 2, 1, "stone", false),
                        Tile("l2-e", 0, 1, "stone", false),
                        Tile("l2-f", 0, 2, "stone", false),
                        Tile("l2-g", 1, 2, "stone", false)
                    }),
                Level(
                    "level-3",
                    3,
                    "Soft Sequence",
                    "Plan a few moves ahead.",
                    "A good path can prepare the next bottom face before you arrive.",
                    4,
                    4,
                    new Vector2Int(1, 2),
                    new[]
                    {
                        Tile("l3-a", 1, 2, "stone", false),
                        Tile("l3-b", 1, 1, "ocean", true),
                        Tile("l3-c", 2, 1, "coral", true),
                        Tile("l3-d", 2, 2, "mint", true),
                        Tile("l3-e", 0, 2, "stone", false),
                        Tile("l3-f", 0, 1, "stone", false)
                    }),
                Level(
                    "level-4",
                    4,
                    "Color Weave",
                    "Multiple colors, tighter routing.",
                    "Use the side tiles to prepare the exact face you want touching down.",
                    5,
                    4,
                    new Vector2Int(2, 2),
                    new[]
                    {
                        Tile("l4-a", 2, 2, "stone", false),
                        Tile("l4-b", 2, 1, "ocean", true),
                        Tile("l4-c", 3, 1, "coral", true),
                        Tile("l4-d", 3, 2, "mint", true),
                        Tile("l4-e", 1, 2, "lavender", true),
                        Tile("l4-f", 1, 1, "stone", false),
                        Tile("l4-g", 1, 0, "stone", false),
                        Tile("l4-h", 2, 0, "amber", true),
                        Tile("l4-i", 3, 0, "stone", false)
                    })
            };

            for (var i = 0; i < levels.Count; i++)
            {
                var path = $"{LevelFolder}/Level_{levels[i].index:00}_{levels[i].levelId}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(levels[i], existing);
                    Object.DestroyImmediate(levels[i]);
                    levels[i] = existing;
                    EditorUtility.SetDirty(existing);
                }
                else
                {
                    AssetDatabase.CreateAsset(levels[i], path);
                }
            }

            return levels;
        }

        private static LevelData Level(string id, int index, string title, string subtitle, string hint, int width, int height, Vector2Int start, TileData[] tiles)
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            level.levelId = id;
            level.index = index;
            level.title = title;
            level.subtitle = subtitle;
            level.hint = hint;
            level.width = width;
            level.height = height;
            level.start = start;
            level.mechanicsMode = MechanicsMode.Classic;
            level.tiles = new List<TileData>(tiles);
            level.faceColorMap = new List<FaceColorEntry>
            {
                new FaceColorEntry { face = FaceKey.Top, colorId = "mint" },
                new FaceColorEntry { face = FaceKey.Bottom, colorId = "slate" },
                new FaceColorEntry { face = FaceKey.North, colorId = "amber" },
                new FaceColorEntry { face = FaceKey.South, colorId = "ocean" },
                new FaceColorEntry { face = FaceKey.East, colorId = "lavender" },
                new FaceColorEntry { face = FaceKey.West, colorId = "coral" }
            };
            return level;
        }

        private static TileData Tile(string id, int col, int row, string colorId, bool required)
        {
            return new TileData
            {
                id = id,
                gridPos = new Vector2Int(col, row),
                colorId = colorId,
                required = required,
                captured = false,
                active = true
            };
        }

        private static void CreateGameScene(List<LevelData> levels)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Game";

            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<UnityEngine.Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.055f, 0.065f, 0.08f);

            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var root = new GameObject("GameRoot");
            var gameManager = root.AddComponent<GameManager>();
            root.AddComponent<LevelManager>();
            root.AddComponent<InputRouter>();
            root.AddComponent<CameraRigController>();
            root.AddComponent<AudioManager>();

            var boardRoot = new GameObject("BoardRoot");
            boardRoot.transform.SetParent(root.transform);
            boardRoot.AddComponent<BoardRenderer>();

            var playerCube = new GameObject("PlayerCubeRoot");
            playerCube.transform.SetParent(root.transform);
            playerCube.AddComponent<CubeRenderer>();
            playerCube.AddComponent<ClassicMovementController>();

            var uiRoot = new GameObject("UIRoot");
            uiRoot.transform.SetParent(root.transform);
            uiRoot.AddComponent<UIController>();

            gameManager.SetLevels(levels);

            var scenePath = $"{SceneFolder}/Game.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            CreateBootstrapScene(scenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{SceneFolder}/Bootstrap.unity", true),
                new EditorBuildSettingsScene(scenePath, true)
            };
        }

        private static void CreateBootstrapScene(string gameScenePath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Bootstrap";

            var loader = new GameObject("BootstrapLoader");
            var loaderComponent = loader.AddComponent<BootstrapSceneLoader>();
            loaderComponent.gameSceneName = System.IO.Path.GetFileNameWithoutExtension(gameScenePath);

            EditorSceneManager.SaveScene(scene, $"{SceneFolder}/Bootstrap.unity");
        }
    }
}
