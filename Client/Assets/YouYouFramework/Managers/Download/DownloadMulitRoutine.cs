using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

/// <summary>
/// ���ض��ļ���
/// </summary>
public class DownloadMulitRoutine : IDisposable
{
	public DownloadMulitRoutine()
	{
		m_DownloadMulitCurrSizeDic = new Dictionary<string, ulong>();
		m_DownloadRoutineList = new LinkedList<DownloadRoutine>();
		m_NeedDownloadList = new LinkedList<string>();
	}
	public void Dispose()
	{
		m_DownloadMulitCurrSizeDic.Clear();
		m_DownloadRoutineList.Clear();
		m_NeedDownloadList.Clear();
	}
	internal void OnUpdate()
	{
		var curr = m_DownloadRoutineList.First;
		while (curr != null)
		{
			curr.Value.OnUpdate();
			curr = curr.Next;
		}
	}

	/// <summary>
	/// ����������
	/// </summary>
	private LinkedList<DownloadRoutine> m_DownloadRoutineList;
	/// <summary>
	/// ��Ҫ���ص��ļ�����
	/// </summary>
	private LinkedList<string> m_NeedDownloadList;

	#region ���ض���ļ�
	/// <summary>
	/// ����ļ�������ί��
	/// </summary>
	private BaseAction<int, int, ulong, ulong> m_OnDownloadMulitUpdate;
	/// <summary>
	/// ����ļ��������ί��
	/// </summary>
	private BaseAction<DownloadMulitRoutine> m_OnDownloadMulitComplete;

	/// <summary>
	/// ����ļ�����_��Ҫ���ص�����
	/// </summary>
	private int m_DownloadMulitNeedCount = 0;
	/// <summary>
	/// ����ļ�����_��ǰ���ص�����
	/// </summary>
	private int m_DownloadMulitCurrCount = 0;
	/// <summary>
	/// ����ļ������ܴ�С(�ֽ�)
	/// </summary>
	private ulong m_DownloadMulitTotalSize = 0;
	/// <summary>
	/// ����ļ����ص�ǰ��С(�ֽ�)
	/// </summary>
	private ulong m_DownloadMulitCurrSize = 0;
	/// <summary>
	/// ����ļ����� ��ǰ��С
	/// </summary>
	private Dictionary<string, ulong> m_DownloadMulitCurrSizeDic;

	/// <summary>
	/// ���ض���ļ�
	/// </summary>
	/// <param name="lstUrl"></param>
	/// <param name="onDownloadMulitUpdate"></param>
	/// <param name="onDownloadMulitComplete"></param>
	internal void BeginDownloadMulit(LinkedList<string> lstUrl, BaseAction<int, int, ulong, ulong> onDownloadMulitUpdate, BaseAction<DownloadMulitRoutine> onDownloadMulitComplete)
	{
		if (lstUrl.Count < 1)
		{
			onDownloadMulitComplete?.Invoke(this);
			return;
		}
		m_OnDownloadMulitUpdate = onDownloadMulitUpdate;
		m_OnDownloadMulitComplete = onDownloadMulitComplete;

		m_NeedDownloadList.Clear();
		m_DownloadMulitCurrSizeDic.Clear();

		m_DownloadMulitNeedCount = 0;
		m_DownloadMulitCurrCount = 0;

		m_DownloadMulitTotalSize = 0;
		m_DownloadMulitCurrSize = 0;

		//1.����Ҫ���صļ������ض���
		for (LinkedListNode<string> item = lstUrl.First; item != null; item = item.Next)
		{
			string url = item.Value;
			AssetBundleInfoEntity entity = GameEntry.Resource.ResourceManager.GetAssetBundleInfo(url);
			if (entity != null)
			{
				m_DownloadMulitTotalSize += entity.Size;
				m_DownloadMulitNeedCount++;
				m_NeedDownloadList.AddLast(url);
				m_DownloadMulitCurrSizeDic[url] = 0;
			}
			else
			{
				GameEntry.LogError("��Ч��Դ��=>" + url);
			}
		}

		//����������
		int routineCount = Mathf.Min(GameEntry.Download.DownloadRoutineCount, m_DownloadMulitNeedCount);
		for (int i = 0; i < routineCount; i++)
		{
			DownloadRoutine routine = GameEntry.Pool.DequeueClassObject<DownloadRoutine>();

			string url = m_NeedDownloadList.First.Value;

			AssetBundleInfoEntity entity = GameEntry.Resource.ResourceManager.GetAssetBundleInfo(url);

			routine.BeginDownload(url, entity, OnDownloadMulitUpdate, OnDownloadMulitComplete);
			m_DownloadRoutineList.AddLast(routine);

			m_NeedDownloadList.RemoveFirst();
		}
	}
	private void OnDownloadMulitUpdate(string url, ulong currDownSize, float progress)
	{
		m_DownloadMulitCurrSizeDic[url] = currDownSize;

		ulong currSize = 0;
		var enumerator = m_DownloadMulitCurrSizeDic.GetEnumerator();
		while (enumerator.MoveNext())
		{
			currSize += enumerator.Current.Value;
		}

		m_DownloadMulitCurrSize = currSize;

		if (m_DownloadMulitCurrSize > m_DownloadMulitTotalSize) m_DownloadMulitCurrSize = m_DownloadMulitTotalSize;

		if (m_OnDownloadMulitUpdate != null) m_OnDownloadMulitUpdate(m_DownloadMulitCurrCount, m_DownloadMulitNeedCount, m_DownloadMulitCurrSize, m_DownloadMulitTotalSize);
	}
	private void OnDownloadMulitComplete(string fileUrl, DownloadRoutine routine)
	{
		//���������Ƿ���Ҫ���ص�����
		if (m_NeedDownloadList.Count > 0)
		{
			//����������������
			string url = m_NeedDownloadList.First.Value;

			AssetBundleInfoEntity entity = GameEntry.Resource.ResourceManager.GetAssetBundleInfo(url);
			routine.BeginDownload(url, entity, OnDownloadMulitUpdate, OnDownloadMulitComplete);

			m_NeedDownloadList.RemoveFirst();
		}
		else
		{
			//�������س�
			m_DownloadRoutineList.Remove(routine);
			GameEntry.Pool.EnqueueClassObject(routine);
		}

		m_DownloadMulitCurrCount++;

		if (m_OnDownloadMulitUpdate != null) m_OnDownloadMulitUpdate(m_DownloadMulitCurrCount, m_DownloadMulitNeedCount, m_DownloadMulitCurrSize, m_DownloadMulitTotalSize);

		if (m_DownloadMulitCurrCount == m_DownloadMulitNeedCount)
		{
			m_DownloadMulitCurrSize = m_DownloadMulitTotalSize;
			if (m_OnDownloadMulitUpdate != null) m_OnDownloadMulitUpdate(m_DownloadMulitCurrCount, m_DownloadMulitNeedCount, m_DownloadMulitCurrSize, m_DownloadMulitTotalSize);

			if (m_OnDownloadMulitComplete != null) m_OnDownloadMulitComplete(this);
			//Debug.LogError("������Դ�������!!!");
		}
	}
	#endregion
}
