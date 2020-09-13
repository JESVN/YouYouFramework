using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

/// <summary>
/// �û�����
/// </summary>
public class UserDataManager : IDisposable
{
	/// <summary>
	/// ������û�����
	/// </summary>
	public ShareUserData ShareUserData;

	public UserDataManager()
	{
		ShareUserData = new ShareUserData();
	}

	/// <summary>
	/// �������
	/// </summary>
	public void Clear()
	{
		ShareUserData.Dispose();
	}

	public void Dispose()
	{
	}
}
