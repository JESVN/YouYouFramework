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
		public override void OnEnter()
		{
			base.OnEnter();
			GameEntry.UI.OpenDialogForm("����ڲ�����ȫ���������, ���Խ��뵽ҵ��������~(��װ�Լ��ǵ�¼����)", "��¼");

		}
		public override void OnUpdate()
		{
			base.OnUpdate();
		}
		public override void OnLeave()
		{
			base.OnLeave();
		}
		public override void OnDestroy()
		{
			base.OnDestroy();
		}
	}
}