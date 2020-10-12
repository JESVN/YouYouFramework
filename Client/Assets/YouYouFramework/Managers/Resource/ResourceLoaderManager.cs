using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

			//ȷ����Ϸ�տ�ʼ���е�ʱ�� �����ֵ��Ѿ���ʼ������
			var enumerator = Enum.GetValues(typeof(AssetCategory)).GetEnumerator();
			while (enumerator.MoveNext())
			{
				AssetCategory assetCategory = (AssetCategory)enumerator.Current;
				m_AssetInfoDic[assetCategory] = new Dictionary<string, AssetEntity>();
			}

			m_AssetBundleLoaderList = new LinkedList<AssetBundleLoaderRoutine>();
			m_AssetLoaderList = new LinkedList<AssetLoaderRoutine>();
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

		internal void Init()
		{
		}

		public void Dispose()
		{
			m_AssetInfoDic.Clear();
			m_AssetLoaderList.Clear();
		}

		/// <summary>
		/// ��ȡ��Դ��Ϣʵ��
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
			GameEntry.LogError("assetCategory=>{0}, assetFullName=>{1}������", assetCategory, assetFullName);
			return null;
		}

		/// <summary>
		/// ��������Դ
		/// </summary>
		/// <param name="assetCategory"></param>
		/// <param name="assetFullName"></param>
		/// <param name="onComplete"></param>
		public void LoadMainAsset<T>(AssetCategory assetCategory, string assetFullName, BaseAction<T> onComplete)
		{
			LoadMainAsset(assetCategory, assetFullName, (ResourceEntity resEntity) =>
			{
				onComplete?.Invoke((T)resEntity.Target);
			});
		}
		public void LoadMainAsset(AssetCategory assetCategory, string assetFullName, BaseAction<ResourceEntity> onComplete)
		{
			MainAssetLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<MainAssetLoaderRoutine>();
			routine.Load(assetCategory, assetFullName, (ResourceEntity resEntity) =>
			{
				if (resEntity.Target != null)
				{
					onComplete?.Invoke(resEntity);
				}
				else
				{
					GameEntry.LogError("��Դ����ʧ��! assetFullName==" + assetFullName);
				}
			});
		}

		public void UnLoadGameObject(GameObject obj)
		{
			GameEntry.Pool.ReleaseInstanceResource(obj.GetInstanceID());
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
						 GameEntry.Http.Get(url, (HttpCallBackArgs args) =>
						 {
							 GameEntry.Log(LogCategory.Normal, "��CDN��ʼ����Դ��Ϣ");
							 InitAssetInfo(args.Data);
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
		#endregion

		#region LoadAssetBundle ������Դ��
		/// <summary>
		/// �����е�Bundle
		/// </summary>
		private Dictionary<string, LinkedList<Action<AssetBundle>>> m_LoadingAssetBundle = new Dictionary<string, LinkedList<Action<AssetBundle>>>();

		/// <summary>
		/// ������Դ��
		/// </summary>
		/// <param name="assetbundlePath"></param>
		/// <param name="onUpdate"></param>
		/// <param name="onComplete"></param>
		public void LoadAssetBundle(string assetbundlePath, Action<float> onUpdate = null, Action<AssetBundle> onComplete = null)
		{
			//Debug.LogError("������Դ��" + assetbundlePath);
			//1.�ж���Դ���Ƿ������AssetBundlePool
			ResourceEntity assetBundleEntity = GameEntry.Pool.AssetBundlePool.Spawn(assetbundlePath);
			if (assetBundleEntity != null)
			{
				//Debug.Log("��Դ������Դ���д��� ����Դ���м���AssetBundle");
				AssetBundle assetBundle = assetBundleEntity.Target as AssetBundle;
				if (onComplete != null) onComplete(assetBundle);
				return;
			}

			//2.�ж�Bundle�Ƿ���ص�һ��,��ֹ�߲��������ظ�����
			LinkedList<Action<AssetBundle>> lst = null;
			if (m_LoadingAssetBundle.TryGetValue(assetbundlePath, out lst))
			{
				//���Bundle�Ѿ��ڼ�����, ��ί�м����Ӧ������ Ȼ��ֱ��return;
				lst.AddLast(onComplete);
				return;
			}
			else
			{
				//���Bundle��û�п�ʼ����, ��ί�м����Ӧ������ Ȼ��ʼ����
				lst = GameEntry.Pool.DequeueClassObject<LinkedList<Action<AssetBundle>>>();
				lst.AddLast(onComplete);
				m_LoadingAssetBundle[assetbundlePath] = lst;
			}


			AssetBundleLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<AssetBundleLoaderRoutine>();
			if (routine == null) routine = new AssetBundleLoaderRoutine();

			//��������ʼѭ��
			m_AssetBundleLoaderList.AddLast(routine);

			//������Դ��
			routine.LoadAssetBundle(assetbundlePath);
			//��Դ������ ������ �ص�
			routine.OnAssetBundleCreateUpdate = onUpdate;
			//��Դ������ ���� �ص�
			routine.OnLoadAssetBundleComplete = (AssetBundle assetbundle) =>
			{
				//��Դ��ȡ��
				assetBundleEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
				assetBundleEntity.ResourceName = assetbundlePath;
				assetBundleEntity.IsAssetBundle = true;
				assetBundleEntity.Target = assetbundle;
				//��Դ��ע�ᵽ��Դ��
				GameEntry.Pool.AssetBundlePool.Register(assetBundleEntity);

				for (LinkedListNode<Action<AssetBundle>> curr = lst.First; curr != null; curr = curr.Next)
				{
					if (curr.Value != null) curr.Value(assetbundle);
				}
				//��Դ������Ϻ�
				lst.Clear();//�������
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
		private Dictionary<string, LinkedList<Action<UnityEngine.Object>>> m_LoadingAsset = new Dictionary<string, LinkedList<Action<UnityEngine.Object>>>();
		/// <summary>
		/// ����Դ���м�����Դ
		/// </summary>
		/// <param name="assetName"></param>
		/// <param name="assetBundle"></param>
		/// <param name="onUpdate"></param>
		/// <param name="onComplete"></param>
		public void LoadAsset(string assetName, AssetBundle assetBundle, Action<float> onUpdate = null, Action<UnityEngine.Object> onComplete = null)
		{
			//Debug.Log(assetName + "===========================================================");
			//1.�ж�Asset�Ƿ���ص�һ��,��ֹ�߲��������ظ�����
			LinkedList<Action<UnityEngine.Object>> lst = null;
			if (m_LoadingAsset.TryGetValue(assetName, out lst))
			{
				//���Asset�Ѿ��ڼ�����, ��ί�м����Ӧ������ Ȼ��ֱ��return;
				lst.AddLast(onComplete);
				return;
			}
			else
			{
				//���Asset��û�п�ʼ����, ��ί�м����Ӧ������ Ȼ��ʼ����
				lst = GameEntry.Pool.DequeueClassObject<LinkedList<Action<UnityEngine.Object>>>();
				lst.AddLast(onComplete);
				m_LoadingAsset[assetName] = lst;
			}


			AssetLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<AssetLoaderRoutine>();
			if (routine == null) routine = new AssetLoaderRoutine();

			//��������ʼѭ��
			m_AssetLoaderList.AddLast(routine);

			//������Դ
			routine.LoadAsset(assetName, assetBundle);
			//��Դ���� ������ �ص�
			routine.OnAssetUpdate = (float progress) =>
			{
				if (onUpdate != null) onUpdate(progress);
			};
			//��Դ���� ��� �ص�
			routine.OnLoadAssetComplete = (UnityEngine.Object obj) =>
			{
				for (LinkedListNode<Action<UnityEngine.Object>> curr = lst.First; curr != null; curr = curr.Next)
				{
					if (curr.Value != null) curr.Value(obj);
				}
				//��Դ������Ϻ�
				lst.Clear();//�������
				GameEntry.Pool.EnqueueClassObject(lst);
				m_LoadingAsset.Remove(assetName);//�Ӽ����е�Asset��Dic �Ƴ�

				//����ѭ�� �س�
				m_AssetLoaderList.Remove(routine);
				GameEntry.Pool.EnqueueClassObject(routine);
			};
		}
		#endregion
	}
}