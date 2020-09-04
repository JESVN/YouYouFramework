using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YouYou;
using System;

public class RoleDataManager : IDisposable
{
	private LinkedList<RoleCtrl> m_RoleList;
	public RoleDataManager()
	{
		m_RoleList = new LinkedList<RoleCtrl>();
	}

	public void CreatePlayerByJobId(int jobId, Action<RoleCtrl> onComplete = null)
	{
		//Ƥ�����
		int skinId = GameEntry.DataTable.JobDBModel.GetDic(jobId).SkinId;

		//���ؽ�ɫ������
		GameEntry.Pool.GameObjectSpawn(SysPrefabId.RoleCtrl, (Transform trans, bool isNewInstance) =>
		 {
			 RoleCtrl roleCtrl = trans.GetComponent<RoleCtrl>();
			 roleCtrl.Init(skinId);

			 if (!isNewInstance)
			 {
				 //���������ʵ�� ������ִ��OnOpen����
				 roleCtrl.OnOpen();
			 }
			 m_RoleList.AddLast(roleCtrl);
			 onComplete?.Invoke(roleCtrl);
		 });
	}
	public void DespawnRole(RoleCtrl roleCtrl)
	{
		//��ִ�н�ɫ�سصķ��� �ѽ�ɫ��������������س�
		roleCtrl.OnClose();

		//Ȼ��سؽ�ɫ
		GameEntry.Pool.GameObjectDespawn(roleCtrl.transform);
		m_RoleList.Remove(roleCtrl);
	}

	public void DespawnAllRole()
	{
		for (LinkedListNode<RoleCtrl> curr = m_RoleList.First; curr != null;)
		{
			LinkedListNode<RoleCtrl> next = curr.Next;
			DespawnRole(curr.Value);
			curr = next;
		}
	}

	public void Dispose()
	{

	}
}
