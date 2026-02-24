using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Battle.UI
{
    /// <summary>
    /// Attach to any UI element to show a tooltip on hover.
    /// Implements IPointerEnterHandler/ExitHandler for mouse,
    /// with a delayed show (0.3s) to avoid flickering.
    /// </summary>
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public enum TooltipType
        {
            Skill,
            Item,
            StatusEffect,
            Element,
            Enemy
        }

        [SerializeField] private TooltipPanel _tooltipPanel;
        [SerializeField] private float _showDelay = 0.3f;

        private string _title;
        private string _body;
        private Coroutine _showCoroutine;

        public void SetData(string title, string body, TooltipPanel panel)
        {
            _title = title;
            _body = body;
            _tooltipPanel = panel;
        }

        public void SetSkillData(SkillDefinition skill, IBattleUnit caster, TooltipPanel panel)
        {
            _tooltipPanel = panel;
            (_title, _body) = TooltipDataProvider.GetSkillTooltip(skill, caster);
        }

        public void SetItemData(ItemDefinition item, TooltipPanel panel)
        {
            _tooltipPanel = panel;
            (_title, _body) = TooltipDataProvider.GetItemTooltip(item);
        }

        public void SetEnemyData(IBattleUnit enemy, TooltipPanel panel)
        {
            _tooltipPanel = panel;
            (_title, _body) = TooltipDataProvider.GetEnemyTooltip(enemy);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tooltipPanel == null) return;
            if (_showCoroutine != null)
                StopCoroutine(_showCoroutine);
            _showCoroutine = StartCoroutine(DelayedShow(eventData.position));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
                _showCoroutine = null;
            }
            if (_tooltipPanel != null)
                _tooltipPanel.Hide();
        }

        private IEnumerator DelayedShow(Vector2 position)
        {
            yield return new WaitForSecondsRealtime(_showDelay);
            _tooltipPanel.Show(_title, _body, position);
            _showCoroutine = null;
        }

        private void OnDisable()
        {
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
                _showCoroutine = null;
            }
            if (_tooltipPanel != null)
                _tooltipPanel.Hide();
        }
    }
}
