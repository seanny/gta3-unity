using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

namespace GTA3Unity.Core
{
    public enum EGameState
    {
        Startup,
        Logo,
        Intro,
        InitialiseOnce,
        InitialiseFrontend
    }

    public sealed class GameManager: MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public string GtaDirectory
        {
            get
            {
                return m_GtaDirectory;
            }
        }

        [SerializeField]
        private string m_GtaDirectory;

        [SerializeField] private VideoPlayer m_VideoPlayer;
        [SerializeField] private string m_Logo;
        [SerializeField] private string m_Intro;

        [SerializeField] private EGameState m_GameState;
        private bool m_IsInit;
        private bool m_IntroFailed;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            if(!Directory.Exists(m_GtaDirectory))
            {
                Debug.LogError($"Cannot access \"{m_GtaDirectory}\": Does not exist");
            }
        }

        private void OnVideoEnded(VideoPlayer source)
        {
            m_GameState++;
            if(m_GameState > EGameState.Intro)
            {
                return;
            }

            try
            {
                m_VideoPlayer.url = @"file:///" + m_Intro;
                m_VideoPlayer.frame = 0; // Is this neccesary?
                m_VideoPlayer.Play();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void Update()
        {
            switch(m_GameState)
            {
                case EGameState.Startup:
                    if(m_IsInit == false)
                    {
                        GameManager.Instance.Init(m_GtaDirectory);
                        m_GameState++;
                    }
                    break;
                case EGameState.Logo:
                case EGameState.Intro:
                    IntroUpdate();
                    break;
                case EGameState.InitialiseOnce:
                    FileLoader.Instance.Init();
                    m_GameState++;
                    break;
            }
        }

        private void IntroUpdate()
        {
            if(Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.Space))
            {
                m_GameState++;
                m_VideoPlayer.Stop();
                OnVideoEnded(m_VideoPlayer);
            }
        }

        public void Init(string gtaRoot)
        {
            m_IsInit = true;
            m_GameState = EGameState.Startup;
            m_Logo = Path.Combine(gtaRoot, "movies", "Logo.mpg").Replace(" ", "%20").Replace("\\", "/");
            m_Intro = Path.Combine(gtaRoot, "movies", "GTAtitles.mpg").Replace(" ", "%20").Replace("\\", "/");

            if(m_VideoPlayer == null)
            {
                m_VideoPlayer = GameObject.FindAnyObjectByType<VideoPlayer>();
                Debug.Assert(m_VideoPlayer != null);
            }

            m_VideoPlayer.url = @"file:///" + m_Logo;
            m_VideoPlayer.loopPointReached += OnVideoEnded;
            m_VideoPlayer.errorReceived += OnPlaybackError;
            m_VideoPlayer.Play();
        }

        private void OnPlaybackError(VideoPlayer source, string message)
        {
            m_GameState = EGameState.InitialiseOnce;
        }
    }
}