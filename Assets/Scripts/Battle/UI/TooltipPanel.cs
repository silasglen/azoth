using UnityEngine;
using TMPro;

namespace Battle.UI
{
    /// <summary>
    /// Tooltip display panel. Positioned near the trigger element,
    /// clamped to screen bounds so it doesn't go offscreen.
    /// </summary>
    public class TooltipPanel : MonoBehaviour
    {
        [Header("=== UI Elements ===")]
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _bodyText;
        [SerializeField] private RectTransform _rectTransform;

        [Header("=== Offset ===")]
        [SerializeField] private Vector2 _offset = new Vector2(10f, -10f);

        private Canvas _parentCanvas;

        private void Awake()
        {
            _parentCanvas = GetComponentInParent<Canvas>();
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

        public void Show(string title, string body, Vector2 screenPos)
        {
            if (_panelRoot == null) return;

            if (_titleText != null) _titleText.text = title;
            if (_bodyText != null) _bodyText.text = body;

            _panelRoot.SetActive(true);

            // Position near the trigger
            if (_rectTransform != null)
            {
                Vector2 pos = screenPos + _offset;

                // Clamp to screen bounds
                float width = _rectTransform.rect.width;
                float height = _rectTransform.rect.height;

                if (pos.x + width > Screen.width)
                    pos.x = Screen.width - width;
                if (pos.x < 0)
                    pos.x = 0;
                if (pos.y - height < 0)
                    pos.y = height;
                if (pos.y > Screen.height)
                    pos.y = Screen.height;

                _rectTransform.position = pos;
            }
        }

        public void Hide()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }
    }
}
