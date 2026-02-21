using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Editor utility that creates the Player Animator Controller with blend trees
/// for 4-directional idle/walk using multi-frame sprite sheet animation clips.
/// </summary>
public static class PlayerAnimatorBuilder
{
    const string SpriteFolder = "Assets/Sprites/Character Sprites";
    const string OutputFolder = "Assets/Animations/Player";
    const string ControllerName = "PlayerAnimator";
    const int FrameRate = 8;

    [MenuItem("Tools/Build Player Animator")]
    public static void Build()
    {
        // Ensure output folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder("Assets/Animations", "Player");

        // Create animation clips from sprite sheets (multiple sub-sprites per PNG)
        var idleFront = CreateSpriteSheetClip("Idle_Front", $"{SpriteFolder}/MainC_Idle_Front.PNG");
        var idleBack  = CreateSpriteSheetClip("Idle_Back",  $"{SpriteFolder}/MainC_Idle_Back.PNG");
        var idleLeft  = CreateSpriteSheetClip("Idle_Left",  $"{SpriteFolder}/MainC_Idle_Left.PNG");
        var idleRight = CreateSpriteSheetClip("Idle_Right", $"{SpriteFolder}/MainC_Idle_Right.PNG");

        var walkFront = CreateSpriteSheetClip("Walk_Front", $"{SpriteFolder}/MainC_Walk_Front.PNG");
        var walkBack  = CreateSpriteSheetClip("Walk_Back",  $"{SpriteFolder}/MainC_Walk_Back.PNG");
        var walkLeft  = CreateSpriteSheetClip("Walk_Left",  $"{SpriteFolder}/MainC_Walk_Left.PNG");
        var walkRight = CreateSpriteSheetClip("Walk_Right", $"{SpriteFolder}/MainC_Walk_Right.PNG");

        // Delete old controller and create fresh
        string controllerPath = $"{OutputFolder}/{ControllerName}.controller";
        AssetDatabase.DeleteAsset(controllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Add parameters
        controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
        controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);

        var rootStateMachine = controller.layers[0].stateMachine;

        // --- Idle Blend Tree ---
        var idleState = controller.CreateBlendTreeInController("Idle", out var idleTree, 0);
        idleTree.blendType = BlendTreeType.SimpleDirectional2D;
        idleTree.blendParameter = "MoveX";
        idleTree.blendParameterY = "MoveY";
        idleTree.AddChild(idleFront, new Vector2( 0, -1)); // down
        idleTree.AddChild(idleBack,  new Vector2( 0,  1)); // up
        idleTree.AddChild(idleLeft,  new Vector2(-1,  0)); // left
        idleTree.AddChild(idleRight, new Vector2( 1,  0)); // right

        // --- Walk Blend Tree ---
        var walkState = controller.CreateBlendTreeInController("Walk", out var walkTree, 0);
        walkTree.blendType = BlendTreeType.SimpleDirectional2D;
        walkTree.blendParameter = "MoveX";
        walkTree.blendParameterY = "MoveY";
        walkTree.AddChild(walkFront, new Vector2( 0, -1));
        walkTree.AddChild(walkBack,  new Vector2( 0,  1));
        walkTree.AddChild(walkLeft,  new Vector2(-1,  0));
        walkTree.AddChild(walkRight, new Vector2( 1,  0));

        // Set Idle as default
        rootStateMachine.defaultState = idleState;

        // --- Transitions ---
        // Idle -> Walk when IsMoving = true
        var toWalk = idleState.AddTransition(walkState);
        toWalk.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
        toWalk.hasExitTime = false;
        toWalk.duration = 0f;

        // Walk -> Idle when IsMoving = false
        var toIdle = walkState.AddTransition(idleState);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
        toIdle.hasExitTime = false;
        toIdle.duration = 0f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Player Animator Controller created at {controllerPath}");
    }

    static AnimationClip CreateSpriteSheetClip(string clipName, string sheetPath)
    {
        // Load ALL assets at the path and filter for Sprite sub-assets
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(sheetPath);
        var sprites = allAssets
            .OfType<Sprite>()
            .OrderBy(s => s.name) // _0, _1, _2, ... sorts correctly
            .ToList();

        if (sprites.Count == 0)
        {
            Debug.LogError($"No sprites found in sheet at {sheetPath}");
            return new AnimationClip { name = clipName };
        }

        Debug.Log($"{clipName}: found {sprites.Count} frames in {sheetPath}");

        var clip = new AnimationClip
        {
            name = clipName,
            frameRate = FrameRate
        };

        // One keyframe per frame in the sprite sheet
        var keyframes = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / (float)FrameRate,
                value = sprites[i]
            };
        }

        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        // Enable looping
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // Save as asset (overwrite if exists)
        string clipPath = $"{OutputFolder}/{clipName}.anim";
        AssetDatabase.DeleteAsset(clipPath);
        AssetDatabase.CreateAsset(clip, clipPath);
        return clip;
    }
}
