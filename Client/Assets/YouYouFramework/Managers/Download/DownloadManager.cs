using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// ���ع�����
	/// </summary>
	public class DownloadManager : ManagerBase
	{
		public int FlushSize { get; private set; }

		public int DownloadRoutineCount { get; private set; }

		/// <summary>
		/// ����ʧ�ܺ�����Դ���
		/// </summary>
		public int Retry { get; private set; }

		/// <summary>
		/// ���ص��ļ�������
		/// </summary>
		private LinkedList<DownloadRoutine> m_DownloadSingleRoutineList;

		/// <summary>
		/// ���ض��ļ�������
		/// </summary>
		private LinkedList<DownloadMulitRoutine> m_DownloadMulitRoutineList;

		public DownloadManager()
		{
			m_DownloadSingleRoutineList = new LinkedList<DownloadRoutine>();
			m_DownloadMulitRoutineList = new LinkedList<DownloadMulitRoutine>();
		}
		internal void Dispose()
		{
			m_DownloadSingleRoutineList.Clear();

			//�������ض��ļ�����Dispose()
			var mulitRoutine = m_DownloadMulitRoutineList.First;
			while (mulitRoutine != null)
			{
				mulitRoutine.Value.Dispose();
				mulitRoutine = mulitRoutine.Next;
			}
			m_DownloadMulitRoutineList.Clear();
		}
		/// <summary>
		/// ����
		/// </summary>
		public void OnUpdate()
		{
			//�������ص��ļ�����OnUpdate()
			var singleRoutine = m_DownloadSingleRoutineList.First;
			while (singleRoutine != null)
			{
				singleRoutine.Value.OnUpdate();
				singleRoutine = singleRoutine.Next;
			}

			//�������ض��ļ�����OnUpdate()
			var mulitRoutine = m_DownloadMulitRoutineList.First;
			while (mulitRoutine != null)
			{
				mulitRoutine.Value.OnUpdate();
				mulitRoutine = mulitRoutine.Next;
			}
		}
		public override void Init()
		{
			Retry = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Download_Retry, GameEntry.CurrDeviceGrade);
			DownloadRoutineCount = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Download_RoutineCount, GameEntry.CurrDeviceGrade);
			FlushSize = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Download_FlushSize, GameEntry.CurrDeviceGrade);
		}

		/// <summary>
		/// ���ص����ļ�
		/// </summary>
		/// <param name="url"></param>
		/// <param name="onUpdate"></param>
		public void BeginDownloadSingle(string url, BaseAction<string, ulong, float> onUpdate = null, BaseAction<string> onComplete = null)
		{
			AssetBundleInfoEntity entity = GameEntry.Resource.ResourceManager.GetAssetBundleInfo(url);
			if (entity == null)
			{
				GameEntry.LogError("��Ч��Դ��=>" + url);
				return;
			}

			DownloadRoutine routine = GameEntry.Pool.DequeueClassObject<DownloadRoutine>();
			routine.BeginDownload(url, entity, onUpdate, onComplete: (string fileUrl, DownloadRoutine r) =>
			{
				m_DownloadSingleRoutineList.Remove(routine);
				GameEntry.Pool.EnqueueClassObject(routine);
				if (onComplete != null) onComplete(fileUrl);
			});
			m_DownloadSingleRoutineList.AddLast(routine);
		}

		/// <summary>
		/// ���ض���ļ�
		/// </summary>
		/// <param name="lstUrl"></param>
		/// <param name="onDownloadMulitUpdate"></param>
		/// <param name="onDownloadMulitComplete"></param>
		public void BeginDownloadMulit(LinkedList<string> lstUrl, BaseAction<int, int, ulong, ulong> onDownloadMulitUpdate = null, BaseAction onDownloadMulitComplete = null)
		{
			DownloadMulitRoutine mulitRoutine = GameEntry.Pool.DequeueClassObject<DownloadMulitRoutine>();
			mulitRoutine.BeginDownloadMulit(lstUrl, onDownloadMulitUpdate, (DownloadMulitRoutine r) =>
			{
				m_DownloadMulitRoutineList.Remove(r);
				GameEntry.Pool.EnqueueClassObject(r);
				onDownloadMulitComplete?.Invoke();
			});
			m_DownloadMulitRoutineList.AddLast(mulitRoutine);
		}

	}
}
