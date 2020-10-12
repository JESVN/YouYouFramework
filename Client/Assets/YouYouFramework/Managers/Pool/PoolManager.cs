using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YouYou
{
	/// <summary>
	/// �ع�����
	/// </summary>
	public class PoolManager : ManagerBase, IDisposable
	{
		/// <summary>
		/// ������
		/// </summary>
		public ClassObjectPool ClassObjectPool { get; private set; }
		/// <summary>
		/// ��Ϸ��������
		/// </summary>
		private GameObjectPool GameObjectPool;
		/// <summary>
		/// ��Դ����
		/// </summary>
		public ResourcePool AssetBundlePool { get; private set; }
		/// <summary>
		/// ������Դ��
		/// </summary>
		public Dictionary<AssetCategory, ResourcePool> AssetPool { get; private set; }

		internal PoolManager()
		{
			ClassObjectPool = new ClassObjectPool();
			GameObjectPool = new GameObjectPool();

			AssetBundlePool = new ResourcePool("AssetBundlePool");
			m_InstanceResourceDic = new Dictionary<int, ResourceEntity>();
			AssetPool = new Dictionary<AssetCategory, ResourcePool>();
		}

		/// <summary>
		/// ��ʼ��
		/// </summary>
		internal override void Init()
		{
			ReleaseClassObjectInterval = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Pool_ReleaseClassObjectInterval, GameEntry.CurrDeviceGrade);
			ReleaseAssetBundleInterval = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Pool_ReleaseAssetBundleInterval, GameEntry.CurrDeviceGrade);
			ReleaseAssetInterval = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Pool_ReleaseAssetInterval, GameEntry.CurrDeviceGrade);

			//ȷ����Ϸ�տ�ʼ���е�ʱ�� ������Դ���Ѿ���ʼ������
			var enumerator = Enum.GetValues(typeof(AssetCategory)).GetEnumerator();
			while (enumerator.MoveNext())
			{
				AssetCategory assetCategory = (AssetCategory)enumerator.Current;
				AssetPool[assetCategory] = new ResourcePool(assetCategory.ToString());
			}

			ReleaseClassObjectNextRunTime = Time.time;
			ReleaseAssetBundleNextRunTime = Time.time;
			ReleaseAssetNextRunTime = Time.time;

			InitGameObjectPool();
			m_LockedAssetBundleLength = GameEntry.Instance.LockedAssetBundle.Length;
			InitClassReside();
		}

		/// <summary>
		/// �ͷ�������
		/// </summary>
		public void ReleaseClassObjectPool()
		{
			ClassObjectPool.Release();
		}

		/// <summary>
		/// �ͷ���Դ����
		/// </summary>
		public void ReleaseAssetBundlePool()
		{
			AssetBundlePool.Release();
		}

		/// <summary>
		/// �ͷŷ�����Դ����������Դ
		/// </summary>
		public void ReleaseAssetPool()
		{
			var enumerator = Enum.GetValues(typeof(AssetCategory)).GetEnumerator();
			while (enumerator.MoveNext())
			{
				AssetCategory assetCategory = (AssetCategory)enumerator.Current;
				AssetPool[assetCategory].Release();
			}
		}

		public void Dispose()
		{
			ClassObjectPool.Dispose();
			GameObjectPool.Dispose();
		}

		//============================


		/// <summary>
		/// ��������Դ�����鳤��
		/// </summary>
		private int m_LockedAssetBundleLength;

		/// <summary>
		/// �����Դ���Ƿ�����
		/// </summary>
		/// <param name="assetBundleName">��Դ������</param>
		/// <returns></returns>
		public bool CheckAssetBundleIsLock(string assetBundleName)
		{
			for (int i = 0; i < m_LockedAssetBundleLength; i++)
			{
				if (GameEntry.Instance.LockedAssetBundle[i].Equals(assetBundleName, StringComparison.CurrentCultureIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// ��ʼ�������ೣפ����
		/// </summary>
		private void InitClassReside()
		{
			SetClassObjectResideCount<HttpRoutine>(3);
			SetClassObjectResideCount<Dictionary<string, object>>(3);
			SetClassObjectResideCount<AssetBundleLoaderRoutine>(10);
			SetClassObjectResideCount<AssetLoaderRoutine>(10);
			SetClassObjectResideCount<ResourceEntity>(10);
			SetClassObjectResideCount<MainAssetLoaderRoutine>(30);
		}
		/// <summary>
		/// �����ೣפ����
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="count"></param>
		public void SetClassObjectResideCount<T>(byte count) where T : class
		{
			ClassObjectPool.SetResideCount<T>(count);
		}

		#region DequeueClassObject ȡ��һ������
		/// <summary>
		/// ȡ��һ������
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T DequeueClassObject<T>() where T : class, new()
		{
			return ClassObjectPool.Dequeue<T>();
		}
		#endregion

		#region EnqueueClassObject ����س�
		/// <summary>
		/// ����س�
		/// </summary>
		/// <param name="obj"></param>
		public void EnqueueClassObject(object obj)
		{
			ClassObjectPool.Enqueue(obj);
		}
		#endregion

		#region ���������

		/// <summary>
		/// �����������
		/// </summary>
		private object m_VarObjectLock = new object();

#if UNITY_EDITOR
		/// <summary>
		/// �ڼ��������ʾ����Ϣ
		/// </summary>
		public Dictionary<Type, int> VarObjectInspectorDic = new Dictionary<Type, int>();
#endif

		/// <summary>
		/// ȡ��һ����������
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T DequeueVarObject<T>() where T : VariableBase, new()
		{
			lock (m_VarObjectLock)
			{
				T item = ClassObjectPool.Dequeue<T>();
#if UNITY_EDITOR
				Type t = item.GetType();
				if (VarObjectInspectorDic.ContainsKey(t))
				{
					VarObjectInspectorDic[t]++;
				}
				else
				{
					VarObjectInspectorDic[t] = 1;
				}
#endif
				return item;
			}
		}

		/// <summary>
		/// ��������س�
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="item"></param>
		public void EnqueueVarObject<T>(T item) where T : VariableBase
		{
			lock (m_VarObjectLock)
			{
				ClassObjectPool.Enqueue(item);
#if UNITY_EDITOR
				Type t = item.GetType();
				if (VarObjectInspectorDic.ContainsKey(t))
				{
					VarObjectInspectorDic[t]--;
					if (VarObjectInspectorDic[t] == 0)
					{
						VarObjectInspectorDic.Remove(t);
					}
				}
#endif
			}
		}

		#endregion

		/// <summary>
		/// �ͷ������ؼ��
		/// </summary>
		public int ReleaseClassObjectInterval
		{
			get;
			private set;
		}

		/// <summary>
		/// �´��ͷ����������ʱ��
		/// </summary>
		public float ReleaseClassObjectNextRunTime
		{
			get;
			private set;
		}


		/// <summary>
		/// �ͷ�AssetBundle�ؼ��
		/// </summary>
		public int ReleaseAssetBundleInterval
		{
			get;
			private set;
		}

		/// <summary>
		/// �´��ͷ�AssetBundle������ʱ��
		/// </summary>
		public float ReleaseAssetBundleNextRunTime
		{
			get;
			private set;
		}

		/// <summary>
		/// �ͷ�Asset�ؼ��
		/// </summary>
		public int ReleaseAssetInterval
		{
			get;
			private set;
		}

		/// <summary>
		/// �´��ͷ�Asset������ʱ��
		/// </summary>
		public float ReleaseAssetNextRunTime
		{
			get;
			private set;
		}

		internal void OnUpdate()
		{
			if (Time.time > ReleaseClassObjectNextRunTime + ReleaseClassObjectInterval)
			{
				ReleaseClassObjectNextRunTime = Time.time;
				ReleaseClassObjectPool();
				GameEntry.Log(LogCategory.Normal, "�ͷ�������");
			}


			if (Time.time > ReleaseAssetBundleNextRunTime + ReleaseAssetBundleInterval)
			{
				ReleaseAssetBundleNextRunTime = Time.time;

#if !EDITORLOAD
				ReleaseAssetBundlePool();
				GameEntry.Log(LogCategory.Normal, "�ͷ�AssetBundle��");
#endif
			}

			if (Time.time > ReleaseAssetNextRunTime + ReleaseAssetInterval)
			{
				ReleaseAssetNextRunTime = Time.time;

#if !EDITORLOAD
				ReleaseAssetPool();
				GameEntry.Log(LogCategory.Normal, "�ͷ�Asset��");
#endif
				GameEntry.Event.CommonEvent.Dispatch(SysEventId.LuaFullGc);

#if !UNLOADRES_CHANGESCENE
				if (LuaManager.luaEnv != null)
				{
					LuaManager.luaEnv.FullGc();
				}
				Resources.UnloadUnusedAssets();
#endif
			}
		}

		#region ��Ϸ��������

		/// <summary>
		/// ��ʼ����Ϸ��������
		/// </summary>
		private void InitGameObjectPool()
		{
			GameEntry.Instance.StartCoroutine(GameObjectPool.Init(GameEntry.Instance.GameObjectPoolGroups, GameEntry.Instance.PoolParent));
		}

		/// <summary>
		/// �Ӷ�����л�ȡ����
		/// </summary>
		/// <param name="prefabId">Ԥ����</param>
		/// <param name="onComplete"></param>
		public void GameObjectSpawn(int prefabId, BaseAction<Transform, bool> onComplete)
		{
			GameObjectSpawn(GameEntry.DataTable.Sys_PrefabDBModel.GetDic(prefabId), onComplete);
		}
		public void GameObjectSpawn(string prefabName, BaseAction<Transform, bool> onComplete)
		{
			GameObjectSpawn(GameEntry.DataTable.Sys_PrefabDBModel.GetPrefabIdByName(prefabName), onComplete);
		}
		public void GameObjectSpawn(Sys_PrefabEntity sys_PrefabEntity, BaseAction<Transform, bool> onComplete)
		{
			if (sys_PrefabEntity == null)
			{
				GameEntry.LogError("Ԥ�����ݲ�����,sys_PrefabEntity==null!");
				return;
			}
			GameObjectPool.Spawn(sys_PrefabEntity, onComplete);
		}

		/// <summary>
		/// ����س�
		/// </summary>
		/// <param name="instance">ʵ��</param>
		public void GameObjectDespawn(Transform instance)
		{
			GameObjectPool.Despawn(instance);
		}
		#endregion

		#region ʵ������ͷ�����Դ���ͷ�
		/// <summary>
		/// ��¡������ʵ����Դ�ֵ�
		/// </summary>
		private Dictionary<int, ResourceEntity> m_InstanceResourceDic;

		/// <summary>
		/// ע�ᵽʵ���ֵ�
		/// </summary>
		/// <param name="instanceId"></param>
		/// <param name="resourceEntity"></param>
		public void RegisterInstanceResource(int instanceId, ResourceEntity resourceEntity)
		{
			//Debug.LogError("ע�ᵽʵ���ֵ�instanceId=" + instanceId);
			m_InstanceResourceDic[instanceId] = resourceEntity;
		}

		/// <summary>
		/// �ͷ�ʵ����Դ
		/// </summary>
		/// <param name="instanceId"></param>
		public void ReleaseInstanceResource(int instanceId)
		{
			//Debug.LogError("�ͷ�ʵ����ԴinstanceId=" + instanceId);
			ResourceEntity resourceEntity = null;
			if (m_InstanceResourceDic.TryGetValue(instanceId, out resourceEntity))
			{
#if EDITORLOAD
				resourceEntity.Target = null;
				GameEntry.Pool.EnqueueClassObject(resourceEntity);
#else
				UnspawnResourceEntity(resourceEntity);
#endif
				m_InstanceResourceDic.Remove(instanceId);
			}
		}

		/// <summary>
		/// ��Դʵ��س�
		/// </summary>
		/// <param name="entity"></param>
		private void UnspawnResourceEntity(ResourceEntity entity)
		{
			var curr = entity.DependsResourceList.First;
			while (curr != null)
			{
				UnspawnResourceEntity(curr.Value);
				curr = curr.Next;
			}

			AssetPool[entity.Category].Unspawn(entity.ResourceName);
		}
		#endregion
	}
}