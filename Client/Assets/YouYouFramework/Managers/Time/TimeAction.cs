using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	/// <summary>
	/// ��ʱ��
	/// </summary>
	public class TimeAction
	{
		/// <summary>
		/// ��ʱ��������
		/// </summary>
		public string TimeName
		{
			get;
			private set;
		}

		/// <summary>
		/// �Ƿ�������
		/// </summary>
		public bool IsRuning
		{
			get;
			private set;
		}

		/// <summary>
		/// �Ƿ���ͣ
		/// </summary>
		public bool m_IsPause = false;

		/// <summary>
		/// ��ǰ���е�ʱ��
		/// </summary>
		private float m_CurrRunTime;

		/// <summary>
		/// ��ǰѭ������
		/// </summary>
		private int m_CurrLoop;

		/// <summary>
		/// �ӳ�ʱ��
		/// </summary>
		private float m_DelayTime;

		/// <summary>
		/// ������룩
		/// </summary>
		private float m_Interval;

		/// <summary>
		/// ѭ������(-1��ʾ ����ѭ�� 0Ҳ��ѭ��һ��)
		/// </summary>
		private int m_Loop;

		/// <summary>
		/// �����ͣʱ��
		/// </summary>
		private float m_LastPauseTime;

		/// <summary>
		/// ��ͣ�˶��
		/// </summary>
		private float m_PauseTime;

		/// <summary>
		/// ��ʼ����
		/// </summary>
		public Action OnStarAction
		{
			get;
			private set;
		}

		/// <summary>
		/// ������ �ص�������ʾʣ�����
		/// </summary>
		public Action<int> OnUpdateAction
		{
			get;
			private set;
		}

		/// <summary>
		/// �������
		/// </summary>
		public Action OnCompleteAction
		{
			get;
			private set;
		}

		/// <summary>
		/// ��ʼ��
		/// </summary>
		/// <param name="timeName">��ʱ������</param>
		/// <param name="delayTime">�ӳ�ʱ��</param>
		/// <param name="interval">���</param>
		/// <param name="loop">ѭ������</param>
		/// <param name="onStar"></param>
		/// <param name="onUpdate"></param>
		/// <param name="onComplete"></param>
		/// <returns></returns>
		public TimeAction Init(string timeName = null, float delayTime = 0, float interval = 1, int loop = 0,
			Action onStar = null, Action<int> onUpdate = null, Action onComplete = null)
		{
			TimeName = timeName;
			m_DelayTime = delayTime;
			m_Interval = interval;
			m_Loop = loop;
			OnStarAction = onStar;
			OnUpdateAction = onUpdate;
			OnCompleteAction = onComplete;

			return this;
		}

		/// <summary>
		/// ����
		/// </summary>
		public void Run()
		{
			//1.��Ҫ�Ȱ��Լ�����ʱ���������������
			GameEntry.Time.RegisterTimeAction(this);

			//2.���õ�ǰ���е�ʱ��
			m_CurrRunTime = Time.realtimeSinceStartup;
m_CurrLoop = 0;
			m_IsPause = false;
		}

		/// <summary>
		/// ֹͣ
		/// </summary>
		public void Stop()
		{
			IsRuning = false;

			//���Լ��Ӷ�ʱ�������Ƴ�
			GameEntry.Time.RemoveTimeAction(this);
		}

		/// <summary>
		/// ��ͣ
		/// </summary>
		public void Pause()
		{
			m_LastPauseTime = Time.realtimeSinceStartup;
			m_IsPause = true;
		}

		/// <summary>
		/// �ָ�
		/// </summary>
		public void Resume()
		{

			m_IsPause = false;

			//������ͣ�˶��
			m_PauseTime = Time.realtimeSinceStartup - m_LastPauseTime;
		}


		internal void OnUpdate()
		{
			if (m_IsPause) return;

			//1.�ȴ��ӳ�ʱ��
			if (Time.realtimeSinceStartup > m_CurrRunTime + m_PauseTime + m_DelayTime)
			{
				if (!IsRuning)
				{
					//��ʼ����
					m_CurrRunTime = Time.realtimeSinceStartup;
					m_PauseTime = 0;
					OnStarAction?.Invoke();
				}
				IsRuning = true;
			}

			if (!IsRuning) return;

			if (Time.realtimeSinceStartup > m_CurrRunTime + m_PauseTime)
			{
				m_CurrRunTime = Time.realtimeSinceStartup + m_Interval;
				m_PauseTime = 0;
				//���´��� ���m_Interval ʱ�� ִ��һ��
				OnUpdateAction?.Invoke(m_Loop - m_CurrLoop);

				if (m_Loop > -1)
				{
					if (m_CurrLoop >= m_Loop)
					{
						OnCompleteAction?.Invoke();
						Stop();
					}
					m_CurrLoop++;
				}
			}
		}
	}
}