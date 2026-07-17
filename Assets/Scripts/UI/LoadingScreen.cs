using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GTA3Unity.UI
{
    public sealed class LoadingScreen: MonoBehaviour
    {
        public static LoadingScreen Instance { get; private set; }
        [SerializeField] private Image m_Image;
        [SerializeField] private TextMeshProUGUI m_Text;
        [SerializeField] private Scrollbar m_Scrollbar;

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
        }

        void Start()
        {
            Debug.Assert(m_Scrollbar != null);
        }

        public string GetRandomSplashScreen()
        {
            int index = Random.Range(1, 25);
            return "loadsc" + index;
        }

        private Sprite LoadSplash(string txdFile, string splashName)
        {
            var texture = FileLoader.Instance.GetFrontendTexture(splashName, txdFile);
            var sprite = Sprite.Create(texture, new Rect(0,0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }

        private void SetImage(string splashName, string txdFile)
        {
            if(m_Image == null)
            {
                // Gracefully fail
                return;
            }

            var sprite = LoadSplash(txdFile, splashName);
            if(sprite == null)
            {
                return;
            }

            m_Image.sprite = sprite;
            m_Image.gameObject.SetActive(true);
        }

        private void HideImage()
        {
            if(m_Image == null)
            {
                // Gracefully fail
                return;
            }

            m_Image.gameObject.SetActive(false);
        }

        public void SetSplashText(string text)
        {
            if(m_Text == null)
            {
                // Gracefully fail
                return;
            }
            // TODO: Add GXT loading
            m_Text.text = text;
        }

        public void HideSplashText()
        {
            if(m_Text == null)
            {
                // Gracefully fail
                return;
            }

            m_Text.text = string.Empty;
        }

        public void ShowSplashScreen(string splashName, string txdFile = null)
        {
            if(txdFile == null)
            {
                txdFile = splashName;
            }

            SetImage(splashName, txdFile);
        }

        public void HideSplashScreen()
        {
            HideImage();
        }

        public void HideProgressBar()
        {
            if(m_Scrollbar == null)
            {
                return;
            }

            m_Scrollbar.size = 0;
            m_Scrollbar.gameObject.SetActive(false);
        }

        public void ShowProgressBar(int loaded, int maxCount)
        {
            if(m_Scrollbar == null)
            {
                return;
            }

            // maxCount needs to be (float) 
            float progress = loaded / (float)maxCount;
            m_Scrollbar.gameObject.SetActive(true);
            m_Scrollbar.size = progress;
        }
    }
}