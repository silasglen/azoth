#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CreateProximityFadeMaterial : MonoBehaviour
{
    [MenuItem("Tools/Create Proximity Fade Material")]
    public static void Create()
    {
        // Find the shader
        Shader shader = Shader.Find("Shader Graphs/TilemapProximityFade");
        if (shader == null)
        {
            EditorUtility.DisplayDialog("Error",
                "Could not find 'Shader Graphs/TilemapProximityFade'. " +
                "Make sure you have created the Shader Graph first and it compiled without errors.", "OK");
            return;
        }

        // Create material
        Material mat = new Material(shader);
        mat.SetFloat("_FadeRadius", 2f);
        mat.SetFloat("_FadedAlpha", 0.3f);
        mat.SetFloat("_EdgeSoftness", 1f);

        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        string path = "Assets/Materials/TilemapProximityFade.mat";
        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Success", $"Material created at {path}", "OK");
        Selection.activeObject = mat;
    }
}
#endif
