using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public partial class Sys_UIFormDBModel
{
	//private Dictionary<string, Sys_UIFormEntity> m_NameByEntityDic;

	//private void InitNameByEntityDic()
	//{
	//	m_NameByEntityDic = new Dictionary<string, Sys_UIFormEntity>();
	//	for (int i = 0; i < m_List.Count; i++)
	//	{
	//		Sys_UIFormEntity entity = m_List[i];

	//		//�ж϶�����
	//		string path = string.Empty;
	//		switch (GameEntry.Localization.CurrLanguage)
	//		{
	//			case YouYouLanguage.Chinese:
	//				path = entity.AssetPath_Chinese;
	//				break;
	//			case YouYouLanguage.English:
	//				if (entity.AssetPath_English != null)
	//				{
	//					path = entity.AssetPath_English;
	//				}
	//				else
	//				{
	//					path = entity.AssetPath_Chinese;
	//				}
	//				break;
	//		}

	//		//�ж�Prefab���ƺͲ���������������
	//		string[] strs = path.Split('/');
	//		if (strs.Length >= 1)
	//		{
	//			string str = strs[strs.Length - 1];
	//			if (m_NameByEntityDic.ContainsKey(str))
	//			{
	//				Debug.Log("UI_Form�����!, �����ظ����Ƶ��� == " + str);
	//			}
	//			else
	//			{
	//				m_NameByEntityDic.Add(str, entity);
	//			}
	//		}
	//	}
	//}

	///// <summary>
	///// ����Prefab�����Ʒ���Id
	///// </summary>
	//public Sys_UIFormEntity GetEntityByName(string name)
	//{
	//	if (m_NameByEntityDic == null) InitNameByEntityDic();

	//	if (m_NameByEntityDic.ContainsKey(name))
	//	{
	//		return m_NameByEntityDic[name];
	//	}

	//	Debug.Log("Prefab���ƴ���,���Ҳ���!!!");
	//	return null;
	//}
}
