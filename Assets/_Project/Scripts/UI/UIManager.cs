using DG.Tweening;
using Game.Core;
using Game.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Views (assign in the Inspector)")]
        [SerializeField] private Image sizeBarFill;
        [SerializeField] private CanvasGroup hintGroup;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Transform winPanelContent;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private Transform losePanelContent;

        [Header("Size Bar")]
        [Tooltip("Size bar color once the player is safely far from the critical threshold.")]
        [SerializeField] private Color sizeBarSafeColor = new Color(0.3f, 0.85f, 0.4f);
        [Tooltip("Size bar color as the player approaches the critical threshold.")]
        [SerializeField] private Color sizeBarDangerColor = new Color(0.9f, 0.25f, 0.25f);

        [Header("Hint")]
        [Tooltip("Seconds the hint stays fully visible before it starts fading out.")]
        [SerializeField] private float hintVisibleDuration = 3f;
        [Tooltip("Seconds the hint takes to fade out.")]
        [SerializeField] private float hintFadeDuration = 1f;

        [Header("End Panel Pop-in")]
        [Tooltip("Starting scale for the win/lose panel's pop-in animation (1 = no pop at all).")]
        [SerializeField] private float panelPopInStartScale = 1f;
        [Tooltip("Duration of the win/lose panel's pop-in animation, in seconds.")]
        [SerializeField] private float panelPopInDuration = 0.7f;

        private PlayerBall _player;
        private GameManager _gameManager;
        private float _startSize;
        private float _criticalMinSize;
        
        public void BindViewsForTest(
            Image sizeBarFillRef,
            CanvasGroup hintGroupRef,
            GameObject winPanelRef,
            Transform winPanelContentRef,
            GameObject losePanelRef,
            Transform losePanelContentRef)
        {
            sizeBarFill = sizeBarFillRef;
            hintGroup = hintGroupRef;
            winPanel = winPanelRef;
            winPanelContent = winPanelContentRef;
            losePanel = losePanelRef;
            losePanelContent = losePanelContentRef;
        }

        public void Initialize(GameManager gameManager, PlayerBall player, BalanceSettings balance)
        {
            _gameManager = gameManager;
            _player = player;
            _startSize = player.CurrentSize;
            _criticalMinSize = balance.CriticalMinSize;

            EnsureEventSystem();

            winPanel.SetActive(false);
            losePanel.SetActive(false);

            RefreshSizeBar();
            if (hintGroup != null)
                hintGroup.DOFade(0f, hintFadeDuration).SetDelay(hintVisibleDuration);

            _gameManager.OnWin += ShowWinPanel;
            _gameManager.OnLose += ShowLosePanel;
        }

        private void OnDestroy()
        {
            if (_gameManager == null)
                return;
            _gameManager.OnWin -= ShowWinPanel;
            _gameManager.OnLose -= ShowLosePanel;
        }

        private void Update()
        {
            RefreshSizeBar();
        }
        
        public void RefreshSizeBar()
        {
            if (sizeBarFill == null || _player == null)
                return;

            float startRange = Mathf.Max(0.0001f, _startSize);
            sizeBarFill.fillAmount = Mathf.Clamp01(_player.CurrentSize / startRange);

            float safeRange = Mathf.Max(0.0001f, _startSize - _criticalMinSize);
            float dangerT = Mathf.Clamp01((_player.CurrentSize - _criticalMinSize) / safeRange);
            sizeBarFill.color = Color.Lerp(sizeBarDangerColor, sizeBarSafeColor, dangerT);
        }

        [ContextMenu("Show Win")]
        private void ShowWinPanel()
        {
            winPanel.SetActive(true);
            AnimateIn(winPanelContent);
        }

        [ContextMenu("Show Lose")]
        private void ShowLosePanel()
        {
            losePanel.SetActive(true);
            AnimateIn(losePanelContent);
        }

        private void AnimateIn(Transform content)
        {
            if (content == null)
                return;
            content.localScale = Vector3.one * panelPopInStartScale;
            content.DOScale(1f, panelPopInDuration).SetEase(Ease.OutBack);
        }

        // Wire a Button's OnClick() to this in the Inspector.
        public void Restart()
        {
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
        
        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
        }
    }
}
