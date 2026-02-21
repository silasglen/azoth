using System.Linq;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Populates the PlayerAnimator sprite arrays from the sprite sheets.
/// Run via Tools > Setup Player Animator Sprites.
/// </summary>
public static class PlayerAnimatorSetup
{
    const string SpriteFolder = "Assets/Sprites/Character Sprites";

    [MenuItem("Tools/Setup Player Animator Sprites")]
    public static void Setup()
    {
        var animator = Object.FindFirstObjectByType<PlayerAnimator>();
        if (animator == null)
        {
            Debug.LogError("No PlayerAnimator found in the scene.");
            return;
        }

        var so = new SerializedObject(animator);

        SetSpriteArray(so, "idleFront", $"{SpriteFolder}/MainC_Idle_Front.PNG");
        SetSpriteArray(so, "idleBack",  $"{SpriteFolder}/MainC_Idle_Back.PNG");
        SetSpriteArray(so, "idleLeft",  $"{SpriteFolder}/MainC_Idle_Left.PNG");
        SetSpriteArray(so, "idleRight", $"{SpriteFolder}/MainC_Idle_Right.PNG");

        SetSpriteArray(so, "walkFront", $"{SpriteFolder}/MainC_Walk_Front.PNG");
        SetSpriteArray(so, "walkBack",  $"{SpriteFolder}/MainC_Walk_Back.PNG");
        SetSpriteArray(so, "walkLeft",  $"{SpriteFolder}/MainC_Walk_Left.PNG");
        SetSpriteArray(so, "walkRight", $"{SpriteFolder}/MainC_Walk_Right.PNG");

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(animator);

        Debug.Log("PlayerAnimator sprites populated successfully.");
    }

    static void SetSpriteArray(SerializedObject so, string propertyName, string sheetPath)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(sheetPath)
            .OfType<Sprite>()
            .OrderBy(s => s.name)
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError($"No sprites found at {sheetPath}");
            return;
        }

        var prop = so.FindProperty(propertyName);
        prop.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];

        Debug.Log($"  {propertyName}: {sprites.Length} frames from {sheetPath}");
    }
}
