using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GTA3Unity.Core;
using System.IO;

namespace GTA3Unity.UI
{
    public sealed class StartupScreen: MonoBehaviour
    {
        [SerializeField] private GameManager m_GameManager;
        [SerializeField] private GameObject m_StartupScreen;
        [SerializeField] private TMP_InputField m_GtaDirectoryInputField;
        [SerializeField] private TextMeshProUGUI m_ErrorText;


        private string m_GtaDirectory;

        private void Start()
        {
            Debug.Assert(m_GameManager != null);
            Debug.Assert(m_StartupScreen != null);
            Debug.Assert(m_GtaDirectoryInputField != null);
            Debug.Assert(m_ErrorText != null);

            string settingsPath = Path.Combine(Application.persistentDataPath, "settings.cfg");
            if(!File.Exists(settingsPath))
            {
                return;
            }

            m_GtaDirectory = File.ReadAllText(settingsPath);
            m_GtaDirectoryInputField.text = m_GtaDirectory;
        }

        private void SetError(string error)
        {
            m_ErrorText.gameObject.SetActive(true);
            m_ErrorText.text = $"<color=red>ERROR: {error}</color>";
        }

        public void OnValueChanged()
        {
            m_GtaDirectory = m_GtaDirectoryInputField.text;
        }

        public void OnLaunch()
        {
            if(string.IsNullOrEmpty(m_GtaDirectory))
            {
                SetError("You must input a valid GTA3 path");
                return;
            }

            // Basic validation to ensure its a correct GTA path by checking if we can find models/gta3.img
            string gta3ImgPath = Path.Combine(m_GtaDirectory, "models", "gta3.img");
            if(!File.Exists(gta3ImgPath))
            {
                SetError("The path you provided does not appear to be a valid GTA3 path.");
                return;
            }

            string settingsPath = Path.Combine(Application.persistentDataPath, "settings.cfg");
            Debug.Log($"Wrote to {settingsPath}");
            File.WriteAllText(settingsPath, m_GtaDirectory);
            m_StartupScreen.SetActive(false);
            m_GameManager.enabled = true;
            m_GameManager.StartGta(m_GtaDirectory);
        }
    }
}