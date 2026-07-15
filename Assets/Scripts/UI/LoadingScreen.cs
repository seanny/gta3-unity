using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GTA3Unity.UI
{
    public sealed class LoadingScreen: MonoBehaviour
    {
        public static LoadingScreen Instance { get; private set; }

        [SerializeField] private Image m_Image;

        void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
        }

        public string GetRandomSplashScreen()
        {
            int index = Random.Range(1, 25);
            return "loadsc" + index;
        }

        private Sprite LoadSplash(string splashName)
        {
            var texture = FileLoader.Instance.GetFrontendTexture(splashName);
            var sprite = Sprite.Create(texture, new Rect(0,0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            return sprite;
        }

        public void ShowSplashScreen(string splashName)
        {
            var sprite = LoadSplash(splashName);
            if(sprite == null)
            {
                return;
            }

            m_Image.sprite = sprite;
            m_Image.gameObject.SetActive(true);
        }

        public void HideSplashScreen()
        {
            m_Image.gameObject.SetActive(false);
        }
    }
}