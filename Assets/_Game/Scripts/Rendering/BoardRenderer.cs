using System.Collections;
using System.Collections.Generic;
using ChromaCube.Core;
using ChromaCube.Data;
using ChromaCube.Level;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ChromaCube.Rendering
{
    public class BoardRenderer : MonoBehaviour
    {
        private const string TileTextureFolder = "Assets/_Game/Textures/Tiles";

        [SerializeField] private float tileSize = 1.12f;
        [SerializeField] private float tileHeight = 0.12f;
        [SerializeField] private float capturedFadeDelay = 3f;
        [SerializeField] private float capturedFadeDuration = 1.1f;

        private readonly List<TileRuntime> runtimeTiles = new List<TileRuntime>();
        private readonly Dictionary<string, Material> materialCache = new Dictionary<string, Material>();
        private readonly Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

        public IReadOnlyList<TileRuntime> RuntimeTiles => runtimeTiles;
        public float TileSize => tileSize;

        public void Clear()
        {
            StopAllCoroutines();

            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            runtimeTiles.Clear();
        }

        public void Render(LevelData level)
        {
            Clear();
            DiscoverTexturesInEditor();

            if (level.mechanicsMode == MechanicsMode.WorldCube)
            {
                RenderWorldCube(level);
                return;
            }

            foreach (var tile in level.tiles)
            {
                if (!tile.active)
                {
                    continue;
                }

                var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.name = tile.required ? $"GoalTile_{tile.id}" : $"Tile_{tile.id}";
                visual.transform.SetParent(transform, false);
                visual.transform.position = GridToWorld(tile.gridPos, level);
                visual.transform.localScale = new Vector3(tileSize * 0.94f, tileHeight, tileSize * 0.94f);

                var renderer = visual.GetComponent<Renderer>();
                renderer.sharedMaterial = GetMaterial(tile.colorId);

                runtimeTiles.Add(new TileRuntime
                {
                    data = tile,
                    visual = visual,
                    renderer = renderer
                });
            }
        }

        public void RefreshCapturedState()
        {
            foreach (var tile in runtimeTiles)
            {
                if (!tile.data.captured)
                {
                    tile.renderer.sharedMaterial = GetMaterial(tile.data.colorId);
                    continue;
                }

                StartCoroutine(FadeCapturedTile(tile));
            }
        }

        public Vector3 GridToWorld(Vector2Int gridPos, LevelData level)
        {
            var centeredX = gridPos.x - (level.width - 1) * 0.5f;
            var centeredZ = (level.height - 1) * 0.5f - gridPos.y;
            return new Vector3(centeredX * tileSize, 0f, centeredZ * tileSize);
        }

        public Vector3 WorldCubeTileCenter(WorldCubeFace face, Vector2Int gridPos, LevelData level)
        {
            var frame = GetWorldCubeFrame(face);
            return WorldCubeTileCenter(frame, gridPos, level);
        }

        public Quaternion WorldCubeSurfaceRotation(WorldCubeFace face)
        {
            var frame = GetWorldCubeFrame(face);
            return Quaternion.LookRotation(frame.forward, frame.normal);
        }

        public static WorldCubeFace FaceFromNormal(Vector3 normal)
        {
            normal.Normalize();
            if (Vector3.Dot(normal, Vector3.up) > 0.9f)
            {
                return WorldCubeFace.Top;
            }

            if (Vector3.Dot(normal, Vector3.down) > 0.9f)
            {
                return WorldCubeFace.Bottom;
            }

            if (Vector3.Dot(normal, Vector3.forward) > 0.9f)
            {
                return WorldCubeFace.North;
            }

            if (Vector3.Dot(normal, Vector3.back) > 0.9f)
            {
                return WorldCubeFace.South;
            }

            if (Vector3.Dot(normal, Vector3.right) > 0.9f)
            {
                return WorldCubeFace.East;
            }

            return WorldCubeFace.West;
        }

        public static WorldCubeFrame GetWorldCubeFrame(WorldCubeFace face)
        {
            switch (face)
            {
                case WorldCubeFace.Top:
                    return new WorldCubeFrame(Vector3.up, Vector3.right, Vector3.forward);
                case WorldCubeFace.Bottom:
                    return new WorldCubeFrame(Vector3.down, Vector3.right, Vector3.back);
                case WorldCubeFace.North:
                    return new WorldCubeFrame(Vector3.forward, Vector3.right, Vector3.down);
                case WorldCubeFace.South:
                    return new WorldCubeFrame(Vector3.back, Vector3.right, Vector3.up);
                case WorldCubeFace.East:
                    return new WorldCubeFrame(Vector3.right, Vector3.down, Vector3.forward);
                case WorldCubeFace.West:
                    return new WorldCubeFrame(Vector3.left, Vector3.up, Vector3.forward);
                default:
                    return new WorldCubeFrame(Vector3.up, Vector3.right, Vector3.forward);
            }
        }

        private void RenderWorldCube(LevelData level)
        {
            foreach (var tile in level.tiles)
            {
                if (!tile.active)
                {
                    continue;
                }

                var frame = GetWorldCubeFrame(tile.worldFace);
                var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visual.name = tile.required ? $"GoalTile_{tile.worldFace}_{tile.id}" : $"Tile_{tile.worldFace}_{tile.id}";
                visual.transform.SetParent(transform, false);
                visual.transform.position = WorldCubeTileCenter(frame, tile.gridPos, level);
                visual.transform.rotation = Quaternion.LookRotation(frame.forward, frame.normal);
                visual.transform.localScale = new Vector3(tileSize * 0.9f, tileHeight, tileSize * 0.9f);

                var renderer = visual.GetComponent<Renderer>();
                renderer.sharedMaterial = GetMaterial(tile.colorId);

                runtimeTiles.Add(new TileRuntime
                {
                    data = tile,
                    visual = visual,
                    renderer = renderer
                });
            }
        }

        private Vector3 WorldCubeTileCenter(WorldCubeFrame frame, Vector2Int gridPos, LevelData level)
        {
            var size = Mathf.Max(level.width, level.height);
            var half = (size - 1) * 0.5f;
            var shellRadius = size * tileSize * 0.5f;
            var rightOffset = (gridPos.x - half) * tileSize;
            var forwardOffset = (half - gridPos.y) * tileSize;
            return frame.normal * shellRadius + frame.right * rightOffset + frame.forward * forwardOffset;
        }

        private IEnumerator FadeCapturedTile(TileRuntime tile)
        {
            if (tile == null || tile.renderer == null)
            {
                yield break;
            }

            var frame = GetWorldCubeFrame(tile.data.worldFace);
            tile.visual.transform.position -= frame.normal * 0.04f;
            yield return new WaitForSeconds(capturedFadeDelay);

            var material = new Material(GetMaterial(tile.data.colorId));
            tile.renderer.sharedMaterial = material;

            var startColor = ColorPalette.Get(tile.data.colorId);
            var endColor = ColorPalette.Get("captured");
            var elapsed = 0f;

            while (elapsed < capturedFadeDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / capturedFadeDuration);
                var color = Color.Lerp(startColor, endColor, Mathf.SmoothStep(0f, 1f, t));
                material.color = color;
                material.SetColor("_BaseColor", color);
                yield return null;
            }

            material.color = endColor;
            material.SetColor("_BaseColor", endColor);
        }

        private Material GetMaterial(string colorId)
        {
            if (!materialCache.TryGetValue(colorId, out var material))
            {
                material = ColorPalette.CreateMaterial(colorId);
                ApplyTileTexture(material, colorId);
                materialCache.Add(colorId, material);
            }

            return material;
        }

        private void ApplyTileTexture(Material material, string colorId)
        {
            if (textureCache.TryGetValue(colorId.ToLowerInvariant(), out var texture))
            {
                material.mainTexture = texture;
                material.SetTexture("_BaseMap", texture);
            }
            else if (textureCache.TryGetValue("floor", out var floorTexture))
            {
                material.mainTexture = floorTexture;
                material.SetTexture("_BaseMap", floorTexture);
            }
        }

        private void DiscoverTexturesInEditor()
        {
#if UNITY_EDITOR
            textureCache.Clear();

            if (!AssetDatabase.IsValidFolder(TileTextureFolder))
            {
                return;
            }

            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { TileTextureFolder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null)
                {
                    continue;
                }

                textureCache[texture.name.ToLowerInvariant()] = texture;
            }
#endif
        }
    }

    public readonly struct WorldCubeFrame
    {
        public readonly Vector3 normal;
        public readonly Vector3 right;
        public readonly Vector3 forward;

        public WorldCubeFrame(Vector3 normal, Vector3 right, Vector3 forward)
        {
            this.normal = normal.normalized;
            this.right = right.normalized;
            this.forward = forward.normalized;
        }
    }
}
