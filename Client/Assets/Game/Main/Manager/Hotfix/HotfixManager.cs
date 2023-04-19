using HybridCLR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class HotfixManager
    {
        private static AssetBundle hotfixAb;

        public void Init()
        {
            //�����ֹ�ȸ������Ҳ���AOT���̵���
            System.Data.AcceptRejectRule acceptRejectRule = System.Data.AcceptRejectRule.None;
            System.Net.WebSockets.WebSocketReceiveResult webSocketReceiveResult = null;

            MainEntry.ResourceManager.CheckVersionComplete = () =>
            {
                MainEntry.Download.BeginDownloadSingle(YFConstDefine.HotfixAssetBundlePath, onComplete: (string fileUrl) =>
                {
                    Debug.LogError(fileUrl);

                    //�����ȸ�����
                    hotfixAb = AssetBundle.LoadFromFile(string.Format("{0}/{1}", Application.persistentDataPath, fileUrl));
                    LoadMetadataForAOTAssemblies();

#if !UNITY_EDITOR
                    System.Reflection.Assembly.Load(hotfixAb.LoadAsset<TextAsset>("Assembly-CSharp.dll.bytes").bytes);
#endif

                    MainEntry.Download.BeginDownloadSingle(YFConstDefine.GameEntryAssetBundlePath, onComplete: (string fileUrl) =>
                    {
                        Debug.LogError(fileUrl);

                        AssetBundle prefabAb = AssetBundle.LoadFromFile(string.Format("{0}/{1}", Application.persistentDataPath, fileUrl));
                        Object.Instantiate(prefabAb.LoadAsset<GameObject>("gameentry.prefab"));
                    });

                });
            };

            MainEntry.ResourceManager.InitStreamingAssetsBundleInfo();

        }

        /// <summary>
        /// Ϊaot assembly����ԭʼmetadata�� ��������aot�����ȸ��¶��С�
        /// һ�����غ����AOT���ͺ�����Ӧnativeʵ�ֲ����ڣ����Զ��滻Ϊ����ģʽִ��
        /// </summary>
        private static void LoadMetadataForAOTAssemblies()
        {
            List<string> aotMetaAssemblyFiles = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
        };
            /// ע�⣬����Ԫ�����Ǹ�AOT dll����Ԫ���ݣ������Ǹ��ȸ���dll����Ԫ���ݡ�
            /// �ȸ���dll��ȱԪ���ݣ�����Ҫ���䣬�������LoadMetadataForAOTAssembly�᷵�ش���
            /// 
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in aotMetaAssemblyFiles)
            {
                byte[] dllBytes = hotfixAb.LoadAsset<TextAsset>(aotDllName + ".bytes").bytes;
                // ����assembly��Ӧ��dll�����Զ�Ϊ��hook��һ��aot���ͺ�����native���������ڣ��ý������汾����
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
                Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} ret:{err}");
            }
        }
    }
}