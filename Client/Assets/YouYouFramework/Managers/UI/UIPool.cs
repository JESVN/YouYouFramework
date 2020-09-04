using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// UI���ڶ����
	/// </summary>
	public class UIPool
	{
		/// <summary>
		/// ������е�����
		/// </summary>
		private LinkedList<UIFormBase> m_UIFormList;

		public UIPool()
		{
			m_UIFormList = new LinkedList<UIFormBase>();
		}

		/// <summary>
		/// �ӳ��л�ȡUI����
		/// </summary>
		/// <param name="uiFormId"></param>
		/// <returns></returns>
		internal UIFormBase Dequeue(int uiFormId)
		{
			for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null; curr = curr.Next)
			{
				if (curr.Value.CurrUIFormId == uiFormId)
				{
					m_UIFormList.Remove(curr.Value);
					return curr.Value;
				}
			}
			return null;
		}

		/// <summary>
		/// UI���ڻس�
		/// </summary>
		/// <param name="form"></param>
		internal void EnQueue(UIFormBase form)
		{
			GameEntry.UI.HideUI(form);
			m_UIFormList.AddLast(form);
		}

		/// <summary>
		/// ����Ƿ�����ͷ�
		/// </summary>
		internal void CheckClear()
		{
			for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null;)
			{
				if (!curr.Value.IsLock && Time.time > curr.Value.CloseTime + GameEntry.UI.UIExpire)
				{
					//����UI
					Object.Destroy(curr.Value.gameObject);
					GameEntry.Pool.ReleaseInstanceResource(curr.Value.GetInstanceID());

					LinkedListNode<UIFormBase> next = curr.Next;
					m_UIFormList.Remove(curr.Value);
					curr = next;
				}
				else
				{
					curr = curr.Next;
				}
			}
		}

		/// <summary>
		/// ����UI��������,����Ƿ�����ͷ�
		/// </summary>
		internal void CheckByOpenUI()
		{
			if (m_UIFormList.Count <= GameEntry.UI.UIPoolMaxCount) return;

			for (LinkedListNode<UIFormBase> curr = m_UIFormList.First; curr != null;)
			{
				if (m_UIFormList.Count == GameEntry.UI.UIPoolMaxCount + 1) break;

				if (!curr.Value.IsLock)
				{
					LinkedListNode<UIFormBase> next = curr.Next;
					m_UIFormList.Remove(curr.Value);

					//����UI
					Object.Destroy(curr.Value.gameObject);
					GameEntry.Pool.ReleaseInstanceResource(curr.Value.gameObject.GetInstanceID());

					curr = next;
				}
				else
				{
					curr = curr.Next;
				}
			}
		}
	}
}