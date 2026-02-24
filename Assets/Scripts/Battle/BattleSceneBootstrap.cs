using System.Collections.Generic;
using UnityEngine;
using Battle.UI;

namespace Battle
{
    /// <summary>
    /// Drop onto the BattleController GameObject in a test scene.
    /// Priority order:
    /// 1. If EncounterData.Pending is set (from overworld NPC), spawns those enemies and starts.
    /// 2. If a BattleSetupUI is present, shows the setup panel and waits for user selection.
    /// 3. Otherwise, starts the battle automatically using Inspector-assigned combatants.
    /// </summary>
    public class BattleSceneBootstrap : MonoBehaviour
    {
        [SerializeField] private BattleController _battleController;
        [SerializeField] private BattleSetupUI _setupUI;

        private GameObject _encounterEnemyContainer;

        private void Start()
        {
            if (_battleController == null)
            {
                Debug.LogError("[BattleSceneBootstrap] BattleController not assigned!");
                return;
            }

            // 1. Check for pending encounter from overworld NPC
            if (EncounterData.Pending != null)
            {
                StartEncounterBattle(EncounterData.Pending);
                EncounterData.Pending = null;
                return;
            }

            // 2. If setup UI exists, let the user pick enemies first
            if (_setupUI != null)
            {
                _setupUI.ShowPanel();
                return;
            }

            // 3. No setup UI — auto-start with Inspector-assigned combatants
            _battleController.StartBattle();
        }

        private void StartEncounterBattle(EncounterData encounter)
        {
            // Disable setup UI during encounter battles (prevents it from re-showing after battle)
            if (_setupUI != null)
                _setupUI.enabled = false;

            // Create enemy GameObjects from encounter data
            _encounterEnemyContainer = new GameObject("EncounterEnemies");
            var enemies = new List<IBattleUnit>();

            foreach (var spawnData in encounter.Enemies)
            {
                var go = new GameObject(spawnData.Name);
                go.transform.SetParent(_encounterEnemyContainer.transform);

                var aiType = EncounterData.GetAIType(spawnData.AIPattern);
                var ai = go.AddComponent(aiType) as EnemyAIBase;
                if (ai != null)
                {
                    ai.Configure(spawnData.Name, UnitType.Knight, spawnData.Element,
                        spawnData.HP, spawnData.ATK, spawnData.DEF, 0.1f, 0, _battleController);
                    enemies.Add(ai);
                }
            }

            if (enemies.Count == 0)
            {
                Debug.LogError("[BattleSceneBootstrap] No valid enemies in encounter data!");
                Destroy(_encounterEnemyContainer);
                return;
            }

            // Subscribe to battle end to notify the encounter system
            _battleController.OnBattleWon += HandleEncounterWon;
            _battleController.OnBattleLost += HandleEncounterLost;

            // Start battle (null players = use Inspector-assigned player units)
            _battleController.InitBattle(null, enemies);
        }

        private void HandleEncounterWon()
        {
            CleanupEncounter();
            EncounterData.NotifyComplete(true);
        }

        private void HandleEncounterLost()
        {
            CleanupEncounter();
            EncounterData.NotifyComplete(false);
        }

        private void CleanupEncounter()
        {
            _battleController.OnBattleWon -= HandleEncounterWon;
            _battleController.OnBattleLost -= HandleEncounterLost;

            if (_encounterEnemyContainer != null)
                Destroy(_encounterEnemyContainer);
        }
    }
}
