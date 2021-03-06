using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
	[RequireComponent(typeof(Canvas))]//�ű�����
	[RequireComponent(typeof(GraphicRaycaster))]//�ű�����
	public class UIFormBase : MonoBehaviour
	{
		/// <summary>
		/// �Ƿ��Ծ
		/// </summary>
		protected internal bool IsActive;

		public Sys_UIFormEntity SysUIForm { get; private set; }

		/// <summary>
		/// ��ǰ����
		/// </summary>
		public Canvas CurrCanvas
		{
			get;
			private set;
		}

		/// <summary>
		/// �ر�ʱ��
		/// </summary>
		public float CloseTime
		{
			get;
			private set;
		}

		/// <summary>
		/// �û�����
		/// </summary>
		public object UserData
		{
			get;
			private set;
		}

		private BaseAction m_InitComplate;

		void Awake()
		{
			if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
			CurrCanvas = GetComponent<Canvas>();
		}

		internal void Init(Sys_UIFormEntity sysUIForm, object userData, BaseAction initComplate)
		{
			SysUIForm = sysUIForm;
			UserData = userData;
			m_InitComplate = initComplate;
		}

		void Start()
		{
			OnInit(UserData);
			m_InitComplate?.Invoke();
			Open(UserData, true);
		}

		internal void Open(object userData, bool isFormInit = false)
		{
			//GameEntry.Audio.PlayAudio(YFConstDefine.Audio_UIOpen);
			if (!isFormInit)
			{
				UserData = userData;
			}


			if (SysUIForm != null && SysUIForm.DisableUILayer != 1)
			{
				//���в㼶���� ���Ӳ㼶
				GameEntry.UI.SetSortingOrder(this, true);
			}
			OnOpen(UserData);
		}

		public void Close()
		{
			GameEntry.UI.CloseUIForm(this);
		}


		internal void ToClose()
		{
			//GameEntry.Audio.PlayAudio(YFConstDefine.Audio_UIClose);
			if (SysUIForm != null && SysUIForm.DisableUILayer != 1)
			{
				//���в㼶���� ���ٲ㼶
				GameEntry.UI.SetSortingOrder(this, false);
			}

			OnClose();

			CloseTime = Time.time;
			GameEntry.UI.EnQueue(this);
		}

		void OnDestroy()
		{
			OnBeforDestroy();
		}

		protected virtual void OnInit(object userData) { }
		protected virtual void OnOpen(object userData) { }
		protected virtual void OnClose() { }
		protected virtual void OnBeforDestroy() { }

	}
}