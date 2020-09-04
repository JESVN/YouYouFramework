using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	/// <summary>
	/// ��Դʵ��(AssetBundle��Asset)
	/// </summary>
	public class ResourceEntity
	{
		/// <summary>
		/// ��Դ����
		/// </summary>
		public string ResourceName;

		/// <summary>
		/// ��Դ����(����Asset)
		/// </summary>
		public AssetCategory Category;

		/// <summary>
		/// �Ƿ�AssetBundle
		/// </summary>
		public bool IsAssetBundle;

		/// <summary>
		/// ����Ŀ��
		/// </summary>
		public object Target;

		/// <summary>
		/// �ϴ�ʹ��ʱ��
		/// </summary>
		public float LastUseTime { get; private set; }

		/// <summary>
		/// ���ü���
		/// </summary>
		public int ReferenceCount { get; private set; }

		/// <summary>
		/// ��������Դʵ������
		/// </summary>
		public LinkedList<ResourceEntity> DependsResourceList { get; private set; }

		public ResourceEntity()
		{
			DependsResourceList = new LinkedList<ResourceEntity>();
		}

		/// <summary>
		/// ����ȡ��
		/// </summary>
		public void Spawn()
		{
			LastUseTime = Time.time;

			if (!IsAssetBundle)
			{
				ReferenceCount++;
			}
			else
			{
				//�������������Դ�� ���ͷ�
				if (GameEntry.Pool.CheckAssetBundleIsLock(ResourceName))
				{
					ReferenceCount = 1;
				}
			}
		}

		/// <summary>
		/// ����س�
		/// </summary>
		public void Unspawn()
		{
			LastUseTime = Time.time;

			ReferenceCount--;
			if (ReferenceCount < 0)
			{
				ReferenceCount = 0;
			}
		}

		/// <summary>
		/// �����Ƿ�����ͷ�
		/// </summary>
		/// <returns></returns>
		public bool GetCanRelease()
		{
			if (ReferenceCount == 0 && Time.time - LastUseTime > GameEntry.Pool.ReleaseAssetBundleInterval)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// �ͷ���Դ
		/// </summary>
		public void Release()
		{
			ResourceName = null;
			ReferenceCount = 0;

			if (IsAssetBundle)
			{
				AssetBundle bundle = Target as AssetBundle;
				//GameEntry.Log(LogCategory.Resource, "ж������Դ��=>{0}", bundle.name);
				bundle.Unload(false);
			}
			Target = null;

			DependsResourceList.Clear();//����������Դʵ���������
			GameEntry.Pool.EnqueueClassObject(this);//��ǰ��Դʵ��س�
		}
	}
}