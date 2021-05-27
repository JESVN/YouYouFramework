using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// ������ö��
	/// </summary>
	public enum YouYouLanguage
	{
		/// <summary>
		/// ����
		/// </summary>
		Chinese = 0,
		/// <summary>
		/// Ӣ��
		/// </summary>
		English = 1
	}


	public class LocalizationManager : ManagerBase, IDisposable
	{
		internal override void Init()
		{
#if !UNITY_EDITOR
            switch (Application.systemLanguage)
            {
                default:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional:
                case SystemLanguage.Chinese:
                    GameEntry.CurrLanguage = YouYouLanguage.Chinese;
                    break;
                case SystemLanguage.English:
                    GameEntry.CurrLanguage = YouYouLanguage.English;
                    break;
            }
#endif
		}

		/// <summary>
		/// ��ȡ���ػ��ı�����
		/// </summary>
		/// <param name="key"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public string GetString(string key, params object[] args)
		{
			string value = null;
			if (GameEntry.DataTable.LocalizationDBModel.LocalizationDic.TryGetValue(key, out value))
			{
				return string.Format(value, args);
			}
			return value;
		}

		public void Dispose()
		{

		}


	}
}