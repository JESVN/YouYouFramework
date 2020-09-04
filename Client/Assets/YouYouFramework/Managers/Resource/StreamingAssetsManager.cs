using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// StreamingAssets������
    /// </summary>
    public class StreamingAssetsManager
    {
        /// <summary>
        /// m_StreamingAssets��Դ·��
        /// </summary>
        private string m_StreamingAssetsPath;


        public StreamingAssetsManager()
        {
            m_StreamingAssetsPath = "file:///" + Application.streamingAssetsPath;

#if UNITY_ANDROID && !UNITY_EDITOR
            m_StreamingAssetsPath = Application.streamingAssetsPath;
#endif
        }

        #region ReadStreamingAssets ��ȡStreamingAssets�µ���Դ
        /// <summary>
        /// ��ȡStreamingAssets�µ���Դ
        /// </summary>
        /// <param name="url">��Դ·��</param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        private IEnumerator ReadStreamingAssets(string url, Action<byte[]> onComplete)
        {
            //Debug.Log(url);
            using (WWW www = new WWW(url))
            {
                yield return www;
                if (www.error == null)
                {
                    if (onComplete != null) onComplete(www.bytes);
                }
                else
                {
                    if (onComplete != null) onComplete(null);
                }
            }
        }
        #endregion

        #region ReadAssetBundle ��ȡֻ������Դ��
        /// <summary>
        /// ��ȡֻ������Դ��
        /// </summary>
        /// <param name="fileUrl">��Դ·��</param>
        /// <param name="onComplete"></param>
        public void ReadAssetBundle(string fileUrl, Action<byte[]> onComplete)
        {
            GameEntry.Instance.StartCoroutine(ReadStreamingAssets(string.Format("{0}/AssetBundles/{1}", m_StreamingAssetsPath, fileUrl), onComplete));
        }
        #endregion

    }
}