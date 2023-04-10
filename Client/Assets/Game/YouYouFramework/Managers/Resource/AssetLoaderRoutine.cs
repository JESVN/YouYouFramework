using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace YouYou
{
    /// <summary>
    /// 资源加载器
    /// </summary>
    public class AssetLoaderRoutine
    {
        /// <summary>
        /// 资源加载请求
        /// </summary>
        private AssetBundleRequest m_CurrAssetBundleRequest;

        private string m_CurrAssetName;

        /// <summary>
        /// 资源请求更新
        /// </summary>
        public Action<float> OnAssetUpdate;

        /// <summary>
        /// 加载资源完毕
        /// </summary>
        public Action<UnityEngine.Object> OnLoadAssetComplete;


        internal void LoadAssetAsync(string assetName, AssetBundle assetBundle)
        {
            m_CurrAssetName = assetName;
            m_CurrAssetBundleRequest = assetBundle.LoadAssetAsync(assetName);
        }
        internal Object LoadAsset(string assetName, AssetBundle assetBundle)
        {
            return assetBundle.LoadAsset(assetName);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            m_CurrAssetBundleRequest = null;
        }

        /// <summary>
        /// 更新
        /// </summary>
        internal void OnUpdate()
        {
            UpdateAssetBundleRequest();
        }

        /// <summary>
        /// 更新 资源加载 请求
        /// </summary>
        private void UpdateAssetBundleRequest()
        {
            if (m_CurrAssetBundleRequest != null)
            {
                if (m_CurrAssetBundleRequest.isDone)
                {
                    Object obj = m_CurrAssetBundleRequest.asset;
                    if (obj != null)
                    {
                        //GameEntry.Log(LogCategory.Resource, "资源=>{0} 加载完毕", m_CurrAssetName);
                        Reset();//一定要早点Reset

                        OnLoadAssetComplete?.Invoke(obj);
                    }
                    else
                    {
                        GameEntry.LogError(LogCategory.Resource, "资源=>{0} 加载失败", m_CurrAssetName);
                        Reset();//一定要早点Reset

                        OnLoadAssetComplete?.Invoke(null);
                    }
                }
                else
                {
                    //加载进度
                    OnAssetUpdate?.Invoke(m_CurrAssetBundleRequest.progress);
                }
            }
        }
    }
}