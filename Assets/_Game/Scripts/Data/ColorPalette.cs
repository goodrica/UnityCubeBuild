using System.Collections.Generic;
using UnityEngine;

namespace ChromaCube.Data
{
    public static class ColorPalette
    {
        private static readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>
        {
            { "stone", new Color(0.82f, 0.84f, 0.86f) },
            { "poliigon_floor", new Color(0.78f, 0.78f, 0.74f) },
            { "slate", new Color(0.00f, 0.88f, 0.95f) },
            { "mint", new Color(0.20f, 0.96f, 0.62f) },
            { "amber", new Color(1.00f, 0.82f, 0.18f) },
            { "ocean", new Color(0.04f, 0.18f, 0.92f) },
            { "lavender", new Color(0.76f, 0.48f, 1.00f) },
            { "coral", new Color(1.00f, 0.36f, 0.42f) },
            { "captured", new Color(0.60f, 0.66f, 0.70f) },
            { "void", new Color(0.08f, 0.11f, 0.14f) }
        };

        public static Color Get(string colorId)
        {
            return Colors.TryGetValue(colorId, out var color) ? color : Color.magenta;
        }

        public static Material CreateMaterial(string colorId)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader);
            material.name = $"MAT_{colorId}";
            material.color = Get(colorId);
            material.SetColor("_BaseColor", Get(colorId));
            material.SetFloat("_Smoothness", 0.58f);
            return material;
        }
    }
}
