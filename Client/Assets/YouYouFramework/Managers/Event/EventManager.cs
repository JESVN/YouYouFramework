using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace YouYou
{
	/// <summary>
	/// �¼�������
	/// </summary>
	public class EventManager : ManagerBase, IDisposable
	{
		/// <summary>
		/// Socket�¼�
		/// </summary>
		public SocketEvent SocketEvent { get; private set; }
		public WebSocketEvent WebSocketEvent { get; private set; }
		/// <summary>
		/// ͨ���¼�
		/// </summary>
		public CommonEvent CommonEvent { get; private set; }

		public EventManager()
		{
			SocketEvent = new SocketEvent();
			WebSocketEvent = new WebSocketEvent();
			CommonEvent = new CommonEvent();
		}

		public void Dispose()
		{
			SocketEvent.Dispose();
			CommonEvent.Dispose();
			WebSocketEvent.Dispose();
		}

		public override void Init()
		{

		}
	}
}
