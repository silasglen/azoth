using System;
using UnityEngine;

/// <summary>
/// Lightweight singleton that tracks top-level game state and provides a
/// central reference to the player.  Other scripts can check
/// <see cref="State"/> to decide whether input/AI should be active.
/// </summary>
public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, Paused, Dialogue, Battle }

    public static GameManager Instance { get; private set; }

    /// <summary>Fires whenever <see cref="State"/> changes. Args: (old, new).</summary>
    public event Action<GameState, GameState> OnStateChanged;

    [field: SerializeField] public PlayerController Player { get; private set; }

    GameState _state = GameState.Playing;
    public GameState State
    {
        get => _state;
        set
        {
            if (_state == value) return;
            var old = _state;
            _state = value;
            Time.timeScale = (_state == GameState.Playing || _state == GameState.Battle) ? 1f : 0f;
            OnStateChanged?.Invoke(old, _state);
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
