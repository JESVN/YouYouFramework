using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace YouYou
{
	/// <summary>
	/// ��¼����-�����¼��ҵ���߼�
	/// </summary>
	public class ProcedureLogin : ProcedureBase
	{
		internal override void OnEnter()
		{
			base.OnEnter();
			GameEntry.UI.OpenDialogForm("����ڲ�����ȫ���������, ���Խ��뵽ҵ��������~(��װ�Լ��ǵ�¼����)", "��¼");

		}
		internal override void OnUpdate()
		{
			base.OnUpdate();
		}
		internal override void OnLeave()
		{
			base.OnLeave();
		}
		internal override void OnDestroy()
		{
			base.OnDestroy();
		}
	}
}