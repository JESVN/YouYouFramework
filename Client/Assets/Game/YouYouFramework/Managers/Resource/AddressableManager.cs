using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// ��Ѱַ��Դ������
	/// </summary>
	public class AddressableManager : ManagerBase, IDisposable
	{
		/// <summary>
		/// ��Դ������
		/// </summary>
		public ResourceManager ResourceManager { get; private set; }

		/// <summary>
		/// ��Դ���ع�����
		/// </summary>
		public ResourceLoaderManager ResourceLoaderManager { get; private set; }

		/// <summary>
		/// �����ļ�·��
		/// </summary>
		public string LocalFilePath { get; private set; }

		internal AddressableManager()
		{
			ResourceManager = new ResourceManager();
			ResourceLoaderManager = new ResourceLoaderManager();
		}
		public void Dispose()
		{
			ResourceManager.Dispose();
			ResourceLoaderManager.Dispose();
		}
		internal override void Init()
		{
#if EDITORLOAD
			LocalFilePath = Application.dataPath;
#elif ASSETBUNDLE
            LocalFilePath = Application.persistentDataPath;
#endif
            ResourceManager.Init();
            ResourceLoaderManager.Init();

            Application.backgroundLoadingPriority = ThreadPriority.High;
        }
		internal void OnUpdate()
		{
			ResourceLoaderManager.OnUpdate();
		}

        /// <summary>
        /// ��ʼ��ֻ������Դ����Ϣ
        /// </summary>
        public void InitStreamingAssetsBundleInfo()
        {
            ResourceManager.InitStreamingAssetsBundleInfo();
        }

        /// <summary>
        /// ��ʼ����Դ��Ϣ
        /// </summary>
        public void InitAssetInfo(BaseAction initAssetInfoComplete)
        {
            ResourceLoaderManager.InitAssetInfo(initAssetInfoComplete);
        }


        /// <summary>
        /// ��ȡ·�����������
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetLastPathName(string path)
        {
            if (path.IndexOf('/') == -1)
            {
                return path;
            }
            return path.Substring(path.LastIndexOf('/') + 1);
        }
        /// <summary>
        /// ��ȡ��������Դ��·��
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public string GetSceneAssetBundlePath(string sceneName)
        {
            //string.Format("download/scenes/{0}.assetbundle", sceneName.ToLower());
            return string.Format("Assets/Download/Scenes/{0}.unity", sceneName.ToLower());
        }
    }
}