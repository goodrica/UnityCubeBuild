using System;
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

        [MenuItem("Chroma Cube/Build Starter Project")]
        [MenuItem("Tools/Chroma Cube/Build Starter Project")]
        public static void BuildStarterProject()
        {
            EnsureFolders();
            var levels = CreateLevels();
            CreateGameScene(levels);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Chroma Cube", "Starter project generated. Open Assets/_Game/Scenes/Game.unity and press Play.", "OK");
            }
        }

        [MenuItem("Chroma Cube/Open Game Scene")]
        [MenuItem("Tools/Chroma Cube/Open Game Scene")]
        public static void OpenGameScene()
        {
            var scenePath = $"{SceneFolder}/Game.unity";
            if (!System.IO.File.Exists(scenePath))
            {
                BuildStarterProject();
                return;
            }

            EditorSceneManager.OpenScene(scenePath);
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
                    new Vector2Int(2, 4),
                    new[] { Direction.West, Direction.North, Direction.West, Direction.South, Direction.East, Direction.North, Direction.East, Direction.North, Direction.East, Direction.South, Direction.East, Direction.East, Direction.North, Direction.West, Direction.North, Direction.East, Direction.North, Direction.West, Direction.West, Direction.South, Direction.West, Direction.North },
                    7,
                    LoadBackgroundTexture(6),
                    scatterGoals: true,
                    decoyCount: 4),
                PathLevel(
                    "level-7",
                    7,
                    "Gentle Switchback",
                    "A longer calm route.",
                    "Rotate the view and move by what you see on screen.",
                    7,
                    6,
                    new Vector2Int(3, 5),
                    new[] { Direction.North, Direction.West, Direction.South, Direction.West, Direction.North, Direction.West, Direction.North, Direction.East, Direction.East, Direction.East, Direction.North, Direction.North, Direction.West, Direction.South, Direction.West, Direction.West, Direction.North, Direction.East, Direction.North, Direction.West, Direction.South, Direction.East, Direction.North, Direction.East, Direction.East, Direction.East, Direction.East, Direction.South },
                    8,
                    LoadBackgroundTexture(5),
                    scatterGoals: true,
                    decoyCount: 6),
                PathLevel(
                    "level-8",
                    8,
                    "Open Drift",
                    "The puzzle breathes outward.",
                    "Each claimed tile fades back, leaving the remaining colors clear.",
                    8,
                    7,
                    new Vector2Int(3, 6),
                    new[] { Direction.East, Direction.North, Direction.West, Direction.North, Direction.West, Direction.South, Direction.West, Direction.South, Direction.West, Direction.North, Direction.North, Direction.East, Direction.North, Direction.West, Direction.North, Direction.East, Direction.North, Direction.East, Direction.North, Direction.East, Direction.South, Direction.East, Direction.South, Direction.East, Direction.South, Direction.West, Direction.South, Direction.East, Direction.South, Direction.South, Direction.East, Direction.North, Direction.East, Direction.North },
                    9,
                    LoadBackgroundTexture(6),
                    scatterGoals: true,
                    decoyCount: 8),
                PathLevel(
                    "level-9",
                    9,
                    "Chroma Garden",
                    "The largest classic board so far.",
                    "Plan the route and let the camera guide your directions.",
                    7,
                    7,
                    new Vector2Int(3, 6),
                    new[] { Direction.West, Direction.North, Direction.West, Direction.South, Direction.West, Direction.North, Direction.North, Direction.East, Direction.East, Direction.North, Direction.East, Direction.South, Direction.East, Direction.South, Direction.East, Direction.South, Direction.West, Direction.North, Direction.West, Direction.South, Direction.East, Direction.North, Direction.East, Direction.North, Direction.East, Direction.South, Direction.South, Direction.West, Direction.North, Direction.East, Direction.North, Direction.North, Direction.West, Direction.West, Direction.North, Direction.West, Direction.North, Direction.West },
                    10,
                    LoadMilkyWayBackgroundTexture(),
                    scatterGoals: true,
                    decoyCount: 12),
                PathLevel(
                    "level-10",
                    10,
                    "Prism Lanes",
                    "A longer classic route with room to recover.",
                    "Use the open lanes to reset the bottom face before each color tile.",
                    8,
                    8,
                    new Vector2Int(3, 7),
                    new[] { Direction.North, Direction.West, Direction.South, Direction.West, Direction.North, Direction.West, Direction.North, Direction.East, Direction.North, Direction.West, Direction.North, Direction.East, Direction.North, Direction.West, Direction.North, Direction.East, Direction.East, Direction.North, Direction.West, Direction.West, Direction.South, Direction.East, Direction.North, Direction.West, Direction.South, Direction.East, Direction.South, Direction.East, Direction.South, Direction.East, Direction.South, Direction.West, Direction.South, Direction.East, Direction.East, Direction.East, Direction.North, Direction.East, Direction.North, Direction.East, Direction.North, Direction.West, Direction.North, Direction.East, Direction.North },
                    11,
                    LoadBackgroundTexture(10),
                    scatterGoals: true,
                    decoyCount: 16),
                PathLevel(
                    "level-11",
                    11,
                    "Aurora Bend",
                    "Wide turns and sharper color timing.",
                    "The route bends back on itself; rotate the view and plan two captures ahead.",
                    9,
                    8,
                    new Vector2Int(4, 7),
                    new[] { Direction.North, Direction.East, Direction.North, Direction.West, Direction.North, Direction.East, Direction.North, Direction.East, Direction.North, Direction.West, Direction.North, Direction.West, Direction.South, Direction.West, Direction.North, Direction.North, Direction.West, Direction.South, Direction.West, Direction.South, Direction.West, Direction.North, Direction.North, Direction.East, Direction.East, Direction.South, Direction.South, Direction.South, Direction.East, Direction.South, Direction.West, Direction.West, Direction.North, Direction.West, Direction.South, Direction.South, Direction.East, Direction.South, Direction.West, Direction.South, Direction.East, Direction.East, Direction.North, Direction.East, Direction.North, Direction.West, Direction.South, Direction.East, Direction.South, Direction.East, Direction.East, Direction.East },
                    12,
                    LoadBackgroundTexture(10),
                    scatterGoals: true,
                    decoyCount: 18),
                PathLevel(
                    "level-12",
                    12,
                    "Glass Spiral",
                    "A large classic finale.",
                    "Work around the spiral and use the neutral floor to prepare difficult faces.",
                    10,
                    9,
                    new Vector2Int(4, 8),
                    new[] { Direction.West, Direction.North, Direction.East, Direction.North, Direction.East, Direction.South, Direction.East, Direction.South, Direction.East, Direction.North, Direction.North, Direction.East, Direction.East, Direction.South, Direction.West, Direction.South, Direction.East, Direction.North, Direction.West, Direction.North, Direction.North, Direction.West, Direction.North, Direction.West, Direction.North, Direction.West, Direction.South, Direction.West, Direction.North, Direction.West, Direction.South, Direction.West, Direction.North, Direction.West, Direction.North, Direction.West, Direction.North, Direction.East, Direction.East, Direction.North, Direction.East, Direction.South, Direction.South, Direction.East, Direction.North, Direction.East, Direction.East, Direction.North, Direction.West, Direction.West, Direction.East, Direction.South, Direction.South, Direction.East, Direction.East, Direction.North, Direction.East, Direction.North, Direction.East, Direction.South },
                    13,
                    LoadBackgroundTexture(10),
                    scatterGoals: true,
                    decoyCount: 22),
                PathLevel(
                    "level-13",
                    13,
                    "Slate Run",
                    "A less obvious texture test.",
                    "Colored tiles are no longer a painted path. Find the order by watching the cube orientation, not the board pattern.",
                    10,
                    10,
                    new Vector2Int(4, 9),
                    new[] { Direction.East, Direction.North, Direction.East, Direction.North, Direction.East, Direction.North, Direction.North, Direction.East, Direction.North, Direction.East, Direction.South, Direction.South, Direction.West, Direction.South, Direction.East, Direction.South, Direction.West, Direction.South, Direction.East, Direction.North, Direction.West, Direction.West, Direction.South, Direction.West, Direction.North, Direction.East, Direction.South, Direction.East, Direction.North, Direction.East, Direction.North, Direction.West, Direction.South, Direction.West, Direction.South, Direction.West, Direction.North, Direction.West, Direction.North, Direction.West, Direction.West, Direction.West, Direction.South, Direction.East, Direction.South, Direction.West, Direction.West, Direction.North, Direction.West, Direction.South, Direction.East, Direction.North, Direction.North, Direction.West, Direction.North, Direction.East, Direction.North, Direction.West, Direction.North, Direction.East, Direction.North, Direction.East, Direction.South, Direction.East, Direction.North, Direction.East, Direction.South, Direction.East },
                    14,
                    LoadBackgroundTexture(10),
                    "poliigon_floor",
                    scatterGoals: true,
                    decoyCount: 26),
                SerpentineLevel(
                    "level-14",
                    14,
                    "Starfield Causeway",
                    "A broader board with long, clean lanes.",
                    "The floor is generous now. Use the open space to set the bottom face before each distant target.",
                    11,
                    10,
                    new Vector2Int(5, 9),
                    moveWestFirst: true,
                    requiredCount: 15,
                    backgroundTexture: LoadArtBackgroundTexture(0, LoadMilkyWayBackgroundTexture()),
                    decoyCount: 28),
                SerpentineLevel(
                    "level-15",
                    15,
                    "Wide Aurora",
                    "The route stretches farther across the board.",
                    "Targets are separated on purpose. Think in travel lanes, not short hops.",
                    12,
                    10,
                    new Vector2Int(5, 9),
                    moveWestFirst: false,
                    requiredCount: 16,
                    backgroundTexture: LoadArtBackgroundTexture(1, LoadBackgroundTexture(10)),
                    decoyCount: 30),
                SerpentineLevel(
                    "level-16",
                    16,
                    "Open Horizon",
                    "More room to recover and reorient.",
                    "The safest move is often a setup move. Let the empty floor help you reset the cube.",
                    12,
                    11,
                    new Vector2Int(6, 10),
                    moveWestFirst: true,
                    requiredCount: 17,
                    backgroundTexture: LoadArtBackgroundTexture(2, LoadBackgroundTexture(6)),
                    floorColorId: "poliigon_floor",
                    decoyCount: 34,
                    edgeInset: 1),
                SerpentineLevel(
                    "level-17",
                    17,
                    "Nebula Traverse",
                    "The puzzle now spans almost the whole field.",
                    "Watch where each successful color sits relative to the others. They are spread wide for a reason.",
                    13,
                    11,
                    new Vector2Int(6, 10),
                    moveWestFirst: false,
                    requiredCount: 18,
                    backgroundTexture: LoadArtBackgroundTexture(3, LoadBackgroundTexture(5)),
                    decoyCount: 36),
                SerpentineLevel(
                    "level-18",
                    18,
                    "Final Expanse",
                    "A large finale with the most breathing room yet.",
                    "Stay patient. Use the oversized floor to rehearse the next bottom face before you commit to faraway goals.",
                    14,
                    12,
                    new Vector2Int(7, 11),
                    moveWestFirst: true,
                    requiredCount: 19,
                    backgroundTexture: LoadArtBackgroundTexture(4, LoadBackgroundTexture(10)),
                    floorColorId: "poliigon_floor",
                    decoyCount: 42,
                    edgeInset: 1),
                ShapedLevel(
                    "level-19",
                    19,
                    "Trapezoid Drift",
                    "A broad field that narrows as you climb.",
                    "The upper rows get tighter. Use the wide base to set up the rarer colors before the board pinches in.",
                    15,
                    13,
                    CreateBottomAnchoredRowSpans(
                        new[] { (0, 14), (0, 14), (1, 14), (1, 14), (2, 14), (2, 13), (3, 13), (3, 13), (4, 13), (4, 12), (5, 12), (5, 12), (6, 12) }),
                    moveWestFirst: true,
                    requiredCount: 20,
                    backgroundTexture: LoadArtBackgroundTexture(5, LoadBackgroundTexture(10)),
                    decoyCount: 44),
                ShapedLevel(
                    "level-20",
                    20,
                    "Crown Path",
                    "A pentagon-like board with a narrower peak.",
                    "The middle is stable, but the crown at the top changes your rhythm. Save a few setup moves for the ascent.",
                    16,
                    14,
                    CreateBottomAnchoredRowSpans(
                        new[] { (2, 13), (1, 14), (1, 14), (0, 15), (0, 15), (0, 15), (1, 14), (1, 14), (2, 13), (2, 13), (3, 12), (4, 11), (5, 10), (6, 9) }),
                    moveWestFirst: false,
                    requiredCount: 21,
                    backgroundTexture: LoadArtBackgroundTexture(0, LoadBackgroundTexture(6)),
                    floorColorId: "poliigon_floor",
                    decoyCount: 46),
                ShapedLevel(
                    "level-21",
                    21,
                    "Hex Bloom",
                    "The board swells at the center, then tightens again.",
                    "The outer shoulders are useful detours. Don’t rush the center just because it looks direct.",
                    16,
                    14,
                    CreateBottomAnchoredRowSpans(
                        new[] { (3, 12), (2, 13), (1, 14), (1, 14), (0, 15), (0, 15), (0, 15), (0, 15), (1, 14), (1, 14), (2, 13), (3, 12), (4, 11), (5, 10) }),
                    moveWestFirst: true,
                    requiredCount: 22,
                    backgroundTexture: LoadArtBackgroundTexture(1, LoadBackgroundTexture(5)),
                    decoyCount: 48),
                ShapedLevel(
                    "level-22",
                    22,
                    "Facet Garden",
                    "A clipped polygon with long edges and short corners.",
                    "The corners vanish quickly here. Plan where you want to be before the board asks you to turn.",
                    17,
                    15,
                    CreateBottomAnchoredRowSpans(
                        new[] { (2, 14), (1, 15), (1, 15), (0, 16), (0, 16), (0, 16), (1, 15), (1, 15), (2, 14), (2, 14), (1, 15), (1, 15), (2, 14), (3, 13), (4, 12) }),
                    moveWestFirst: false,
                    requiredCount: 23,
                    backgroundTexture: LoadArtBackgroundTexture(2, LoadBackgroundTexture(10)),
                    decoyCount: 52),
                ShapedLevel(
                    "level-23",
                    23,
                    "Ember Hex",
                    "A larger hex field with extra shoulders to roam.",
                    "The board is generous, but the goals are farther apart now. Use the side lanes to rehearse the next face.",
                    17,
                    15,
                    CreateBottomAnchoredRowSpans(
                        new[] { (2, 14), (1, 15), (1, 15), (0, 16), (0, 16), (0, 16), (0, 16), (0, 16), (0, 16), (0, 16), (1, 15), (1, 15), (2, 14), (3, 13), (4, 12) }),
                    moveWestFirst: true,
                    requiredCount: 24,
                    backgroundTexture: LoadArtBackgroundTexture(3, LoadBackgroundTexture(6)),
                    floorColorId: "poliigon_floor",
                    decoyCount: 56),
                ShapedLevel(
                    "level-24",
                    24,
                    "Grand Pavilion",
                    "A tall pentagon with a very forgiving base.",
                    "You have room at the bottom to fix almost anything. Spend that space before climbing into the narrow roof.",
                    18,
                    16,
                    CreateBottomAnchoredRowSpans(
                        new[] { (1, 16), (1, 16), (0, 17), (0, 17), (0, 17), (0, 17), (0, 17), (1, 16), (1, 16), (1, 16), (2, 15), (2, 15), (3, 14), (4, 13), (5, 12), (6, 11) }),
                    moveWestFirst: false,
                    requiredCount: 25,
                    backgroundTexture: LoadArtBackgroundTexture(4, LoadBackgroundTexture(5)),
                    decoyCount: 60),
                ShapedLevel(
                    "level-25",
                    25,
                    "Prism Summit",
                    "A final polygon run with the widest floor count yet.",
                    "The board rewards patience. Use the deep lower lanes to line up the exact face you need before tackling the summit.",
                    18,
                    16,
                    CreateBottomAnchoredRowSpans(
                        new[] { (3, 14), (2, 15), (1, 16), (1, 16), (0, 17), (0, 17), (0, 17), (0, 17), (0, 17), (0, 17), (1, 16), (1, 16), (2, 15), (3, 14), (4, 13), (5, 12) }),
                    moveWestFirst: true,
                    requiredCount: 26,
                    backgroundTexture: LoadArtBackgroundTexture(5, LoadBackgroundTexture(10)),
                    floorColorId: "poliigon_floor",
                    decoyCount: 64)
            };

            ApplyGroupedArtBackgrounds(levels, 3);

            for (var i = 0; i < levels.Count; i++)
            {
                var path = $"{LevelFolder}/Level_{levels[i].index:00}_{levels[i].levelId}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<LevelData>(path);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(levels[i], existing);
                    UnityEngine.Object.DestroyImmediate(levels[i]);
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

        private static LevelData PathLevel(string id, int index, string title, string subtitle, string hint, int width, int height, Vector2Int start, Direction[] path, int requiredCount, Texture2D backgroundTexture, string floorColorId = "stone", bool scatterGoals = false, int decoyCount = 0)
        {
            var activeCells = new HashSet<Vector2Int>();
            for (var row = 0; row < height; row++)
            {
                for (var col = 0; col < width; col++)
                {
                    activeCells.Add(new Vector2Int(col, row));
                }
            }

            return MaskedPathLevel(id, index, title, subtitle, hint, width, height, start, activeCells, path, requiredCount, backgroundTexture, floorColorId, scatterGoals, decoyCount);
        }

        private static LevelData MaskedPathLevel(string id, int index, string title, string subtitle, string hint, int width, int height, Vector2Int start, HashSet<Vector2Int> activeCells, Direction[] path, int requiredCount, Texture2D backgroundTexture, string floorColorId = "stone", bool scatterGoals = false, int decoyCount = 0)
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
                if (!activeCells.Contains(position))
                {
                    throw new InvalidOperationException($"Level {id} path moved outside the playable mask at {position}.");
                }

                var bottomColor = ColorForFace(orientation.GetBottomFace());
                pathTiles.Add(Tile($"l{index}-{i + 1:00}", position.x, position.y, bottomColor, true));
            }

            var goalIndices = PickGoalIndices(pathTiles, requiredCount, scatterGoals);
            for (var i = 0; i < pathTiles.Count; i++)
            {
                if (goalIndices.Contains(i))
                {
                    requiredByPosition[pathTiles[i].gridPos] = pathTiles[i];
                }
                else
                {
                    pathTiles[i].colorId = floorColorId;
                    pathTiles[i].required = false;
                }
            }

            for (var row = 0; row < height; row++)
            {
                for (var col = 0; col < width; col++)
                {
                    var gridPos = new Vector2Int(col, row);
                    if (!activeCells.Contains(gridPos))
                    {
                        continue;
                    }

                    if (requiredByPosition.TryGetValue(gridPos, out var requiredTile))
                    {
                        tiles.Add(requiredTile);
                    }
                    else
                    {
                        tiles.Add(Tile($"l{index}-floor-{col}-{row}", col, row, floorColorId, false));
                    }
                }
            }

            AddColorDecoys(tiles, requiredByPosition, index, decoyCount, floorColorId);

            var level = Level(id, index, title, subtitle, hint, width, height, start, tiles.ToArray());
            level.backgroundTexture = backgroundTexture;
            return level;
        }

        private static LevelData SerpentineLevel(string id, int index, string title, string subtitle, string hint, int width, int height, Vector2Int start, bool moveWestFirst, int requiredCount, Texture2D backgroundTexture, string floorColorId = "stone", int decoyCount = 0, int edgeInset = 0)
        {
            return PathLevel(
                id,
                index,
                title,
                subtitle,
                hint,
                width,
                height,
                start,
                CreateSerpentinePath(width, height, start, moveWestFirst, edgeInset).ToArray(),
                requiredCount,
                backgroundTexture,
                floorColorId,
                scatterGoals: true,
                decoyCount: decoyCount);
        }

        private static LevelData ShapedLevel(string id, int index, string title, string subtitle, string hint, int width, int height, RowSpan[] rowSpans, bool moveWestFirst, int requiredCount, Texture2D backgroundTexture, string floorColorId = "stone", int decoyCount = 0)
        {
            var activeCells = BuildActiveCells(rowSpans);
            var start = GetShapeStart(rowSpans, moveWestFirst);
            var path = CreateMaskedSerpentinePath(rowSpans, start, moveWestFirst);
            return MaskedPathLevel(
                id,
                index,
                title,
                subtitle,
                hint,
                width,
                height,
                start,
                activeCells,
                path.ToArray(),
                requiredCount,
                backgroundTexture,
                floorColorId,
                scatterGoals: true,
                decoyCount: decoyCount);
        }

        private static HashSet<int> PickGoalIndices(List<TileData> pathTiles, int requiredCount, bool scatterGoals)
        {
            var goalIndices = new HashSet<int>();
            var pathTileCount = pathTiles.Count;
            if (!scatterGoals)
            {
                var firstRequiredIndex = Mathf.Max(1, pathTileCount - requiredCount);
                for (var i = firstRequiredIndex; i < pathTileCount; i++)
                {
                    goalIndices.Add(i);
                }

                return goalIndices;
            }

            var candidates = new List<int>();
            var seenPositions = new HashSet<Vector2Int>();
            for (var i = 2; i < pathTiles.Count; i++)
            {
                if (seenPositions.Add(pathTiles[i].gridPos))
                {
                    candidates.Add(i);
                }
            }

            if (candidates.Count == 0)
            {
                return goalIndices;
            }

            goalIndices.Add(candidates[candidates.Count - 1]);
            while (goalIndices.Count < requiredCount && goalIndices.Count < candidates.Count)
            {
                var bestIndex = candidates[0];
                var bestScore = float.NegativeInfinity;
                foreach (var candidate in candidates)
                {
                    if (goalIndices.Contains(candidate))
                    {
                        continue;
                    }

                    var nearestDistance = float.PositiveInfinity;
                    foreach (var selected in goalIndices)
                    {
                        var distance = Mathf.Abs(pathTiles[candidate].gridPos.x - pathTiles[selected].gridPos.x)
                            + Mathf.Abs(pathTiles[candidate].gridPos.y - pathTiles[selected].gridPos.y);
                        nearestDistance = Mathf.Min(nearestDistance, distance);
                    }

                    var linePenalty = CalculateLinePenalty(pathTiles[candidate].gridPos, goalIndices, pathTiles);
                    var edgeBias = Mathf.Abs(candidate - pathTiles.Count * 0.5f) * 0.08f;
                    var score = nearestDistance + edgeBias - linePenalty;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIndex = candidate;
                    }
                }

                goalIndices.Add(bestIndex);
            }

            return goalIndices;
        }

        private static float CalculateLinePenalty(Vector2Int candidate, HashSet<int> goalIndices, List<TileData> pathTiles)
        {
            var penalty = 0f;
            foreach (var selected in goalIndices)
            {
                var selectedPosition = pathTiles[selected].gridPos;
                if (selectedPosition.x == candidate.x || selectedPosition.y == candidate.y)
                {
                    penalty += 0.65f;
                }
            }

            var selectedIndices = new List<int>(goalIndices);
            for (var i = 0; i < selectedIndices.Count; i++)
            {
                for (var j = i + 1; j < selectedIndices.Count; j++)
                {
                    var a = pathTiles[selectedIndices[i]].gridPos;
                    var b = pathTiles[selectedIndices[j]].gridPos;
                    var ab = b - a;
                    var ac = candidate - a;
                    if (ab.x * ac.y - ab.y * ac.x == 0)
                    {
                        penalty += 2.75f;
                    }
                }
            }

            return penalty;
        }

        private static void AddColorDecoys(List<TileData> tiles, Dictionary<Vector2Int, TileData> requiredByPosition, int levelIndex, int decoyCount, string floorColorId)
        {
            if (decoyCount <= 0)
            {
                return;
            }

            var colors = new[] { "mint", "slate", "amber", "ocean", "lavender", "coral" };
            var placed = 0;
            var cursor = levelIndex * 7;
            var attempts = 0;
            while (placed < decoyCount && attempts < tiles.Count * 4)
            {
                attempts++;
                cursor = (cursor + 11) % tiles.Count;
                var tile = tiles[cursor];
                if (tile.required || tile.colorId != floorColorId || requiredByPosition.ContainsKey(tile.gridPos))
                {
                    continue;
                }

                tile.id = $"l{levelIndex}-decoy-{placed:00}";
                tile.colorId = colors[(placed + levelIndex) % colors.Length];
                placed++;
            }
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

        private static List<Direction> CreateSerpentinePath(int width, int height, Vector2Int start, bool moveWestFirst, int edgeInset = 0)
        {
            var path = new List<Direction>();
            var position = start;
            var moveTowardWest = moveWestFirst;
            MoveHorizontally(path, ref position, moveTowardWest ? edgeInset : width - 1 - edgeInset);

            while (position.y > 0)
            {
                path.Add(Direction.North);
                position += DirectionToGridOffset(Direction.North);
                moveTowardWest = !moveTowardWest;
                MoveHorizontally(path, ref position, moveTowardWest ? edgeInset : width - 1 - edgeInset);
            }

            return path;
        }

        private static List<Direction> CreateMaskedSerpentinePath(RowSpan[] rowSpans, Vector2Int start, bool moveWestFirst)
        {
            var path = new List<Direction>();
            var position = start;
            var currentDirectionWest = moveWestFirst;

            for (var rowIndex = rowSpans.Length - 1; rowIndex >= 0; rowIndex--)
            {
                var span = rowSpans[rowIndex];
                var targetColumn = currentDirectionWest ? span.startColumn : span.endColumn;
                MoveHorizontally(path, ref position, targetColumn);

                if (rowIndex == 0)
                {
                    break;
                }

                var nextSpan = rowSpans[rowIndex - 1];
                var nextTargetColumn = currentDirectionWest ? nextSpan.endColumn : nextSpan.startColumn;
                var overlapColumn = Mathf.Clamp(position.x, nextSpan.startColumn, nextSpan.endColumn);
                MoveHorizontally(path, ref position, overlapColumn);

                path.Add(Direction.North);
                position += DirectionToGridOffset(Direction.North);

                MoveHorizontally(path, ref position, nextTargetColumn);
                currentDirectionWest = !currentDirectionWest;
            }

            return path;
        }

        private static void MoveHorizontally(List<Direction> path, ref Vector2Int position, int targetColumn)
        {
            while (position.x < targetColumn)
            {
                path.Add(Direction.East);
                position += DirectionToGridOffset(Direction.East);
            }

            while (position.x > targetColumn)
            {
                path.Add(Direction.West);
                position += DirectionToGridOffset(Direction.West);
            }
        }

        private static HashSet<Vector2Int> BuildActiveCells(RowSpan[] rowSpans)
        {
            var activeCells = new HashSet<Vector2Int>();
            for (var row = 0; row < rowSpans.Length; row++)
            {
                for (var col = rowSpans[row].startColumn; col <= rowSpans[row].endColumn; col++)
                {
                    activeCells.Add(new Vector2Int(col, row));
                }
            }

            return activeCells;
        }

        private static Vector2Int GetShapeStart(RowSpan[] rowSpans, bool moveWestFirst)
        {
            var bottomSpan = rowSpans[rowSpans.Length - 1];
            return new Vector2Int(moveWestFirst ? bottomSpan.endColumn : bottomSpan.startColumn, rowSpans.Length - 1);
        }

        private static RowSpan[] CreateBottomAnchoredRowSpans((int start, int end)[] spansFromTopToBottom)
        {
            var rowSpans = new RowSpan[spansFromTopToBottom.Length];
            for (var i = 0; i < spansFromTopToBottom.Length; i++)
            {
                var source = spansFromTopToBottom[spansFromTopToBottom.Length - 1 - i];
                rowSpans[i] = new RowSpan(source.start, source.end);
            }

            return rowSpans;
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

        private static Texture2D LoadMilkyWayBackgroundTexture()
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Scenes/beautiful-shot-of-the-milky-way-constellation-full-2026-03-18-09-25-34-utc.jpeg");
        }

        private static Texture2D LoadArtBackgroundTexture(int artIndex, Texture2D fallbackTexture)
        {
            var artBackgrounds = new List<Texture2D>();
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
            var assetPaths = new List<string>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.IndexOf("/Art/", StringComparison.OrdinalIgnoreCase) < 0
                    && !path.EndsWith("/Art", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                assetPaths.Add(path);
            }

            assetPaths.Sort(StringComparer.Ordinal);
            foreach (var path in assetPaths)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null)
                {
                    artBackgrounds.Add(texture);
                }
            }

            if (artBackgrounds.Count == 0)
            {
                return fallbackTexture;
            }

            return artBackgrounds[Mathf.Abs(artIndex) % artBackgrounds.Count];
        }

        private static void ApplyGroupedArtBackgrounds(List<LevelData> levels, int levelsPerImage)
        {
            if (levels == null || levels.Count == 0 || levelsPerImage <= 0)
            {
                return;
            }

            var artBackgrounds = LoadRootArtJpgTextures();
            if (artBackgrounds.Count == 0)
            {
                return;
            }

            for (var i = 0; i < levels.Count; i++)
            {
                var groupIndex = (i / levelsPerImage) % artBackgrounds.Count;
                levels[i].backgroundTexture = artBackgrounds[groupIndex];
            }
        }

        private static List<Texture2D> LoadRootArtJpgTextures()
        {
            var textures = new List<Texture2D>();
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art" });
            var assetPaths = new List<string>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith("Assets/Art/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var extension = System.IO.Path.GetExtension(path);
                if (!extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                    && !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                assetPaths.Add(path);
            }

            assetPaths.Sort(CompareArtPaths);
            foreach (var path in assetPaths)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null)
                {
                    textures.Add(texture);
                }
            }

            return textures;
        }

        private static int CompareArtPaths(string left, string right)
        {
            var leftName = System.IO.Path.GetFileNameWithoutExtension(left);
            var rightName = System.IO.Path.GetFileNameWithoutExtension(right);

            if (int.TryParse(leftName, out var leftIndex) && int.TryParse(rightName, out var rightIndex))
            {
                return leftIndex.CompareTo(rightIndex);
            }

            return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private readonly struct RowSpan
        {
            public readonly int startColumn;
            public readonly int endColumn;

            public RowSpan(int startColumn, int endColumn)
            {
                this.startColumn = startColumn;
                this.endColumn = endColumn;
            }
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
