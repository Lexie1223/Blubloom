using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public static class OceanBackgroundSetup
{
    [MenuItem("Tools/Ocean Background/Create Menu Background")]
    public static void Create()
    {
        const string root = "Assets/OceanStartBackground/Textures/";
        string[] names = { "01_Clean", "02_LightPollution", "03_Polluted", "04_HeavyPollution" };
        var textures = new Texture2D[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            string path = root + names[i] + ".jpeg";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogError("Missing texture: " + path); return; }
            importer.textureType = TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 4096;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            textures[i] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        var go = new GameObject("Ocean Background Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Undo.RegisterCreatedObjectUndo(go, "Create Ocean Background");
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -100;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        var panel = new GameObject("Animated Ocean", typeof(RectTransform));
        panel.transform.SetParent(go.transform, false);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        panel.AddComponent<OceanMenuBackground>().backgrounds = textures;
        Selection.activeGameObject = panel;
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
    }
}
