using System.Collections;
using System.Collections.Generic;
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

        private IEnumerator FadeCapturedTile(TileRuntime tile)
        {
            if (tile == null || tile.renderer == null)
            {
                yield break;
            }

            tile.visual.transform.localPosition = new Vector3(tile.visual.transform.localPosition.x, -0.04f, tile.visual.transform.localPosition.z);
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
}
