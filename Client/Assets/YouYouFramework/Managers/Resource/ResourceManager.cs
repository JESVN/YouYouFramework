using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace YouYou
{
	public class ResourceManager : ManagerBase, IDisposable
	{
		#region GetAssetBundleVersionList �����ֽ������ȡ��Դ���汾��Ϣ
		/// <summary>
		/// �����ֽ������ȡ��Դ���汾��Ϣ
		/// </summary>
		/// <param name="buffer">�ֽ�����</param>
		/// <param name="version">�汾��</param>
		/// <returns></returns>
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

		/// <summary>
		/// ֻ����������
		/// </summary>
		public StreamingAssetsManager StreamingAssetsManager
		{
			get;
			private set;
		}

		/// <summary>
		/// ��д��������
		/// </summary>
		public LocalAssetsManager LocalAssetsManager
		{
			get;
			private set;
		}

		/// <summary>
		/// ��Ҫ���ص���Դ���б�
		/// </summary>
		private LinkedList<string> m_NeedDownloadList;

		/// <summary>
		/// ���汾��������ʱ��Ĳ���
		/// </summary>
		private BaseParams m_DownloadingParams;

		public ResourceManager()
		{
			StreamingAssetsManager = new StreamingAssetsManager();
			LocalAssetsManager = new LocalAssetsManager();

			m_NeedDownloadList = new LinkedList<string>();
		}

		internal override void Init()
		{

		}

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
		private bool m_IsExistsStreamingAssetsBundleInfo = false;

		#region InitStreamingAssetsBundleInfo ��ʼ��ֻ������Դ����Ϣ
		/// <summary>
		/// ��ʼ��ֻ������Դ����Ϣ
		/// </summary>
		public void InitStreamingAssetsBundleInfo()
		{
			ReadStreamingAssetsBundle(YFConstDefine.VersionFileName, (byte[] buffer) =>
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
		internal void ReadStreamingAssetsBundle(string fileUrl, BaseAction<byte[]> onComplete)
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

		/// <summary>
		/// CDN��Դ�汾��
		/// </summary>
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
			StringBuilder sbr = StringHelper.PoolNew();
			string url = sbr.AppendFormatNoGC("{0}{1}", GameEntry.Data.SysDataManager.CurrChannelConfig.RealSourceUrl, YFConstDefine.VersionFileName).ToString();
			StringHelper.PoolDel(ref sbr);
			GameEntry.Log(LogCategory.Resource, url);
			GameEntry.Http.Get(url, false, OnInitCDNAssetBundleInfo);
		}

		/// <summary>
		/// ��ʼ��CDN��Դ����Ϣ�ص�
		/// </summary>
		/// <param name="args"></param>
		private void OnInitCDNAssetBundleInfo(HttpCallBackArgs args)
		{
			if (!args.HasError)
			{
				m_CDNVersionDic = GetAssetBundleVersionList(args.Data, ref m_CDNVersion);
				GameEntry.Log(LogCategory.Resource, "OnInitCDNAssetBundleInfo");

				CheckVersionFileExistsInLocal();
			}
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
		private void CheckVersionFileExistsInLocal()
		{
			GameEntry.Log(LogCategory.Resource, "CheckVersionFileExistsInLocal");

			if (LocalAssetsManager.GetVersionFileExists())
			{
				//��д���汾�ļ�����
				//���ؿ�д����Դ����Ϣ
				InitLocalAssetsBundleInfo();
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
		///��ʼ����д����Դ����Ϣ
		/// </summary>
		private void InitLocalAssetsBundleInfo()
		{
			GameEntry.Log(LogCategory.Resource, "InitLocalAssetsBundleInfo");

			m_LocalAssetsVersionDic = LocalAssetsManager.GetAssetBundleVersionList(ref m_LocalAssetsVersion);

			CheckVersionChange();
		}

		/// <summary>
		/// ����汾��Ϣ
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
		/// ������Դ�汾�ţ����ڼ��汾������Ϻ� ���棩
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
		/// <param name="assetbundlePath"></param>
		/// <returns></returns>
		internal AssetBundleInfoEntity GetAssetBundleInfo(string assetbundlePath)
		{
			AssetBundleInfoEntity entity = null;
			m_CDNVersionDic.TryGetValue(assetbundlePath, out entity);
			return entity;
		}

		#region ������

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
			m_DownloadingParams.Reset();

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

			//���û�г�ʼ��Դ ֱ�Ӽ�����
			if (m_NeedDownloadList.Count == 0)
			{
				BeginCheckVersionChange();
			}
			else
			{
				//LocalAssetsManager.SetResourceVersion(null);//�����汾��, ����ֱ�Ӽ��MD5
				GameEntry.Log(LogCategory.Resource, "���س�ʼ��Դ,�ļ�����==>>" + m_NeedDownloadList.Count);
				GameEntry.Download.BeginDownloadMulit(m_NeedDownloadList, OnDownloadMulitUpdate, OnDownloadMulitComplete);
			}
		}
		#endregion

		/// <summary>
		/// ��ʼ������
		/// </summary>
		private void BeginCheckVersionChange()
		{
			m_DownloadingParams = GameEntry.Pool.DequeueClassObject<BaseParams>();
			m_DownloadingParams.Reset();

			//��Ҫɾ�����ļ�
			LinkedList<string> delList = new LinkedList<string>();

			//��д����ԴMD5��CDN��ԴMD5��һ�µ��ļ�
			LinkedList<string> inconformityList = new LinkedList<string>();

			LinkedList<string> needDownloadList = new LinkedList<string>();

			#region �ҳ���Ҫɾ�����ļ�����ɾ��
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

			//ѭ���ж�����ļ���ֻ������MD5��CDN�Ƿ�һ�� һ�µĽ���ɾ�� ��һ�µĽ�����������
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

			#region ɾ����Ҫɾ����
			GameEntry.Log(LogCategory.Resource, "ɾ������Դ=>{0}", delList.ToJson());
			LinkedListNode<string> currDel = delList.First;
			while (currDel != null)
			{
				StringBuilder sbr = StringHelper.PoolNew();
				string filePath = sbr.AppendFormatNoGC("{0}/{1}", GameEntry.Resource.LocalFilePath, currDel.Value).ToString();
				StringHelper.PoolDel(ref sbr);

				if (File.Exists(filePath)) File.Delete(filePath);
				LinkedListNode<string> next = currDel.Next;
				delList.Remove(currDel);
				currDel = next;
			}
			#endregion

			#region �����Ҫ���ص�
			enumerator = m_CDNVersionDic.GetEnumerator();
			while (enumerator.MoveNext())
			{
				AssetBundleInfoEntity cdnAssetBundleInfo = enumerator.Current.Value;
				if (cdnAssetBundleInfo.IsFirstData)//����ʼ��Դ
				{
					if (!m_LocalAssetsVersionDic.ContainsKey(cdnAssetBundleInfo.AssetBundleName))//�����д��û�� ��ȥֻ�����ж�һ��
					{
						AssetBundleInfoEntity streamingAssetsAssetBundleInfo = null;
						if (m_StreamingAssetsVersionDic != null)
						{
							m_StreamingAssetsVersionDic.TryGetValue(cdnAssetBundleInfo.AssetBundleName, out streamingAssetsAssetBundleInfo);
						}
						if (streamingAssetsAssetBundleInfo == null)//ֻ����������
						{
							needDownloadList.AddLast(cdnAssetBundleInfo.AssetBundleName);
						}
						else//ֻ�������� ��֤MD5
						{
							if (!cdnAssetBundleInfo.MD5.Equals(streamingAssetsAssetBundleInfo.MD5, StringComparison.CurrentCultureIgnoreCase))//MD5��һ��
							{
								needDownloadList.AddLast(cdnAssetBundleInfo.AssetBundleName);
							}
						}
					}
				}
			}
			#endregion

			GameEntry.Event.CommonEvent.Dispatch(SysEventId.CheckVersionBeginDownload);

			//��������
			GameEntry.Log(LogCategory.Resource, "���ظ�����Դ,�ļ�����==>" + needDownloadList.Count + "==>" + needDownloadList.ToJson());
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
			if (m_StreamingAssetsVersionDic != null) m_StreamingAssetsVersionDic.Clear();
			if (m_CDNVersionDic != null) m_CDNVersionDic.Clear();
			if (m_LocalAssetsVersionDic != null) m_LocalAssetsVersionDic.Clear();
		}
	}
}