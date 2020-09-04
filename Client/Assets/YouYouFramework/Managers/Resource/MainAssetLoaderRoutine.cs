using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// ����Դ������
	/// </summary>
	public class MainAssetLoaderRoutine
	{
		/// <summary>
		/// ��ǰ����Դ��Ϣ
		/// </summary>
		private AssetEntity m_CurrAssetEnity;

		/// <summary>
		/// ��ǰ����Դʵ��
		/// </summary>
		private ResourceEntity m_CurrResourceEntity;

		/// <summary>
		/// ��ǰ��Դ������ʵ������(��ʱ�洢)
		/// </summary>
		private LinkedList<ResourceEntity> m_DependResourceList = new LinkedList<ResourceEntity>();

		/// <summary>
		/// ��Ҫ���ص�������Դ����
		/// </summary>
		private int m_NeedLoadAssetDependCount = 0;

		/// <summary>
		/// ��ǰ�Ѿ����ص�������Դ����
		/// </summary>
		private int m_CurrLoadAssetDependCount = 0;

		/// <summary>
		/// ��ǰ����Դ������ �������
		/// </summary>
		private BaseAction<ResourceEntity> m_OnComplete;


		/// <summary>
		/// ��������Դ
		/// </summary>
		/// <param name="assetCategory"></param>
		/// <param name="assetFullName"></param>
		/// <param name="onComplete"></param>
		internal void Load(AssetCategory assetCategory, string assetFullName, BaseAction<ResourceEntity> onComplete)
		{
#if EDITORLOAD && UNITY_EDITOR
			m_CurrResourceEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
			m_CurrResourceEntity.Category = assetCategory;
			m_CurrResourceEntity.IsAssetBundle = false;
			m_CurrResourceEntity.ResourceName = assetFullName;
			m_CurrResourceEntity.Target = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetFullName);
			if (onComplete != null) onComplete(m_CurrResourceEntity);
#elif RESOURCES
			string resourcesPath = assetFullName.Split('.')[0].Replace("Assets/Download/", string.Empty);

			m_CurrResourceEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
			m_CurrResourceEntity.Category = assetCategory;
			m_CurrResourceEntity.IsAssetBundle = false;
			m_CurrResourceEntity.ResourceName = assetFullName;
			m_CurrResourceEntity.Target = Resources.Load(resourcesPath);
			if (onComplete != null) onComplete(m_CurrResourceEntity);
#else
			m_OnComplete = onComplete;
			m_CurrAssetEnity = GameEntry.Resource.ResourceLoaderManager.GetAssetEntity(assetCategory, assetFullName);
			if (m_CurrAssetEnity != null) LoadDependsAsset();
#endif
		}

		/// <summary>
		/// �����ļ�������Դ
		/// </summary>
		private void LoadMainAsset()
		{
			//1.�ӷ�����Դ��(AssetPool)�в���
			m_CurrResourceEntity = GameEntry.Pool.AssetPool[m_CurrAssetEnity.Category].Spawn(m_CurrAssetEnity.AssetFullName);
			if (m_CurrResourceEntity != null)
			{
				//GameEntry.Log(LogCategory.Resource, "�ӷ�����Դ���м���{0}", m_CurrResourceEntity.ResourceName);
				if (m_OnComplete != null) m_OnComplete(m_CurrResourceEntity);
				return;
			}

			//2.����Դ��
			GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(m_CurrAssetEnity.AssetBundleName, onComplete: (AssetBundle bundle) =>
			{
				//3.������Դ
				GameEntry.Resource.ResourceLoaderManager.LoadAsset(m_CurrAssetEnity.AssetFullName, bundle, onComplete: (UnityEngine.Object obj) =>
				  {
					  //4.�ٴμ�� ����Ҫ ��������ü��������
					  m_CurrResourceEntity = GameEntry.Pool.AssetPool[m_CurrAssetEnity.Category].Spawn(m_CurrAssetEnity.AssetFullName);
					  if (m_CurrResourceEntity != null)
					  {
						  if (m_OnComplete != null) m_OnComplete(m_CurrResourceEntity);
						  return;
					  }

					  //��Դ��ע����Դ
					  m_CurrResourceEntity = GameEntry.Pool.DequeueClassObject<ResourceEntity>();
					  m_CurrResourceEntity.Category = m_CurrAssetEnity.Category;
					  m_CurrResourceEntity.IsAssetBundle = false;
					  m_CurrResourceEntity.ResourceName = m_CurrAssetEnity.AssetFullName;
					  m_CurrResourceEntity.Target = obj;
					  GameEntry.Pool.AssetPool[m_CurrAssetEnity.Category].Register(m_CurrResourceEntity);

					  //���뵽�����Դ��������Դ������
					  var currDependsResource = m_DependResourceList.First;
					  while (currDependsResource != null)
					  {
						  var next = currDependsResource.Next;
						  m_DependResourceList.Remove(currDependsResource);
						  m_CurrResourceEntity.DependsResourceList.AddLast(currDependsResource);
						  currDependsResource = next;
					  }

					  //��ǰ����Դ������ �������(��ݹ�)
					  if (m_OnComplete != null) m_OnComplete(m_CurrResourceEntity);

					  Reset();
				  });
			});
		}

		/// <summary>
		/// ����������Դ
		/// </summary>
		private void LoadDependsAsset()
		{
			List<AssetDependsEntity> lst = m_CurrAssetEnity.DependsAssetList;
			if (lst != null)
			{
				int len = lst.Count;
				m_NeedLoadAssetDependCount = len;
				for (int i = 0; i < len; i++)
				{
					AssetDependsEntity entity = lst[i];
					MainAssetLoaderRoutine routine = GameEntry.Pool.DequeueClassObject<MainAssetLoaderRoutine>();
					routine.Load(entity.Category, entity.AssetFullName, OnLoadDependAssetComplete);
				}
			}
			else
			{
				//�����Դû������ ֱ�Ӽ�������Դ
				LoadMainAsset();
			}
		}

		/// <summary>
		/// ����ĳ��������Դ���
		/// </summary>
		/// <param name="res"></param>
		private void OnLoadDependAssetComplete(ResourceEntity res)
		{
			//���������Դ��������Դʵ�� ������ʱ����
			m_DependResourceList.AddLast(res);

			//�Ѽ��س�������Դ ���뵽�� ��Ҫ��
			m_CurrLoadAssetDependCount++;

			//�������Դ���������������
			if (m_NeedLoadAssetDependCount == m_CurrLoadAssetDependCount)
			{
				//���������Դ������Դ
				LoadMainAsset();
			}

		}

		/// <summary>
		/// ����
		/// </summary>
		private void Reset()
		{
			m_OnComplete = null;
			m_CurrAssetEnity = null;
			m_NeedLoadAssetDependCount = 0;
			m_CurrLoadAssetDependCount = 0;
			m_DependResourceList.Clear();
			GameEntry.Pool.EnqueueClassObject(this);
		}

	}
}