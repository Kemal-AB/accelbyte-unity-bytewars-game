using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Debugger
{
    public class DebugConsole : MonoBehaviour
    {

        [SerializeField]
        private DraggableBtn debuggerBtn;
        [SerializeField]
        private Transform container;
        [SerializeField]
        private DebugButtonItem btnPrefab;
        [SerializeField]
        private GameObject logScrollView;
        [SerializeField]
        private Text logText;
        [SerializeField]
        private ContentSizeFitter contentSizeFitter;

        private static DebugConsole _instance = null;

        public static DebugConsole Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<DebugConsole>("DebugConsole");
                    _instance = Instantiate(_instance);
                    DontDestroyOnLoad(_instance.gameObject);
                }
                return _instance;
            }
        }

        internal void Start()
        {
            debuggerBtn.SetClickCallback(ClickDebugger);
            AddButton("close", Close);
            AddButton("destroy debugger", Destroy);
            AddButton("toggle log", SwitchLogVisibility);
            AddButton("clear log", ClearLog);
        }

        private void Destroy()
        {
            Destroy(gameObject);
        }

        private void Close()
        {
            container.gameObject.SetActive(false);
        }

        private void ClickDebugger()
        {
            container.gameObject.SetActive(true);
        }

        private void SwitchLogVisibility()
        {
            logScrollView.SetActive(!logScrollView.activeSelf);
            if(logScrollView.activeSelf)
            {
                Instance.StartCoroutine(waitOneFrame(() => { Instance.contentSizeFitter.enabled = true; }));
            }
        }

        internal void OnEnable()
        {
            Application.logMessageReceived += OnReceivedMsg;
        }

        internal void OnDisable()
        {
            Application.logMessageReceived -= OnReceivedMsg;
        }

        private void OnReceivedMsg(string logString, string stackTrace, LogType type)
        {
            if(type==LogType.Error || type==LogType.Exception)
            {
                Log(logString);
            }
        }

        private void ClearLog()
        {
            logText.text = "";
            contentSizeFitter.enabled = false;
            StartCoroutine(waitOneFrame(() => { Instance.contentSizeFitter.enabled = true; }));
        }

        public static void AddButton(string btnLabel, UnityAction callback)
        {
#if BYTEWARS_DEBUG
            DebugButtonItem localButton = Instantiate(Instance.btnPrefab, Instance.container, false);
            localButton.SetBtn(btnLabel, callback);
            localButton.name = btnLabel;
#endif
        }

        public static void Log(string text)
        {
#if BYTEWARS_DEBUG
            Instance.logText.text += text + '\n';
            Instance.contentSizeFitter.enabled = false;
            Instance.StartCoroutine(waitOneFrame(() => { Instance.contentSizeFitter.enabled = true; }));
            Debug.Log(text);
#endif
        }

        static IEnumerator waitOneFrame(Action callback)
        {
            yield return new WaitForEndOfFrame();
            if(callback!=null)
            {
                callback();
            }
        }
    }
}
