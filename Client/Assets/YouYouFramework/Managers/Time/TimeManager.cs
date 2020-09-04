using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	public class TimeManager : ManagerBase, IDisposable
	{
		/// <summary>
		/// ��ʱ������
		/// </summary>
		private LinkedList<TimeAction> m_TimeActionList;

		internal TimeManager()
		{
			m_TimeActionList = new LinkedList<TimeAction>();

		}
		public void Dispose()
		{
			m_TimeActionList.Clear();
		}
		public override void Init()
		{
		}
		internal void OnUpdate()
		{
			for (LinkedListNode<TimeAction> curr = m_TimeActionList.First; curr != null; curr = curr.Next)
			{
				if (curr.Value.OnStarAction.Target == null || curr.Value.OnStarAction.Target.ToString() == "null")
				{
					RemoveTimeAction(curr.Value);
					continue;
				}
				if (curr.Value.OnUpdateAction.Target == null || curr.Value.OnUpdateAction.Target.ToString() == "null")
				{
					RemoveTimeAction(curr.Value);
					continue;
				}
				if (curr.Value.OnCompleteAction.Target == null || curr.Value.OnCompleteAction.Target.ToString() == "null")
				{
					RemoveTimeAction(curr.Value);
					continue;
				}
				curr.Value.OnUpdate();
			}
		}


		/// <summary>
		/// ע�ᶨʱ��
		/// </summary>
		/// <param name="action"></param>
		internal void RegisterTimeAction(TimeAction action)
		{
			m_TimeActionList.AddLast(action);
		}
		/// <summary>
		/// �Ƴ���ʱ��
		/// </summary>
		/// <param name="action"></param>
		internal void RemoveTimeAction(TimeAction action)
		{
			m_TimeActionList.Remove(action);
			GameEntry.Pool.EnqueueClassObject(action);
		}
		/// <summary>
		/// ���ݶ�ʱ������ ɾ����ʱ��
		/// </summary>
		/// <param name="timeName"></param>
		public void RemoveTimeActionByName(string timeName)
		{
			LinkedListNode<TimeAction> curr = m_TimeActionList.First;
			while (curr != null)
			{
				if (curr.Value.TimeName.Equals(timeName, StringComparison.CurrentCultureIgnoreCase))
				{
					RemoveTimeAction(curr.Value);
					break;
				}
				curr = curr.Next;
			}
		}

		/// <summary>
		/// ������ʱ��
		/// </summary>
		/// <returns></returns>
		public TimeAction CreateTimeAction()
		{
			return GameEntry.Pool.DequeueClassObject<TimeAction>();
		}

		/// <summary>
		/// �ӳ�һ֡
		/// </summary>
		/// <param name="onComplete"></param>
		public void Yield(BaseAction onComplete)
		{
			GameEntry.Instance.StartCoroutine(YieldCoroutine(onComplete));
		}
		private IEnumerator YieldCoroutine(BaseAction onComplete)
		{
			yield return null;
			if (onComplete != null) onComplete();
		}

		/// <summary>
		/// �޸�ʱ������
		/// </summary>
		/// <param name="toTimeScale">���ŵ�ֵ</param>
		/// <param name="continueTime">����ʱ��</param>
		public void ChangeTimeScale(float toTimeScale, float continueTime)
		{
			Time.timeScale = toTimeScale;
			GameEntry.Time.CreateTimeAction().Init(null, continueTime, 0, 0, () =>
			 {
				 Time.timeScale = 1;
			 }).Run();
		}



	}
}