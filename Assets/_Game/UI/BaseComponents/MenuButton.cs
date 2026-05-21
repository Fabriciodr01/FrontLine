using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace FrontLine.UI
{

    public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public enum ButtonType
        {
            StartGame,
            QuitGame,
            ReturnToMenu
        }

        [Header("Button Configuration")]
        [SerializeField] private ButtonType buttonType = ButtonType.StartGame;

        [SerializeField] private string targetSceneName = "BattleScene";
        [SerializeField] private string menuSceneName = "MainMenu";

        [Header("Hover Scale Effect")]
        [SerializeField] private bool enableHoverScale = true;
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float scaleSpeed = 20f;

        private Vector3 originalScale;
        private Vector3 targetScale;

        // Public properties for the editor to check
        public ButtonType Type => buttonType;
        public string TargetSceneName => targetSceneName;
        public string MenuSceneName => menuSceneName;

        void Start()
        {
            originalScale = transform.localScale;
            targetScale = originalScale;
        }

        void Update()
        {
            if (enableHoverScale)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
            }
        }

        public void OnButtonClick()
        {
            switch (buttonType)
            {
                case ButtonType.StartGame:
                    StartGame();
                    break;
                case ButtonType.QuitGame:
                    QuitGame();
                    break;
                case ButtonType.ReturnToMenu:
                    ReturnToMenu();
                    break;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (enableHoverScale)
                targetScale = originalScale * hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (enableHoverScale)
                targetScale = originalScale;
        }

        private void StartGame()
        {
            SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ReturnToMenu()
        {
            SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
        }
    }
}
