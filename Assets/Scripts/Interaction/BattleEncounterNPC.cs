using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Battle;

/// <summary>
/// NPC that triggers a battle encounter when the player collides with its trigger collider.
/// Loads the battle scene additively, passes encounter data via EncounterData.Pending,
/// and handles post-battle cleanup (scene unload, state restore).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BattleEncounterNPC : MonoBehaviour
{
    [Header("=== Encounter Setup ===")]
    [SerializeField] private string _encounterName = "Battle";
    [SerializeField] private List<EnemySpawnData> _enemies = new List<EnemySpawnData>();

    [Header("=== Battle Scene ===")]
    [SerializeField] private string _battleSceneName = "BattleTest";

    [Header("=== NPC Identity ===")]
    [SerializeField] private string _displayName = "Enemy";

    [Header("=== Post-Battle ===")]
    [Tooltip("If true, this NPC is destroyed after being defeated")]
    [SerializeField] private bool _destroyOnDefeat = false;

    private bool _battleInProgress;
    private bool _defeated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_battleInProgress || _defeated) return;

        // Only trigger on the player (not pickups or other triggers)
        var player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        if (_enemies.Count == 0)
        {
            Debug.LogWarning($"[BattleEncounterNPC] {_displayName} has no enemies defined!");
            return;
        }

        StartCoroutine(StartEncounter());
    }

    private IEnumerator StartEncounter()
    {
        _battleInProgress = true;

        // Set pending encounter data
        EncounterData.Pending = new EncounterData
        {
            EncounterName = _encounterName,
            Enemies = new List<EnemySpawnData>(_enemies)
        };

        // Switch to battle state (freezes player movement)
        if (GameManager.Instance != null)
            GameManager.Instance.State = GameManager.GameState.Battle;

        // Subscribe to completion callback
        EncounterData.OnEncounterComplete += HandleEncounterComplete;

        // Load battle scene additively
        var loadOp = SceneManager.LoadSceneAsync(_battleSceneName, LoadSceneMode.Additive);
        if (loadOp == null)
        {
            Debug.LogError($"[BattleEncounterNPC] Failed to load scene '{_battleSceneName}'. Is it in Build Settings?");
            _battleInProgress = false;
            EncounterData.Pending = null;
            EncounterData.OnEncounterComplete -= HandleEncounterComplete;
            if (GameManager.Instance != null)
                GameManager.Instance.State = GameManager.GameState.Playing;
            yield break;
        }

        yield return loadOp;
    }

    private void HandleEncounterComplete(bool playerWon)
    {
        EncounterData.OnEncounterComplete -= HandleEncounterComplete;
        StartCoroutine(CleanupEncounter(playerWon));
    }

    private IEnumerator CleanupEncounter(bool playerWon)
    {
        // Unload battle scene
        var unloadOp = SceneManager.UnloadSceneAsync(_battleSceneName);
        if (unloadOp != null)
            yield return unloadOp;

        // Restore game state
        if (GameManager.Instance != null)
            GameManager.Instance.State = GameManager.GameState.Playing;

        _battleInProgress = false;

        if (playerWon)
        {
            _defeated = true;
            if (_destroyOnDefeat)
                Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        EncounterData.OnEncounterComplete -= HandleEncounterComplete;
    }
}
