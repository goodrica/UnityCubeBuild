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
                    }),
                WorldCubeLevel(
                    "level-5",
                    5,
                    "World Cube",
                    "Bonus round: roll across every side.",
                    "Cross the edges of the large cube and solve colored tiles on each face.",
                    4,
                    new Vector2Int(1, 1),
                    LoadBackgroundTexture(5)),
                PathLevel(
                    "level-6",
                    6,
                    "Soft Crossing",
                    "More floor, more color.",
                    "Use the extra space to prepare the next bottom face.",
                    6,
                    5,
                    new Vector2Int(1, 4),
                    new[] { Direction.East, Direction.East, Direction.East, Direction.North, Direction.North, Direction.West, Direction.West, Direction.West, Direction.North, Direction.East, Direction.East, Direction.East },
                    7,
                    LoadBackgroundTexture(6)),
                PathLevel(
                    "level-7",
                    7,
                    "Gentle Switchback",
                    "A longer calm route.",
                    "Rotate the view and move by what you see on screen.",
                    6,
                    6,
                    new Vector2Int(0, 5),
                    new[] { Direction.East, Direction.East, Direction.East, Direction.East, Direction.North, Direction.North, Direction.West, Direction.West, Direction.West, Direction.North, Direction.North, Direction.East, Direction.East, Direction.East },
                    8,
                    LoadBackgroundTexture(5)),
                PathLevel(
                    "level-8",
                    8,
                    "Open Drift",
                    "The puzzle breathes outward.",
                    "Each claimed tile fades back, leaving the remaining colors clear.",
                    7,
                    6,
                    new Vector2Int(1, 5),
                    new[] { Direction.East, Direction.East, Direction.East, Direction.East, Direction.North, Direction.North, Direction.West, Direction.West, Direction.West, Direction.West, Direction.North, Direction.North, Direction.East, Direction.East, Direction.East, Direction.East },
                    9,
                    LoadBackgroundTexture(6)),
                PathLevel(
                    "level-9",
                    9,
                    "Chroma Garden",
                    "The largest classic board so far.",
                    "Plan the route and let the camera guide your directions.",
                    7,
                    7,
                    new Vector2Int(0, 6),
                    new[] { Direction.East, Direction.East, Direction.East, Direction.East, Direction.East, Direction.North, Direction.North, Direction.West, Direction.West, Direction.West, Direction.West, Direction.West, Direction.North, Direction.North, Direction.East, Direction.East, Direction.East, Direction.East },
                    10,
                    LoadBackgroundTexture(5)),
                PathLevel(
                    "level-10",
                    10,
                    "Prism Lanes",
                    "A longer classic route with room to recover.",
                    "Use the open lanes to reset the bottom face before each color tile.",
                    8,
                    7,
                    new Vector2Int(0, 6),
                    new[] { Direction.East, Direction.East, Direction.East, Direction.East, Direction.East, Direction.North, Direction.North, Direction.West, Direction.West, Direction.West, Direction.North, Direction.North, Direction.East, Direction.East, Direction.East, Direction.East, Direction.North, Direction.North, Direction.West, Direction.West },
                    11,
                    LoadBackgroundTexture(10)),
                PathLevel(
                    "level-11",
                    11,
                    "Aurora Bend",
                    "Wide turns and sharper color timing.",
                    "The route bends back on itself; rotate the view and plan two captures ahead.",
                    8,
                    8,
                    new Vector2Int(1, 7),
                    new[] { Direction.East, Direction.East, Direction.East, Direction.East, Direction.East, Direction.North, Direction.North, Direction.North, Direction.West, Direction.West, Direction.West, Direction.West, Direction.North, Direction.North, Direction.East, Direction.East, Direction.East, Direction.East, Direction.East, Direction.South, Direction.West, Direction.West, Direction.North, Direction.North, Direction.North },
                    12,
                    LoadBackgroundTexture(10)),
                PathLevel(
                    "level-12",
                    12,
                    "Glass Spiral",
                    "A large classic finale.",
                    "Work around the spiral and use the neutral floor to prepare difficult faces.",
                    9,
                    8,
                    new Vector2Int(0, 7),
                    new[] { Direction.East, Direction.East, Direction.East, Direction.East, Direction.East, Direction.East, Direction.East, Direction.North, Direction.North, Direction.West, Direction.West, Direction.West, Direction.West, Direction.West, Direction.North, Direction.North, Direction.East, Direction.East, Direction.East, Direction.East, Direction.East, Direction.East, Direction.North, Direction.North, Direction.North, Direction.West, Direction.West, Direction.West, Direction.West, Direction.South, Direction.South, Direction.West, Direction.West, Direction.West },
                    13,
                    LoadBackgroundTexture(10))
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

        private static LevelData PathLevel(string id, int index, string title, string subtitle, string hint, int width, int height, Vector2Int start, Direction[] path, int requiredCount, Texture2D backgroundTexture)
        {
            var tiles = new List<TileData>();
            var position = start;
            var orientation = CubeOrientation.Identity();
            var requiredByPosition = new Dictionary<Vector2Int, TileData>();
            var pathTiles = new List<TileData>
            {
                Tile($"l{index}-00", position.x, position.y, "stone", false)
            };

            for (var i = 0; i < path.Length; i++)
            {
                orientation.Roll(path[i]);
                position += DirectionToGridOffset(path[i]);
                var bottomColor = ColorForFace(orientation.GetBottomFace());
                pathTiles.Add(Tile($"l{index}-{i + 1:00}", position.x, position.y, bottomColor, true));
            }

            var firstRequiredIndex = Mathf.Max(1, pathTiles.Count - requiredCount);
            for (var i = 0; i < pathTiles.Count; i++)
            {
                if (i < firstRequiredIndex)
                {
                    pathTiles[i].colorId = "stone";
                    pathTiles[i].required = false;
                }
                else
                {
                    requiredByPosition[pathTiles[i].gridPos] = pathTiles[i];
                }
            }

            for (var row = 0; row < height; row++)
            {
                for (var col = 0; col < width; col++)
                {
                    var gridPos = new Vector2Int(col, row);
                    if (requiredByPosition.TryGetValue(gridPos, out var requiredTile))
                    {
                        tiles.Add(requiredTile);
                    }
                    else
                    {
                        tiles.Add(Tile($"l{index}-floor-{col}-{row}", col, row, "stone", false));
                    }
                }
            }

            var level = Level(id, index, title, subtitle, hint, width, height, start, tiles.ToArray());
            level.backgroundTexture = backgroundTexture;
            return level;
        }

        private static LevelData WorldCubeLevel(string id, int index, string title, string subtitle, string hint, int size, Vector2Int start, Texture2D backgroundTexture)
        {
            var tiles = new List<TileData>();
            foreach (WorldCubeFace face in System.Enum.GetValues(typeof(WorldCubeFace)))
            {
                for (var row = 0; row < size; row++)
                {
                    for (var col = 0; col < size; col++)
                    {
                        tiles.Add(WorldTile($"l{index}-{face}-{col}-{row}", face, col, row, "stone", false));
                    }
                }
            }

            AddGoal(tiles, WorldCubeFace.Top, 2, 1, "lavender");
            AddGoal(tiles, WorldCubeFace.North, 1, 2, "amber");
            AddGoal(tiles, WorldCubeFace.East, 2, 1, "mint");
            AddGoal(tiles, WorldCubeFace.South, 1, 1, "ocean");
            AddGoal(tiles, WorldCubeFace.West, 2, 2, "coral");
            AddGoal(tiles, WorldCubeFace.Bottom, 1, 2, "slate");

            var level = Level(id, index, title, subtitle, hint, size, size, start, tiles.ToArray());
            level.mechanicsMode = MechanicsMode.WorldCube;
            level.backgroundTexture = backgroundTexture;
            return level;
        }

        private static void AddGoal(List<TileData> tiles, WorldCubeFace face, int col, int row, string colorId)
        {
            for (var i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].worldFace == face && tiles[i].gridPos == new Vector2Int(col, row))
                {
                    tiles[i].colorId = colorId;
                    tiles[i].required = true;
                    return;
                }
            }
        }

        private static TileData Tile(string id, int col, int row, string colorId, bool required)
        {
            return new TileData
            {
                id = id,
                worldFace = WorldCubeFace.Top,
                gridPos = new Vector2Int(col, row),
                colorId = colorId,
                required = required,
                captured = false,
                active = true
            };
        }

        private static TileData WorldTile(string id, WorldCubeFace face, int col, int row, string colorId, bool required)
        {
            return new TileData
            {
                id = id,
                worldFace = face,
                gridPos = new Vector2Int(col, row),
                colorId = colorId,
                required = required,
                captured = false,
                active = true
            };
        }

        private static Vector2Int DirectionToGridOffset(Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return new Vector2Int(0, -1);
                case Direction.South:
                    return new Vector2Int(0, 1);
                case Direction.East:
                    return new Vector2Int(1, 0);
                case Direction.West:
                    return new Vector2Int(-1, 0);
                default:
                    return Vector2Int.zero;
            }
        }

        private static string ColorForFace(FaceKey face)
        {
            switch (face)
            {
                case FaceKey.Top:
                    return "mint";
                case FaceKey.Bottom:
                    return "slate";
                case FaceKey.North:
                    return "amber";
                case FaceKey.South:
                    return "ocean";
                case FaceKey.East:
                    return "lavender";
                case FaceKey.West:
                    return "coral";
                default:
                    return "stone";
            }
        }

        private static Texture2D LoadBackgroundTexture(int levelIndex)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/_Game/Textures/{levelIndex}.png");
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
