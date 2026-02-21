using UnityEngine;

/// <summary>
/// Overworld trigger that starts a battle via <see cref="BattleManager"/>.
/// Supports both interaction (IInteractable) and automatic collision trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BattleEncounter : MonoBehaviour, IInteractable
{
    [SerializeField] EnemyData[] enemies;
    [SerializeField] bool triggerOnCollision;
    [SerializeField] bool destroyAfterBattle = true;

    [SerializeField] string prompt = "Challenge";

    public string InteractionPrompt => prompt;

    bool _used;

    public void Interact(PlayerController player)
    {
        TryStartBattle();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggerOnCollision) return;
        if (other.GetComponent<PlayerController>() != null)
            TryStartBattle();
    }

    void TryStartBattle()
    {
        if (_used) return;
        if (enemies == null || enemies.Length == 0) return;
        if (BattleManager.Instance == null || BattleManager.Instance.IsBattleActive) return;

        _used = true;
        BattleManager.Instance.StartBattle(enemies);

        if (destroyAfterBattle)
            BattleManager.Instance.OnBattleEnd += OnBattleFinished;
        else
            BattleManager.Instance.OnBattleEnd += OnBattleFinishedReusable;
    }

    void OnBattleFinished(bool victory)
    {
        BattleManager.Instance.OnBattleEnd -= OnBattleFinished;
        Destroy(gameObject);
    }

    void OnBattleFinishedReusable(bool victory)
    {
        BattleManager.Instance.OnBattleEnd -= OnBattleFinishedReusable;
        _used = false;
    }
}
