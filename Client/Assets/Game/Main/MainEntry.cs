using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class MainEntry : MonoBehaviour
    {
        /// <summary>
        /// ��־����
        /// </summary>
        public enum LogCategory
        {
            /// <summary>
            /// �����־
            /// </summary>
            Framework,
            /// <summary>
            /// ����
            /// </summary>
            Procedure,
            /// <summary>
            /// ��Դ����
            /// </summary>
            Resource,
            /// <summary>
            /// ������Ϣ
            /// </summary>
            NetWork
        }

        //ȫ�ֲ�������
        [FoldoutGroup("ParamsSettings")]
        [SerializeField]
        private ParamsSettings m_ParamsSettings;
        public static ParamsSettings ParamsSettings { get; private set; }

        //��ǰ�豸�ȼ�
        [FoldoutGroup("ParamsSettings")]
        [SerializeField]
        private ParamsSettings.DeviceGrade m_CurrDeviceGrade;
        public static ParamsSettings.DeviceGrade CurrDeviceGrade { get; private set; }


        /// <summary>
        /// ���ع�����
        /// </summary>
        public static DownloadManager Download { get; private set; }
        /// <summary>
        /// ��Դ������
        /// </summary>
        public static ResourceManager ResourceManager { get; private set; }
        /// <summary>
        /// ������
        /// </summary>
        public static ClassObjectPool ClassObjectPool { get; private set; }
        /// <summary>
        /// ϵͳ���ݹ�����
        /// </summary>
        public static SysDataMgr SysData { get; private set; }
        /// <summary>
        /// �ȸ��¹�����
        /// </summary>
        public static HotfixManager Hotfix { get; private set; }


        /// <summary>
        /// ����
        /// </summary>
        public static MainEntry Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            //��Ļ����
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            //�˴��Ժ��ж�������Ǳ༭��ģʽ Ҫ�����豸��Ϣ�жϵȼ�
            CurrDeviceGrade = m_CurrDeviceGrade;
            ParamsSettings = m_ParamsSettings;
        }
        private void Start()
        {
            Download = new DownloadManager();
            ResourceManager = new ResourceManager();
            ClassObjectPool = new ClassObjectPool();
            SysData = new SysDataMgr();
            Hotfix = new HotfixManager();

            Download.Init();
            ResourceManager.Init();
            Hotfix.Init();
        }
        private void Update()
        {
            Download.OnUpdate();
        }
        private void OnApplicationQuit()
        {
            Download.Dispose();
        }

        public static void Log(LogCategory catetory, object message, params object[] args)
        {
#if DEBUG_LOG_NORMAL
            string value = string.Empty;
            if (args.Length == 0)
            {
                value = message.ToString();
            }
            else
            {
                value = string.Format(message.ToString(), args);
            }
            Debug.Log(string.Format("youyouLog=={0}=={1}", catetory.ToString(), value));
#endif
        }

        public static void LogWarning(LogCategory catetory, object message, params object[] args)
        {
#if DEBUG_LOG_WARNING
            string value = string.Empty;
            if (args.Length == 0)
            {
                value = message.ToString();
            }
            else
            {
                value = string.Format(message.ToString(), args);
            }
            Debug.LogWarning(string.Format("youyouLog=={0}=={1}", catetory.ToString(), value));
#endif
        }

        public static void LogError(LogCategory catetory, object message, params object[] args)
        {
#if DEBUG_LOG_ERROR
            string value = string.Empty;
            if (args.Length == 0)
            {
                value = message.ToString();
            }
            else
            {
                value = string.Format(message.ToString(), args);
            }
            Debug.LogError(string.Format("youyouLog=={0}=={1}", catetory.ToString(), value));
#endif
        }

    }
}