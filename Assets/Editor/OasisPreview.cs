using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class OasisPreview
{
    private const string Scene = "Assets/Scenes/Caravan Stop.unity";
    private const string Request = "StartOasisPreview.request";

    [InitializeOnLoadMethod]
    private static void CheckForPreviewRequest()
    {
        if (File.Exists(Request)) EditorApplication.delayCall += Open;
    }

    public static void Open()
    {
        if (File.Exists(Request)) File.Delete(Request);
        if (File.Exists(Scene)) EditorSceneManager.OpenScene(Scene);
        else OasisBuilder.Build();
        if (File.Exists("PaintOasis.request"))
        {
            File.Delete("PaintOasis.request");
            var terrain = AssetDatabase.LoadAssetAtPath<UnityEngine.TerrainData>("Assets/Art/Terrain/Caravan dunes.asset");
            OasisBuilder.PaintTerrain(terrain);
            EditorSceneManager.SaveOpenScenes();
            UnityEngine.Debug.Log("Oasis terrain blend saved.");
        }
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        EditorApplication.EnterPlaymode();
    }
}

