using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebSocketEvent : IDisposable
{
	/// <summary>
	/// ����ԭ��       ���о������ⶼ���Ը�ԭ��Ϊί������
	/// </summary>
	/// <param name="P">�����������</param>
	public delegate void OnActionHandler(string jsonData);
	/// <summary>
	/// Key: �����б��Key   ͬ���б�������ⶼ��ͬһ��Key
	/// Value: �����б�
	/// </summary>
	private Dictionary<string, LinkedList<OnActionHandler>> dic = new Dictionary<string, LinkedList<OnActionHandler>>();


	#region AddEventListener �۲��߼����¼�
	/// <summary> 
	/// �۲��߼����¼�
	/// </summary>
	/// <param name="Key">�����б��Key</param>
	/// <param name="handler">����</param>
	public void AddEventListener(string key, OnActionHandler handler)
	{
		LinkedList<OnActionHandler> lstHandler = null;
		dic.TryGetValue(key, out lstHandler);
		if (lstHandler == null)
		{
			lstHandler = new LinkedList<OnActionHandler>();
			dic[key] = lstHandler;
		}
		lstHandler.AddLast(handler);
	}
	#endregion

	#region RemoveEventListener �۲����Ƴ������¼�
	/// <summary>
	/// �۲����Ƴ������¼�
	/// </summary>
	/// <param name="key">�����б��Key</param>
	/// <param name="handler">����</param>
	public void RemoveEventListener(string key, OnActionHandler handler)
	{
		LinkedList<OnActionHandler> lstHandler = null;
		dic.TryGetValue(key, out lstHandler);
		if (lstHandler != null)
		{
			lstHandler.Remove(handler);
			if (lstHandler.Count == 0)
			{
				dic.Remove(key);
			}
		}
	}
	#endregion

	#region Dispatch �������ɷ��¼�
	/// <summary>
	/// �������ɷ��¼�
	/// </summary>
	/// <param name="btnKey">�����б��Key</param>
	/// <param name="jsonData">�������</param>
	public void Dispatch(string key, string jsonData)
	{
		LinkedList<OnActionHandler> lstHandler = null;
		dic.TryGetValue(key, out lstHandler);

		if (lstHandler != null && lstHandler.Count > 0)
		{
			for (LinkedListNode<OnActionHandler> curr = lstHandler.First; curr != null; curr = curr.Next)
			{
				curr.Value?.Invoke(jsonData);
			}
		}
	}
	public void Dispatch(string key)
	{
		Dispatch(key, null);
	}
	#endregion

	public void Dispose()
	{
		dic.Clear();
	}
}
