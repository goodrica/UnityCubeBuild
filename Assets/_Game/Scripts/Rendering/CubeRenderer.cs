using System.Collections.Generic;
using ChromaCube.Core;
using ChromaCube.Data;
using UnityEngine;

namespace ChromaCube.Rendering
{
    public class CubeRenderer : MonoBehaviour
    {
        [SerializeField] private float cubeSize = 0.92f;
        [SerializeField] private float faceThickness = 0.035f;

        private readonly Dictionary<string, Material> materialCache = new Dictionary<string, Material>();

        public void Build(LevelData level)
        {
            Clear();
            CreateFace("Top", Vector3.up, new Vector3(cubeSize, faceThickness, cubeSize), level.GetColorForFace(FaceKey.Top));
            CreateFace("Bottom", Vector3.down, new Vector3(cubeSize, faceThickness, cubeSize), level.GetColorForFace(FaceKey.Bottom));
            CreateFace("North", Vector3.forward, new Vector3(cubeSize, cubeSize, faceThickness), level.GetColorForFace(FaceKey.North));
            CreateFace("South", Vector3.back, new Vector3(cubeSize, cubeSize, faceThickness), level.GetColorForFace(FaceKey.South));
            CreateFace("East", Vector3.right, new Vector3(faceThickness, cubeSize, cubeSize), level.GetColorForFace(FaceKey.East));
            CreateFace("West", Vector3.left, new Vector3(faceThickness, cubeSize, cubeSize), level.GetColorForFace(FaceKey.West));
        }

        private void Clear()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        private void CreateFace(string faceName, Vector3 normal, Vector3 scale, string colorId)
        {
            var face = GameObject.CreatePrimitive(PrimitiveType.Cube);
            face.name = $"Face_{faceName}";
            face.transform.SetParent(transform, false);
            face.transform.localPosition = normal * ((cubeSize - faceThickness) * 0.5f);
            face.transform.localRotation = Quaternion.identity;
            face.transform.localScale = scale;
            face.GetComponent<Renderer>().sharedMaterial = GetMaterial(colorId);
        }

        private Material GetMaterial(string colorId)
        {
            if (!materialCache.TryGetValue(colorId, out var material))
            {
                material = ColorPalette.CreateMaterial(colorId);
                materialCache.Add(colorId, material);
            }

            return material;
        }
    }
}
