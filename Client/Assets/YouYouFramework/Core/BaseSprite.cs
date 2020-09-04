using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseSprite : MonoBehaviour
{
	private void Start()
	{
		OnInit();
		OnOpen();
	}
	private void Update()
	{
		OnUpdate();
	}
	private void OnDestroy()
	{
		OnBeforDestroy();
	}

	/// <summary>
	/// ��¡ʱ����
	/// </summary>
	protected virtual void OnInit() { }
	/// <summary>
	/// �Ӷ����ȡ��ʱ����
	/// </summary>
	public virtual void OnOpen() { }
	/// <summary>
	/// �˻ص������ʱ����
	/// </summary>
	public virtual void OnClose() { }
	/// <summary>
	/// ����ʱ����
	/// </summary>
	protected virtual void OnBeforDestroy() { }
	protected virtual void OnUpdate() { }
}
