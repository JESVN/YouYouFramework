using Main;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// ��Դ��������
    /// </summary>
    public class AssetBundleLoaderRoutine
    {
        /// <summary>
        /// ��ǰ����Դ����Ϣ
        /// </summary>
        private AssetBundleInfoEntity m_CurrAssetBundleInfo;

        /// <summary>
        /// ��Դ����������
        /// </summary>
        private AssetBundleCreateRequest m_CurrAssetBundleCreateRequest;

        /// <summary>
        /// ��Դ�������������
        /// </summary>
        public Action<float> OnAssetBundleCreateUpdate;

        /// <summary>
        /// ������Դ�����
        /// </summary>
        public Action<AssetBundle> OnLoadAssetBundleComplete;

        #region LoadAssetBundle ������Դ��
        public void LoadAssetBundleAsync(string assetBundlePath)
        {
            void LoadAssetBundleAsync(byte[] buffer)
            {
                if (m_CurrAssetBundleInfo.IsEncrypt)
                {
                    //�����Դ���Ǽ��ܵ�,�����
                    buffer = SecurityUtil.Xor(buffer);
                }

                m_CurrAssetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync(buffer);
            }


            m_CurrAssetBundleInfo = MainEntry.ResourceManager.GetAssetBundleInfo(assetBundlePath);

            //����ļ��ڿ�д���Ƿ����
            bool isExistsInLocal = MainEntry.ResourceManager.LocalAssetsManager.CheckFileExists(assetBundlePath);

            if (isExistsInLocal && !m_CurrAssetBundleInfo.IsEncrypt)
            {
                //��д������, ���ý���
                m_CurrAssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(string.Format("{0}/{1}", Application.persistentDataPath, assetBundlePath));
            }
            else
            {
                //��д������, ��Ҫ����
                byte[] buffer = MainEntry.ResourceManager.LocalAssetsManager.GetFileBuffer(assetBundlePath);
                if (buffer != null)
                {
                    LoadAssetBundleAsync(buffer);
                    return;
                }

                //�����д��û�� ��ô�ʹ�ֻ������ȡ
                MainEntry.ResourceManager.StreamingAssetsManager.ReadAssetBundleAsync(assetBundlePath, (byte[] buff) =>
                {
                    if (buff != null)
                    {
                        //��ֻ����������Դ��
                        LoadAssetBundleAsync(buff);
                        return;
                    }

                    //���ֻ����Ҳû��,��CDN����
                    MainEntry.Download.BeginDownloadSingle(assetBundlePath, (url, currSize, progress) =>
                    {
                        //YouYou.GameEntry.LogError(progress);
                        OnAssetBundleCreateUpdate?.Invoke(progress);
                    }, (string fileUrl) =>
                    {
                        buffer = MainEntry.ResourceManager.LocalAssetsManager.GetFileBuffer(fileUrl);
                        LoadAssetBundleAsync(buffer);
                    });
                });
            }

        }
        public AssetBundle LoadAssetBundle(string assetBundlePath)
        {
            AssetBundle LoadAssetBundle(byte[] buffer)
            {
                if (m_CurrAssetBundleInfo.IsEncrypt)
                {
                    //�����Դ���Ǽ��ܵ�,�����
                    buffer = SecurityUtil.Xor(buffer);
                }

                return AssetBundle.LoadFromMemory(buffer);
            }


            m_CurrAssetBundleInfo = MainEntry.ResourceManager.GetAssetBundleInfo(assetBundlePath);

            //����ļ��ڿ�д���Ƿ����
            bool isExistsInLocal = MainEntry.ResourceManager.LocalAssetsManager.CheckFileExists(assetBundlePath);

            if (isExistsInLocal && !m_CurrAssetBundleInfo.IsEncrypt)
            {
                //��д������, ���ý���
                return AssetBundle.LoadFromFile(string.Format("{0}/{1}", Application.persistentDataPath, assetBundlePath));
            }
            else
            {
                //��д������, ��Ҫ����
                byte[] buffer = MainEntry.ResourceManager.LocalAssetsManager.GetFileBuffer(assetBundlePath);
                if (buffer != null)
                {
                    return LoadAssetBundle(buffer);
                }

                //ֻ��������(Ŀǰ��֧�ּ�����Դ)
                AssetBundle assetBundle = MainEntry.ResourceManager.StreamingAssetsManager.ReadAssetBundle(assetBundlePath);
                if (assetBundle != null)
                {
                    return assetBundle;
                }

                GameEntry.LogError(LogCategory.Resource, "����û�и���Դ, ����Ҫȥ���������==" + assetBundlePath);
                return null;
            }

        }

        #endregion

        /// <summary>
        /// ����
        /// </summary>
        public void Reset()
        {
            m_CurrAssetBundleCreateRequest = null;
        }

        /// <summary>
        /// ����
        /// </summary>
        internal void OnUpdate()
        {
            UpdateAssetBundleCreateRequest();
        }

        #region UpdateAssetBundleCreateRequest ������Դ������
        /// <summary>
        /// ������Դ������
        /// </summary>
        private void UpdateAssetBundleCreateRequest()
        {
            if (m_CurrAssetBundleCreateRequest == null) return;
            if (m_CurrAssetBundleCreateRequest.isDone)
            {
                AssetBundle assetBundle = m_CurrAssetBundleCreateRequest.assetBundle;
                if (assetBundle != null)
                {
                    //GameEntry.Log(LogCategory.Resource, "��Դ��=>{0} �������", m_CurrAssetBundleInfo.AssetBundleName);
                }
                else
                {
                    GameEntry.LogError(LogCategory.Resource, "��Դ��=>{0} ����ʧ��", m_CurrAssetBundleInfo.AssetBundleName);
                }
                OnLoadAssetBundleComplete?.Invoke(assetBundle);
                Reset();//һ��Ҫ���Reset
            }
            else
            {
                //���ؽ���
                //OnAssetBundleCreateUpdate?.Invoke(m_CurrAssetBundleCreateRequest.progress);
            }
        }
        #endregion
    }
}