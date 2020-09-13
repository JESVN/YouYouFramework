using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// ��Դ������
    /// </summary>
    public class ResourceManager : ManagerBase
    {
        public StreamingAssetsManager StreamingAssetsManager { get; private set; }

        public LocalAssetsManager LocalAssetsManager { get; private set; }

        /// <summary>
        /// ��Ҫ���ص���Դ���б�
        /// </summary>
        private LinkedList<string> m_NeedDownloadList;
        /// <summary>
        /// ���汾�������� �õ��Ĳ���
        /// </summary>
        private BaseParams m_DownloadingParams;

        public ResourceManager()
        {
            StreamingAssetsManager = new StreamingAssetsManager();
            LocalAssetsManager = new LocalAssetsManager();
            m_NeedDownloadList = new LinkedList<string>();
            m_StreamingAssetsVersionDic = new Dictionary<string, AssetBundleInfoEntity>();
            m_CDNVersionDic = new Dictionary<string, AssetBundleInfoEntity>();
            m_LocalAssetsVersionDic = new Dictionary<string, AssetBundleInfoEntity>();
		}

		internal override void Init()
		{

		}


		#region GetAssetBundleVersionList �����ֽ������ȡ��Դ���汾��Ϣ(�����汾�ļ�)
		/// <summary>
		/// �����ֽ������ȡ��Դ���汾��Ϣ
		/// </summary>
		/// <param name="buffer">�������Buffer��ȡ��Դ���汾��Ϣ</param>
		/// <param name="version">�汾��</param>
		/// <returns>Key:��Դ������;Value:��Դ���汾��Ϣ</returns>
		public static Dictionary<string, AssetBundleInfoEntity> GetAssetBundleVersionList(byte[] buffer, ref string version)
        {
            buffer = ZlibHelper.DeCompressBytes(buffer);

            Dictionary<string, AssetBundleInfoEntity> dic = new Dictionary<string, AssetBundleInfoEntity>();

            MMO_MemoryStream ms = new MMO_MemoryStream(buffer);

            int len = ms.ReadInt();
            for (int i = 0; i < len; i++)
            {
                if (i == 0)
                {
                    //�汾��
                    version = ms.ReadUTF8String().Trim();
                }
                else
                {
                    AssetBundleInfoEntity entity = new AssetBundleInfoEntity();
                    entity.AssetBundleName = ms.ReadUTF8String();
                    entity.MD5 = ms.ReadUTF8String();
                    entity.Size = ms.ReadULong();
                    entity.IsFirstData = ms.ReadByte() == 1;
                    entity.IsEncrypt = ms.ReadByte() == 1;

                    dic[entity.AssetBundleName] = entity;
                }
            }
            return dic;
        }
        #endregion

        #region ֻ����
        /// <summary>
        /// ֻ������Դ�汾��
        /// </summary>
        private string m_StreamingAssetsVersion;

        /// <summary>
        /// ֻ������Դ����Ϣ
        /// </summary>
        private Dictionary<string, AssetBundleInfoEntity> m_StreamingAssetsVersionDic;

        /// <summary>
        /// �Ƿ����ֻ������Դ����Ϣ
        /// </summary>
        private bool m_IsExistsStreamingAssetsBundleInfo;

        #region InitStreamingAssetsBundleInfo ��ʼ��ֻ������Դ����Ϣ
        /// <summary>
        /// ��ʼ��ֻ������Դ����Ϣ
        /// </summary>
        public void InitStreamingAssetsBundleInfo()
        {
            ReadStreamingAssetsBundle("VersionFile.bytes", (byte[] buffer) =>
            {
                if (buffer == null)
                {
                    InitCDNAssetBundleInfo();
                }
                else
                {
                    m_IsExistsStreamingAssetsBundleInfo = true;
                    m_StreamingAssetsVersionDic = GetAssetBundleVersionList(buffer, ref m_StreamingAssetsVersion);

                    GameEntry.Log(LogCategory.Resource, "��ȡֻ��������Դ���ɹ�=>ReadStreamingAssetsBundle=>onComplete()");

                    InitCDNAssetBundleInfo();
                }
            });
        }
        #endregion

        #region ReadStreamingAssetsBundle ��ȡֻ��������Դ��
        /// <summary>
        /// ��ȡֻ��������Դ��
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="onComplete"></param>
        internal void ReadStreamingAssetsBundle(string fileUrl, Action<byte[]> onComplete)
        {
            StreamingAssetsManager.ReadAssetBundle(fileUrl, onComplete);
        }
        #endregion


        #endregion

        #region CDN
        /// <summary>
        /// CDN��Դ�汾��
        /// </summary>
        private string m_CDNVersion;

        public string CDNVersion { get { return m_CDNVersion; } }

        /// <summary>
        /// CDN��Դ����Ϣ
        /// </summary>
        private Dictionary<string, AssetBundleInfoEntity> m_CDNVersionDic;

        /// <summary>
        /// ��ʼ��CDN��Դ����Ϣ
        /// </summary>
        private void InitCDNAssetBundleInfo()
        {
            string url = string.Format("{0}VersionFile.bytes", GameEntry.Data.SysDataManager.CurrChannelConfig.RealSourceUrl);
			GameEntry.Http.Get(url, OnInitCDNAssetBundleInfo);
        }
        /// <summary>
        /// ��ʼ��CDN��Դ����Ϣ�ص�
        /// </summary>
        /// <param name="args"></param>
        private void OnInitCDNAssetBundleInfo(HttpCallBackArgs args)
        {
                m_CDNVersionDic = GetAssetBundleVersionList(args.Data, ref m_CDNVersion);
                GameEntry.Log(LogCategory.Resource, "��ʼ��CDN��Դ����Ϣ�ص�=>OnInitCDNAssetBundleInfo()");

                //foreach (var item in m_CDNVersionDic)
                //{
                //    GameEntry.Log(LogCategory.Normal, "item=>" + item.Key);
                //}
                CheckVersionFileExistsInLocal();
            }
        #endregion

        #region ��д��
        /// <summary>
        /// ��д����Դ�汾��
        /// </summary>
        private string m_LocalAssetsVersion;

        /// <summary>
        /// ��д����Դ����Ϣ
        /// </summary>
        private Dictionary<string, AssetBundleInfoEntity> m_LocalAssetsVersionDic;

        /// <summary>
        /// ����д���汾�ļ��Ƿ����
        /// </summary>
        public void CheckVersionFileExistsInLocal()
        {
            GameEntry.Log(LogCategory.Resource, "����д���汾�ļ��Ƿ����=>CheckVersionFileExistsInLocal()");

            if (LocalAssetsManager.GetVersionFileExists())
            {
                //��д���汾�ļ�����
                //���ؿ�д����Դ����Ϣ
                InitLocalAssetBundleInfo();
            }
            else
            {
                //��д���汾�ļ�������

                //�ж�ֻ�����汾�ļ��Ƿ����
                if (m_IsExistsStreamingAssetsBundleInfo)
                {
                    //ֻ�����汾�ļ�����
                    //��ֻ�����汾�ļ���ʼ������д��
                    InitVersionFileFormStreamingAssetsToLocal();
                }

                CheckVersionChange();
            }
        }

        /// <summary>
        /// ��ֻ�����汾�ļ���ʼ������д��
        /// </summary>
        private void InitVersionFileFormStreamingAssetsToLocal()
        {
            GameEntry.Log(LogCategory.Resource, "��ֻ�����汾�ļ���ʼ������д��=>InitVersionFileFormStreamingAssetsToLocal()");

            m_LocalAssetsVersionDic = new Dictionary<string, AssetBundleInfoEntity>();

            var enumerator = m_StreamingAssetsVersionDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetBundleInfoEntity entity = enumerator.Current.Value;
                m_LocalAssetsVersionDic[enumerator.Current.Key] = new AssetBundleInfoEntity()
                {
                    AssetBundleName = entity.AssetBundleName,
                    MD5 = entity.MD5,
                    Size = entity.Size,
                    IsFirstData = entity.IsFirstData,
                    IsEncrypt = entity.IsEncrypt
                };
            }

            //����汾�ļ�
            LocalAssetsManager.SaveVersionFile(m_LocalAssetsVersionDic);

            //����汾��
            m_LocalAssetsVersion = m_StreamingAssetsVersion;
            LocalAssetsManager.SetResourceVersion(m_LocalAssetsVersion);
        }

        /// <summary>
        /// ��ʼ����д����Դ����Ϣ
        /// </summary>
        private void InitLocalAssetBundleInfo()
        {
            GameEntry.Log(LogCategory.Resource, "��ʼ����д����Դ����Ϣ=>InitLocalAssetBundleInfo()");

            m_LocalAssetsVersionDic = LocalAssetsManager.GetAssetBundleVersionList(ref m_LocalAssetsVersion);

            CheckVersionChange();
        }
        /// <summary>
        /// �����д���汾��Ϣ
        /// </summary>
        /// <param name="entity"></param>
        public void SaveVersion(AssetBundleInfoEntity entity)
        {
            if (m_LocalAssetsVersionDic == null)
            {
                m_LocalAssetsVersionDic = new Dictionary<string, AssetBundleInfoEntity>();
            }
            m_LocalAssetsVersionDic[entity.AssetBundleName] = entity;

            //����汾�ļ�
            LocalAssetsManager.SaveVersionFile(m_LocalAssetsVersionDic);
        }
        /// <summary>
        /// ������Դ�汾��(���ڼ��汾������Ϻ� ����)
        /// </summary>
        public void SetResourceVersion()
        {
            m_LocalAssetsVersion = m_CDNVersion;
            LocalAssetsManager.SetResourceVersion(m_LocalAssetsVersion);
        }
        #endregion

        /// <summary>
        /// ��ȡCDN�ϵ���Դ����Ϣ(�������һ��Ҫ�ܷ�����Դ����Ϣ)
        /// </summary>
        /// <param name="assetBundlePath"></param>
        /// <returns></returns>
        internal AssetBundleInfoEntity GetAssetBundleInfo(string assetBundlePath)
        {
            AssetBundleInfoEntity entity = null;
            m_CDNVersionDic.TryGetValue(assetBundlePath, out entity);
            if (entity == null)
            {
                GameEntry.LogError("��ȡ��Դ����Ϣʧ��=>{0}", assetBundlePath);
            }
            return entity;
        }


        /// <summary>
        /// ������
        /// </summary>
        private void CheckVersionChange()
        {
			GameEntry.Log(LogCategory.Resource, "������=>CheckVersionChange(), �汾��=>{0}", m_LocalAssetsVersion);

            if (LocalAssetsManager.GetVersionFileExists())
            {
                if (!string.IsNullOrEmpty(m_LocalAssetsVersion) && m_LocalAssetsVersion.Equals(m_CDNVersion))
                {
                    GameEntry.Log(LogCategory.Resource, "��д���汾�ź�CDN�汾��һ�� ����Ԥ��������");
                    GameEntry.Procedure.ChangeState(ProcedureState.Preload);
                }
                else
                {
                    GameEntry.Log(LogCategory.Resource, "��д���汾�ź�CDN�汾�Ų�һ�� ��ʼ������");
                    BeginCheckVersionChange();
                }
            }
            else
            {
                //���س�ʼ��Դ
                DownloadInitResources();
            }
        }

        #region DownloadInitResources ���س�ʼ��Դ
        /// <summary>
        /// ���س�ʼ��Դ
        /// </summary>
        private void DownloadInitResources()
        {
            GameEntry.Event.CommonEvent.Dispatch(SysEventId.CheckVersionBeginDownload);
            m_DownloadingParams = GameEntry.Pool.DequeueClassObject<BaseParams>();
            m_NeedDownloadList.Clear();

            var enumerator = m_CDNVersionDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetBundleInfoEntity entity = enumerator.Current.Value;
                if (entity.IsFirstData)
                {
                    m_NeedDownloadList.AddLast(entity.AssetBundleName);
                }
            }

			GameEntry.Log(LogCategory.Resource, "���س�ʼ��Դ,�ļ�����==>>" + m_NeedDownloadList.Count);
            GameEntry.Download.BeginDownloadMulit(m_NeedDownloadList, OnDownloadMulitUpdate, OnDownloadMulitComplete);
        }
        #endregion

        #region BeginCheckVersionChange ��ʼ������
        /// <summary>
        /// ��ʼ������
        /// </summary>
        private void BeginCheckVersionChange()
        {
            m_DownloadingParams = GameEntry.Pool.DequeueClassObject<BaseParams>();
            m_DownloadingParams.Reset();

            //��Ҫɾ�����ļ�
            LinkedList<string> delList = new LinkedList<string>();

            //��д����ԴMD5��CDN��ԴCD5��һ�µ��ļ�
            LinkedList<string> inconformityList = new LinkedList<string>();
            LinkedList<string> needDownloadList = new LinkedList<string>();

            #region �ҳ���Ҫɾ���Ŀ�д���ļ����� �� ��Ҫ���صĿ�д���ļ�����
            var enumerator = m_LocalAssetsVersionDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string assetBundleName = enumerator.Current.Key;

                AssetBundleInfoEntity cdnAssetBundleInfo = null;
                if (m_CDNVersionDic.TryGetValue(assetBundleName, out cdnAssetBundleInfo))
                {
                    //��д���� CDNҲ��
                    if (!cdnAssetBundleInfo.MD5.Equals(enumerator.Current.Value.MD5, StringComparison.CurrentCultureIgnoreCase))
                    {
                        //���MD5��һ�� ���벻һ������
                        inconformityList.AddLast(assetBundleName);
                    }
                }
                else
                {
                    //��д���� CDN��û�� ����ɾ������
                    delList.AddLast(assetBundleName);
                }
            }

            //�ж�����ļ���ֻ������MD5��CDN�Ƿ�һ�� һ�µ�Ҫɾ�� ��һ�µ�����������
            LinkedListNode<string> currInconformity = inconformityList.First;
            while (currInconformity != null)
            {
                AssetBundleInfoEntity cdnAssetBundleInfo = null;
                m_CDNVersionDic.TryGetValue(currInconformity.Value, out cdnAssetBundleInfo);

                AssetBundleInfoEntity streamingAssetsAssetBundleInfo = null;
                if (m_StreamingAssetsVersionDic != null)
                {
                    m_StreamingAssetsVersionDic.TryGetValue(currInconformity.Value, out streamingAssetsAssetBundleInfo);
                }

                if (streamingAssetsAssetBundleInfo == null)
                {
                    //���ֻ����û��,����������
                    needDownloadList.AddLast(currInconformity.Value);
                }
                else
                {
                    if (cdnAssetBundleInfo.MD5.Equals(streamingAssetsAssetBundleInfo.MD5, StringComparison.CurrentCultureIgnoreCase))
                    {
                        //һ��,��ɾ��
                        delList.AddLast(currInconformity.Value);
                    }
                    else
                    {
                        //��һ��,����������
                        needDownloadList.AddLast(currInconformity.Value);
                    }
                }

                currInconformity = currInconformity.Next;
            }
            #endregion

            #region ������Ҫɾ���Ŀ�д���ļ�����
            LinkedListNode<string> currDel = delList.First;
            while (currDel != null)
            {
                string filePath = string.Format("{0}/{1}", GameEntry.Resource.LocalFilePath, currDel.Value);

                if (File.Exists(filePath)) File.Delete(filePath);

                LinkedListNode<string> next = currDel.Next;
                delList.Remove(currDel);
                currDel = next;
            }
            #endregion

            #region ������Ҫ���صĿ�д���ļ�����
            enumerator = m_CDNVersionDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                AssetBundleInfoEntity cdnAssetBundleInfo = enumerator.Current.Value;
                if (cdnAssetBundleInfo.IsFirstData)//�Ƿ��ʼ��Դ
                {
                    if (!m_LocalAssetsVersionDic.ContainsKey(cdnAssetBundleInfo.AssetBundleName))
                    {
                        //�����д��û�� ��ȥֻ��������
                        AssetBundleInfoEntity streamingAssetsAssetBundleInfo = null;
                        if (m_StreamingAssetsVersionDic != null)
                        {
                            m_StreamingAssetsVersionDic.TryGetValue(cdnAssetBundleInfo.AssetBundleName, out streamingAssetsAssetBundleInfo);
                        }
                        if (streamingAssetsAssetBundleInfo == null)
                        {
                            //���ֻ����Ҳû��,��ȥCDN������
                            needDownloadList.AddLast(cdnAssetBundleInfo.AssetBundleName);
                        }
                        else
                        {
                            //���ֻ������ ��֤MD5
                            if (!cdnAssetBundleInfo.MD5.Equals(streamingAssetsAssetBundleInfo.MD5, StringComparison.CurrentCultureIgnoreCase))
                            {
                                //MD5��һ��,��ȥCDN������
                                needDownloadList.AddLast(cdnAssetBundleInfo.AssetBundleName);
                            }
                        }
                    }
                }
            }
            #endregion

            GameEntry.Event.CommonEvent.Dispatch(SysEventId.CheckVersionBeginDownload);

            //��������
			GameEntry.Log(LogCategory.Resource, "���ظ�����Դ,�ļ�����==>>" + needDownloadList.Count);
            GameEntry.Download.BeginDownloadMulit(needDownloadList, OnDownloadMulitUpdate, OnDownloadMulitComplete);
        }
        #endregion

        /// <summary>
        /// ���ؽ�����
        /// </summary>
        private void OnDownloadMulitUpdate(int t1, int t2, ulong t3, ulong t4)
        {
            m_DownloadingParams.IntParam1 = t1;
            m_DownloadingParams.IntParam2 = t2;

            m_DownloadingParams.ULongParam1 = t3;
            m_DownloadingParams.ULongParam2 = t4;

            GameEntry.Event.CommonEvent.Dispatch(SysEventId.CheckVersionDownloadUpdate, m_DownloadingParams);
        }
        /// <summary>
        /// �������
        /// </summary>
        private void OnDownloadMulitComplete()
        {
            SetResourceVersion();

            GameEntry.Event.CommonEvent.Dispatch(SysEventId.CheckVersionDownloadComplete);
            GameEntry.Pool.EnqueueClassObject(m_DownloadingParams);

            //����Ԥ��������
            GameEntry.Procedure.ChangeState(ProcedureState.Preload);
        }


        public void Dispose()
        {
            m_StreamingAssetsVersionDic.Clear();
            m_CDNVersionDic.Clear();
            m_LocalAssetsVersionDic.Clear();
        }
	}
}