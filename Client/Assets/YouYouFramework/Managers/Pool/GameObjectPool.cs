//===================================================
//��    �ߣ�����  http://www.u3dol.com
//����ʱ�䣺
//��    ע��
//===================================================
using PathologicalGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	/// <summary>
	/// ��Ϸ��������
	/// </summary>
	public class GameObjectPool : IDisposable
	{
		/// <summary>
		/// ��Ϸ���������ֵ�
		/// </summary>
		private Dictionary<byte, GameObjectPoolEntity> m_SpawnPoolDic;

		/// <summary>
		/// ʵ��ID��Ӧ�����ID
		/// </summary>
		private Dictionary<int, byte> m_InstanceIdPoolIdDic;

		/// <summary>
		/// ����Ԥ��ض��� �൱�ڶ����Ԥ����ټ���һ���
		/// </summary>
		private Queue<PrefabPool> m_PrefabPoolQueue;

		public GameObjectPool()
		{
			m_SpawnPoolDic = new Dictionary<byte, GameObjectPoolEntity>();
			m_InstanceIdPoolIdDic = new Dictionary<int, byte>();
			m_PrefabPoolQueue = new Queue<PrefabPool>();

			InstanceHandler.InstantiateDelegates += this.InstantiateDelegate;
			InstanceHandler.DestroyDelegates += this.DestroyDelegate;
		}

		public void Dispose()
		{
			m_SpawnPoolDic.Clear();
		}

		/// <summary>
		/// ����������崴��ʱ��
		/// </summary>
		/// <param name="prefab"></param>
		/// <param name="pos"></param>
		/// <param name="rot"></param>
		/// <param name="userData"></param>
		/// <returns></returns>
		public GameObject InstantiateDelegate(GameObject prefab, Vector3 pos, Quaternion rot, object userData)
		{
			ResourceEntity resourceEntity = userData as ResourceEntity;

			if (resourceEntity == null)
			{
				Debug.LogError("��Դ��Ϣ������ resourceEntity=" + resourceEntity.ResourceName);
				return null;
			}

			GameObject obj = UnityEngine.Object.Instantiate(prefab, pos, rot) as GameObject;

			//ע��
			GameEntry.Pool.RegisterInstanceResource(obj.GetInstanceID(), resourceEntity);
			return obj;
		}

		/// <summary>
		/// ���������������ʱ��
		/// </summary>
		/// <param name="instance"></param>
		public void DestroyDelegate(GameObject instance)
		{
			UnityEngine.Object.Destroy(instance);
			GameEntry.Resource.ResourceLoaderManager.UnLoadGameObject(instance);
		}

		#region Init ��ʼ��
		/// <summary>
		/// ��ʼ��
		/// </summary>
		/// <param name="arr"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public IEnumerator Init(GameObjectPoolEntity[] arr, Transform parent)
		{
			int len = arr.Length;
			for (int i = 0; i < len; i++)
			{
				GameObjectPoolEntity entity = arr[i];

				if (entity.Pool != null)
				{
					UnityEngine.Object.Destroy(entity.Pool.gameObject);
					yield return null;
					entity.Pool = null;
				}

				//���������
				SpawnPool pool = PathologicalGames.PoolManager.Pools.Create(entity.PoolName);
				pool.group.parent = parent;
				pool.group.localPosition = Vector3.zero;
				entity.Pool = pool;

				m_SpawnPoolDic[entity.PoolId] = entity;
			}
		}
		#endregion

		#region Spawn �Ӷ�����л�ȡ����
		private Dictionary<int, HashSet<Action<SpawnPool, Transform, ResourceEntity>>> m_LoadingPrefabPoolDic = new Dictionary<int, HashSet<Action<SpawnPool, Transform, ResourceEntity>>>();
		/// <summary>
		/// �Ӷ�����л�ȡ����
		/// </summary>
		/// <param name="prefabId">Ԥ����</param>
		/// <param name="onComplete"></param>
		public void Spawn(Sys_PrefabEntity entity, BaseAction<Transform, bool> onComplete)
		{
			lock (m_PrefabPoolQueue)
			{
				//�õ������
				GameObjectPoolEntity gameObjectPoolEntity = m_SpawnPoolDic[(byte)entity.PoolId];

				//ʹ��Ԥ���� ������ID
				PrefabPool prefabPool = gameObjectPoolEntity.Pool.GetPrefabPool(entity.Id);
				if (prefabPool != null)
				{
					//�õ�һ��ʵ�� ����һ�����е�
					Transform retTrans = prefabPool.TrySpawnInstance();
					if (retTrans != null)
					{
						int instanceID = retTrans.gameObject.GetInstanceID();
						m_InstanceIdPoolIdDic[instanceID] = entity.PoolId;
						onComplete?.Invoke(retTrans, false);
						return;
					}
				}
				HashSet<Action<SpawnPool, Transform, ResourceEntity>> lst = null;
				if (m_LoadingPrefabPoolDic.TryGetValue(entity.Id, out lst))
				{
					//��������
					//������ڼ����е�Asset ��ί�м����Ӧ������ Ȼ��ֱ�ӷ���
					lst.Add((_SpawnPool, _Transform, _ResourceEntity) =>
					{
						//�õ�һ��ʵ��
						bool isNewInstance = false;
						Transform retTrans = _SpawnPool.Spawn(_Transform, ref isNewInstance, _ResourceEntity);
						int instanceID = retTrans.gameObject.GetInstanceID();
						m_InstanceIdPoolIdDic[instanceID] = entity.PoolId;
						onComplete?.Invoke(retTrans, isNewInstance);
					});
					return;
				}

				//����˵���Ǽ����ڵ�һ��
				lst = GameEntry.Pool.DequeueClassObject<HashSet<Action<SpawnPool, Transform, ResourceEntity>>>();
				lst.Add((_SpawnPool, _Transform, _ResourceEntity) =>
				{
					//�õ�һ��ʵ��
					bool isNewInstance = false;
					Transform retTrans = _SpawnPool.Spawn(_Transform, ref isNewInstance, _ResourceEntity);
					int instanceID = retTrans.gameObject.GetInstanceID();
					m_InstanceIdPoolIdDic[instanceID] = entity.PoolId;
					onComplete?.Invoke(retTrans, isNewInstance);
				});
				m_LoadingPrefabPoolDic[entity.Id] = lst;

				GameEntry.Resource.ResourceLoaderManager.LoadMainAsset((AssetCategory)entity.AssetCategory, entity.AssetFullName, (ResourceEntity resourceEntity) =>
				{
					GameObject retObj = resourceEntity.Target as GameObject;
					Transform prefab = retObj.transform;

					if (prefabPool == null)
					{
						//��ȥ�������� ���еĳ�
						if (m_PrefabPoolQueue.Count > 0)
						{
							prefabPool = m_PrefabPoolQueue.Dequeue();

							prefabPool.PrefabPoolId = entity.Id; //����Ԥ��ر��
							gameObjectPoolEntity.Pool.AddPrefabPool(prefabPool);

							prefabPool.prefab = prefab;
							prefabPool.prefabGO = prefab.gameObject;
							prefabPool.AddPrefabToDic(prefab.name, prefab);
						}
						else
						{
							prefabPool = new PrefabPool(prefab, entity.Id);
							gameObjectPoolEntity.Pool.CreatePrefabPool(prefabPool, resourceEntity);
						}

						prefabPool.OnPrefabPoolClear = (PrefabPool pool) =>
						{
							//Ԥ��ؼ������
							pool.PrefabPoolId = 0;
							gameObjectPoolEntity.Pool.RemovePrefabPool(pool);
							m_PrefabPoolQueue.Enqueue(pool);
						};

						//��Щ����Ҫ�ӱ���ж�ȡ
						prefabPool.cullDespawned = entity.CullDespawned == 1;
						prefabPool.cullAbove = entity.CullAbove;
						prefabPool.cullDelay = entity.CullDelay;
						prefabPool.cullMaxPerPass = entity.CullMaxPerPass;

					}
					var enumerator = lst.GetEnumerator();
					while (enumerator.MoveNext())
					{
						enumerator.Current?.Invoke(gameObjectPoolEntity.Pool, prefab, resourceEntity);
					}
					m_LoadingPrefabPoolDic.Remove(entity.Id);
					lst.Clear();//һ��Ҫ���
					GameEntry.Pool.EnqueueClassObject(lst);
				});
			}
		}
		#endregion

		#region Despawn ����س�
		/// <summary>
		/// ����س�
		/// </summary>
		/// <param name="poolId"></param>
		/// <param name="instance">ʵ��</param>
		internal void Despawn(byte poolId, Transform instance)
		{
			GameObjectPoolEntity entity = m_SpawnPoolDic[poolId];
			instance.SetParent(entity.Pool.transform);
			entity.Pool.Despawn(instance);
		}

		/// <summary>
		/// ����س�
		/// </summary>
		/// <param name="instance">ʵ��</param>
		public void Despawn(Transform instance)
		{
			int instanceID = instance.gameObject.GetInstanceID();
			byte poolId = m_InstanceIdPoolIdDic[instanceID];
			m_InstanceIdPoolIdDic.Remove(instanceID);
			Despawn(poolId, instance);
		}
		#endregion
	}
}