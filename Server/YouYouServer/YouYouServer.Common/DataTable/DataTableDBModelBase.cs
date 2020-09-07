using System.Collections;
using System.Collections.Generic;
using YouYouServer.Common;
using YouYouServer.Core;

/// <summary>
/// ���ݱ�������
/// </summary>
/// <typeparam name="T">���ݱ�������������</typeparam>
/// <typeparam name="P">���ݱ�ʵ�����������</typeparam>
public abstract class DataTableDBModelBase<T, P>
where T : class, new()
where P : DataTableEntityBase
{
	/// <summary>
	/// Entity����ļ���
	/// </summary>
	protected List<P> m_List;

	/// <summary>
	/// Key:Entity��ID
	/// Value:Entity����
	/// </summary>
	protected Dictionary<int, P> m_Dic;

	public DataTableDBModelBase()
	{
		m_List = new List<P>();
		m_Dic = new Dictionary<int, P>();
	}

	#region ��Ҫ����ʵ�ֵ�����,����
	/// <summary>
	/// ���ݱ�����
	/// </summary>
	public abstract string DataTableName { get; }
	/// <summary>
	/// ���������б�
	/// </summary>
	protected abstract void LoadList(MMO_MemoryStream ms);
	#endregion

	#region LoadData �������ݱ�����
	/// <summary>
	/// �������ݱ�����
	/// </summary>
	public void LoadData()
	{
		//1.�õ��������buffer
		byte[] buffer = YFIOUtil.GetBuffer(string.Format(ServerConfig.DataTablePath + "/{0}.bytes", DataTableName), false);
		using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
		{
			LoadList(ms);
		}
	}
	#endregion

	#region GetList ��ȡ�����Ӧ������ʵ��List
	/// <summary>
	/// ��ȡ�����Ӧ������ʵ��List
	/// </summary>
	/// <returns></returns>
	public List<P> GetList()
	{
		return m_List;
	}
	#endregion

	#region GetDic ����ID��ȡʵ��
	/// <summary>
	/// ����ID��ȡʵ��
	/// </summary>
	public P GetDic(int id)
	{
		P p;
		if (m_Dic.TryGetValue(id, out p))
		{
			return p;
		}
		else
		{
			//Debug.Log("��ID��Ӧ������ʵ�岻����");
			return null;
		}
	}
	#endregion

	/// <summary>
	/// �������
	/// </summary>
	public void Clear()
	{
		m_List.Clear();
		m_Dic.Clear();
	}

}
