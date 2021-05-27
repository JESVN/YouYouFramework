using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	/// <summary>
	/// ��Դ���ع�����
	/// </summary>
	public class ResourceLoaderManager : IDisposable
	{
		/// <summary>
		/// ��Դ��Ϣ�ֵ�
		/// </summary>
		private Dictionary<AssetCategory, Dictionary<string, AssetEntity>> m_AssetInfoDic;

		/// <summary>
		/// ��Դ������������
		/// </summary>
		private LinkedList<AssetBundleLoaderRoutine> m_AssetBundleLoaderList;

		/// <summary>
		/// ��Դ����������
		/// </summary>
		private LinkedList<AssetLoaderRoutine> m_AssetLoaderList;

		public ResourceLoaderManager()
		{
			m_AssetInfoDic = new Dictionary<AssetCategory, Dictionary<string, AssetEntity>>();
			m_AssetBundleLoaderList = new LinkedList<AssetBundleLoaderRoutine>();
			m_AssetLoaderList = new LinkedList<AssetLoaderRoutine>();
		}
		internal void Init()
		{
			//ȷ����Ϸ�տ�ʼ���е�ʱ�� �����ֵ��Ѿ���ʼ������
			var enumerator = Enum.GetValues(typeof(AssetCategory)).GetEnumerator();
			while (enumerator.MoveNext())
			{
				AssetCategory assetCategory = (AssetCategory)enumerator.Current;
				m_AssetInfoDic[assetCategory] = new Dictionary<string, AssetEntity>();
			}
		}
		internal void OnUpdate()
		{
			for (LinkedListNode<AssetBundleLoaderRoutine> curr = m_AssetBundleLoaderList.First; curr != null; curr = curr.Next)
			{
				curr.Value.OnUpdate();
			}

			for (LinkedListNode<AssetLoaderRoutine> curr = m_AssetLoaderList.First; curr != null; curr = curr.Next)
			{
				curr.Value.OnUpdate();
			}
		}
		public void Dispose()
		{
			m_AssetInfoDic.Clear();
			m_AssetLoaderList.Clear();
		}

		#region InitAssetInfo ��ʼ����Դ��Ϣ
		private BaseAction m_InitAssetInfoComplete;
		/// <summary>
		/// ��ʼ����Դ��Ϣ
		/// </summary>
		internal void InitAssetInfo(BaseAction initAssetInfoComplete)
		{
			m_InitAssetInfoComplete = initAssetInfoComplete;

			byte[] buffer = GameEntry.Resource.ResourceManager.LocalAssetsManager.GetFileBuffer(YFConstDefine.AssetInfoName);
			if (buffer == null)
			{
				//�����д��û�� ��ô�ʹ�ֻ������ȡ
				GameEntry.Resource.ResourceManager.StreamingAssetsManager.ReadAssetBundle(YFConstDefine.AssetInfoName, (byte[] buff) =>
				 {
					 if (buff == null)
					 {
						 //���ֻ����Ҳû��,��CDN��ȡ
						 string url = string.Format("{0}{1}", GameEntry.Data.SysDataManager.CurrChannelConfig.RealSourceUrl, YFConstDefine.AssetInfoName);
						 GameEntry.Http.Get(url, false, (HttpCallBackArgs args) =>
						  {
							  if (!args.HasError)
							  {
								  GameEntry.Log(LogCategory.Normal, "��CDN��ʼ����Դ��Ϣ");
								  InitAssetInfo(args.Data);
							  }
						  });
					 }
					 else
					 {
						 GameEntry.Log(LogCategory.Normal, "��ֻ������ʼ����Դ��Ϣ");
						 InitAssetInfo(buff);
					 }
				 });
			}
			else
			{
				GameEntry.Log(LogCategory.Normal, "�ӿ�д����ʼ����Դ��Ϣ");
				InitAssetInfo(buffer);
			}
		}

		/// <summary>
		/// ��ʼ����Դ��Ϣ
		/// </summary>
		/// <param name="buffer"></param>
		private void InitAssetInfo(byte[] buffer)
		{
			buffer = ZlibHelper.DeCompressBytes(buffer);//��ѹ

			MMO_MemoryStream ms = new MMO_MemoryStream(buffer);
			int len = ms.ReadInt();
			int depLen = 0;
			for (int i = 0; i < len; i++)
			{
				AssetEntity entity = new AssetEntity();
				entity.Category = (AssetCategory)ms.ReadByte();
				entity.AssetFullName = ms.ReadUTF8String();
				entity.AssetBundleName = ms.ReadUTF8String();

				//Debug.Log("entity.Category=" + entity.Category);
				//Debug.Log("entity.AssetBundleName=" + entity.AssetBundleName);
				//Debug.Log("entity.AssetFullName=" + entity.AssetFullName);

				depLen = ms.ReadInt();
				if (depLen > 0)
				{
					entity.DependsAssetList = new List<AssetDependsEntity>(depLen);
					for (int j = 0; j < depLen; j++)
					{
						AssetDependsEntity assetDepends = new AssetDependsEntity();
						assetDepends.Category = (AssetCategory)ms.ReadByte();
						assetDepends.AssetFullName = ms.ReadUTF8String();
						entity.DependsAssetList.Add(assetDepends);
					}
				}

				m_AssetInfoDic[entity.Category][entity.AssetFullName] = entity;
			}

			m_InitAssetInfoComplete?.Invoke();
		}

		/// <summary>
		/// ������Դ�������Դ·����ȡ��Դ��Ϣ
		/// </summary>
		/// <param name="assetCategory">��Դ����</param>
		/// <param name="assetFullName">��Դ·��</param>
		/// <returns></returns>
		internal AssetEntity GetAssetEntity(AssetCategory assetCategory, string assetFullName)
		{
			Dictionary<string, AssetEntity> dicCategory = null;
			if (m_AssetInfoDic.TryGetValue(assetCategory, out dicCategory))
			{
				AssetEntity entity = null;
				if (dicCategory.TryGetValue(assetFullName, out entity))
				{
					return entity;
				}
			}
			GameEntry.LogError("��Դ������,assetCategory=>{0}, assetFullName=>{1}", assetCategory, assetFullName);
			return null;
		}
		#endregion

		#region LoadAssetBundle ������Դ��
		/// <summary>
		/// �����е�Bundle
		/// </summary>
		private Dictionary<string, LinkedList<Action<ResourceEntity>>> m_LoadingAssetBundle = new Dictionary<string, LinkedList<Action<ResourceEntity>>>();

		/// <summary>
		/// ������Դ��
		/// </summary>
		/// <param name="assetbundlePath"></param>
		/// <param name="onUpdate"></param>
		/// <param name="onComplete"></param>
		public void LoadAssetBundle(string assetbundlePath, Action<float> onUpdate = null, Action<ResourceEntity> onComplete = null)
		{
			//1.�ж���Դ���Ƿ������AssetBundlePool
			ResourceEntity assetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetbundlePath);
			if (assetBundleEntity != null)
			{
				//Debug.Log("��Դ������Դ���д��� ����Դ���м���AssetBundle");
				onComplete?.Invoke(assetBundleEntity);
				return;
			}

			//2.�ж�Bundle�Ƿ���ص�һ��,��ֹ�߲��������ظ�����
			LinkedList<Action<ResourceEntity>> lst = null;
			if (m_LoadingAssetBundle.TryGetValue(assetbundlePath, out lst))
			{
				//���Bundle�Ѿ��ڼ�����, ��ί�м����Ӧ������ Ȼ��ֱ��return;
				lst.AddLast(onComplete);
				return;
			}
			else
			{
				//���Bundle��û�п�ʼ����, ��ί�м����Ӧ������ Ȼ��ʼ����
				lst = GameEntry.Pool.DequeueClassObject<LinkedList<Action<ResourceEntity>>>();
				lst.AddLast(onComplete);
				m_LoadingAssetBundle[assetbundlePath] = lst;
			}

			AssetBundleLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<AssetBundleLoaderRoutine>();
			if (routine == null) routine = new AssetBundleLoaderRoutine();

			//��������ʼUpdate()
			m_AssetBundleLoaderList.AddLast(routine);
			//������Դ��
			routine.LoadAssetBundle(assetbundlePath);
			//��Դ������ �����ص�
			routine.OnAssetBundleCreateUpdate = onUpdate;
			routine.OnLoadAssetBundleComplete = (AssetBundle assetbundle) =>
			{
				//��Դ��ע�ᵽ��Դ��
				assetBundleEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
				assetBundleEntity.ResourceName = assetbundlePath;
				assetBundleEntity.IsAssetBundle = true;
				assetBundleEntity.Target = assetbundle;
				GameEntry.Pool.AssetBundlePool.Register(assetBundleEntity);

				for (LinkedListNode<Action<ResourceEntity>> curr = lst.First; curr != null; curr = curr.Next)
				{
					curr.Value?.Invoke(assetBundleEntity);
				}

				lst.Clear();//��Դ������Ϻ�������
				GameEntry.Pool.EnqueueClassObject(lst);
				m_LoadingAssetBundle.Remove(assetbundlePath);//�Ӽ����е�Bundle��Dic �Ƴ�

				//����ѭ�� �س�
				m_AssetBundleLoaderList.Remove(routine);
				GameEntry.Pool.EnqueueClassObject(routine);
			};
		}
		#endregion

		#region LoadAsset ����Դ���м�����Դ
		/// <summary>
		/// �����е�Asset
		/// </summary>
		private Dictionary<string, LinkedList<Action<UnityEngine.Object, bool>>> m_LoadingAsset = new Dictionary<string, LinkedList<Action<UnityEngine.Object, bool>>>();
		/// <summary>
		/// ����Դ���м�����Դ
		/// </summary>
		/// <param name="assetName"></param>
		/// <param name="assetBundle"></param>
		/// <param name="onUpdate"></param>
		/// <param name="onComplete"></param>
		public void LoadAsset(AssetCategory assetCategory, string assetName, AssetBundle assetBundle, Action<float> onUpdate = null, Action<UnityEngine.Object, bool> onComplete = null)
		{
			//Debug.Log(assetName + "===========================================================");
			//1.�ж�Asset�Ƿ���ص�һ��,��ֹ�߲��������ظ�����
			LinkedList<Action<UnityEngine.Object, bool>> lst = null;
			if (m_LoadingAsset.TryGetValue(assetName, out lst))
			{
				//���Asset�Ѿ��ڼ�����, ��ί�м����Ӧ������ Ȼ��ֱ��return;
				lst.AddLast(onComplete);
				return;
			}
			else
			{
				//���Asset��û�п�ʼ����, ��ί�м����Ӧ������ Ȼ��ʼ����
				lst = GameEntry.Pool.DequeueClassObject<LinkedList<Action<UnityEngine.Object, bool>>>();
				lst.AddLast(onComplete);
				m_LoadingAsset[assetName] = lst;
			}


			AssetLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<AssetLoaderRoutine>();
			if (routine == null) routine = new AssetLoaderRoutine();

			//��������ʼѭ��
			m_AssetLoaderList.AddLast(routine);

			//��Դ���� ������ �ص�
			routine.OnAssetUpdate = onUpdate;
			//��Դ���� ��� �ص�
			routine.OnLoadAssetComplete = (UnityEngine.Object obj) =>
			{
				LinkedListNode<Action<UnityEngine.Object, bool>> curr = lst.First;
				curr.Value?.Invoke(obj, true);
				for (curr = curr.Next; curr != null; curr = curr.Next)
				{
					curr.Value?.Invoke(obj, false);
				}
				//��Դ������Ϻ�
				lst.Clear();//�������
				GameEntry.Pool.EnqueueClassObject(lst);
				m_LoadingAsset.Remove(assetName);//�Ӽ����е�Asset��Dic �Ƴ�

				//����ѭ�� �س�
				m_AssetLoaderList.Remove(routine);
				GameEntry.Pool.EnqueueClassObject(routine);
			};
			//������Դ
			routine.LoadAsset(assetName, assetBundle);
		}
		#endregion

		/// <summary>
		/// ��������Դ
		/// </summary>
		/// <param name="assetCategory"></param>
		/// <param name="assetFullName"></param>
		/// <param name="onComplete"></param>
		public void LoadMainAsset<T>(AssetCategory assetCategory, string assetFullName, BaseAction<T> onComplete)
		{
			MainAssetLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<MainAssetLoaderRoutine>();
			routine.Load(assetCategory, assetFullName, false, true, (ResourceEntity resEntity) =>
			{
				onComplete?.Invoke((T)resEntity.Target);
			});
		}
		public void LoadMainAsset(AssetCategory assetCategory, string assetFullName, BaseAction<ResourceEntity> onComplete)
		{
			MainAssetLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<MainAssetLoaderRoutine>();
			routine.Load(assetCategory, assetFullName, true, true, onComplete);
		}
	}
}