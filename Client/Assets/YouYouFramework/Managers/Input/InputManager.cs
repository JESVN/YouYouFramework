using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	public class InputManager : ManagerBase, IDisposable
	{
		private InputCtrlBase m_InputCtrl;
		/// <summary>
		/// ���²�̧�� ��궼��ͬһλ�� �򴥷�һ��
		/// </summary>
		public event BaseAction<TouchEventData> OnClick;
		/// <summary>
		/// ���� ����һ��
		/// </summary>
		public event BaseAction<TouchEventData> OnBeginDrag;
		/// <summary>
		/// ̧�� ����һ��
		/// </summary>
		public event BaseAction<TouchEventData> OnEndDrag;
		/// <summary>
		/// ��ק���� Axis!=(0,0)ʱ,Ҳ�����з���Ļ� ��������
		/// </summary>
		public event BaseAction<TouchDirection, TouchEventData> OnDrag;
		/// <summary>
		/// �Ŵ���С Axis!=0ʱ, ��������
		/// </summary>
		public event BaseAction<ZoomType> OnZoom;

		internal override void Init()
		{
#if UNITY_EDITOR || UNITY_STANDALONE
			m_InputCtrl = new StandalonInputCtrl(t => OnClick?.Invoke(t),
				t => OnBeginDrag?.Invoke(t),
				t => OnEndDrag?.Invoke(t),
				(t1, t2) => OnDrag?.Invoke(t1, t2),
				t => OnZoom?.Invoke(t));
#else
			m_InputCtrl = new MobileInputCtrl(t => OnClick?.Invoke(t),
				t => OnBeginDrag?.Invoke(t),
				t => OnEndDrag?.Invoke(t),
				(t1, t2) => OnDrag?.Invoke(t1, t2),
				t => OnZoom?.Invoke(t));
#endif
		}

		internal void OnUpdate()
		{
			m_InputCtrl.OnUpdate();
		}
		public void Dispose()
		{

		}
	}
}