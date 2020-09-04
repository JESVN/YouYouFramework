using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
	public class UIManager : ManagerBase
	{
		/// <summary>
		/// �Ѿ��򿪵�UI��������
		/// </summary>
		private LinkedList<UIFormBase> m_OpenUIFormList;
		/// <summary>
		/// ����UIջ
		/// </summary>
		private Stack<UIFormBase> m_ReverseChangeUIStack;
		/// <summary>
		/// ���ڼ����е�UI����
		/// </summary>
		private LinkedList<int> m_LoadingUIFormList;

		private UILayer m_UILayer;

		private Dictionary<byte, UIGroup> m_UIGroupDic;

		private UIPool m_UIPool;

		/// <summary>
		/// UI��������������
		/// </summary>
		public int UIPoolMaxCount { get; private set; }
		/// <summary>
		/// UI�سغ����ʱ��_��
		/// </summary>
		public float UIExpire { get; private set; }
		/// <summary>
		/// UI�ͷż��_��
		/// </summary>
		public float ClearInterval { get; private set; }

		/// <summary>
		/// �´�����ʱ��
		/// </summary>
		private float m_NextRunTime = 0f;

		/// <summary>
		/// ��׼�ֱ��ʱ�ֵ
		/// </summary>
		private float m_StandardScreen = 0;
		/// <summary>
		/// ��ǰ�ֱ��ʱ�ֵ
		/// </summary>
		private float m_CurrScreen = 0;

		public UIManager()
		{
			m_OpenUIFormList = new LinkedList<UIFormBase>();
			m_ReverseChangeUIStack = new Stack<UIFormBase>();
			m_LoadingUIFormList = new LinkedList<int>();

			m_UILayer = new UILayer();
			m_UIGroupDic = new Dictionary<byte, UIGroup>();
			m_UIPool = new UIPool();

		}
		internal void Dispose()
		{
		}
		public override void Init()
		{
			UIPoolMaxCount = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.UI_PoolMaxCount, GameEntry.CurrDeviceGrade);
			UIExpire = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.UI_Expire, GameEntry.CurrDeviceGrade);
			ClearInterval = GameEntry.ParamsSettings.GetGradeParamData(YFConstDefine.UI_ClearInterval, GameEntry.CurrDeviceGrade);

			m_StandardScreen = GameEntry.Instance.StandardWidth / (float)GameEntry.Instance.StandardHeight;
			m_CurrScreen = Screen.width / (float)Screen.height;

			LoadingFormCanvasScaler();

			for (int i = 0; i < GameEntry.Instance.UIGroups.Length; i++)
			{
				m_UIGroupDic[GameEntry.Instance.UIGroups[i].Id] = GameEntry.Instance.UIGroups[i];
			}
			m_UILayer.Init(GameEntry.Instance.UIGroups);
		}
		public void OnUpdate()
		{
			if (Time.time > m_NextRunTime + ClearInterval)
			{
				m_NextRunTime = Time.time;

				//�ͷ�UI�����
				m_UIPool.CheckClear();
			}
		}

		#region UI����
		/// <summary>
		/// LoadingForm��������
		/// </summary>
		public void LoadingFormCanvasScaler()
		{
			GameEntry.Instance.UIRootCanvasScaler.matchWidthOrHeight = (m_CurrScreen >= m_StandardScreen) ? 1 : 0;
		}
		/// <summary>
		/// FullForm��������
		/// </summary>
		public void FullFormCanvasScaler()
		{
			GameEntry.Instance.UIRootCanvasScaler.matchWidthOrHeight = 1;
		}
		/// <summary>
		/// NormalForm��������
		/// </summary>
		public void NormalFormCanvasScaler()
		{
			if (m_CurrScreen > m_StandardScreen)
			{
				//����Ϊ0
				GameEntry.Instance.UIRootCanvasScaler.matchWidthOrHeight = 0;
			}
			else
			{
				GameEntry.Instance.UIRootCanvasScaler.matchWidthOrHeight = m_StandardScreen - m_CurrScreen;
			}
		}

		#endregion

		#region GetUIGroup ����UI�����Ż�ȡUI����
		/// <summary>
		/// ����UI�����Ż�ȡUI����
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public UIGroup GetUIGroup(byte id)
		{
			UIGroup group = null;
			m_UIGroupDic.TryGetValue(id, out group);
			return group;
		}
		#endregion

		#region OpenDialogForm ����ʾ����
		/// <summary>
		/// ����ʾ����
		/// </summary>
		public void OpenDialogForm(int sysCode, string title = "��ʾ", DialogFormType dialogFormType = DialogFormType.Noraml, BaseAction okAction = null, BaseAction cancelAction = null)
		{
			OpenDialogForm(GameEntry.Data.SysDataManager.GetSysCodeContent(sysCode), title, dialogFormType, okAction, cancelAction);
		}
		/// <summary>
		/// ����ʾ����
		/// </summary>
		public void OpenDialogForm(string str, string title = "��ʾ", DialogFormType dialogFormType = DialogFormType.Noraml, BaseAction okAction = null, BaseAction cancelAction = null)
		{
			OpenUIForm(UIFormId.UI_Dialog, onOpen: (UIFormBase uiFormBase) =>
			  {
				  UIDialogForm messageForm = uiFormBase as UIDialogForm;
				  messageForm.SetUI(str, title, dialogFormType, okAction, cancelAction);
			  });
		}
		#endregion

		#region OpenUIForm ��UI����
		public void OpenUIForm(string uiFormName, object userData = null, BaseAction<UIFormBase> onOpen = null)
		{
			OpenUIForm(GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName), userData, onOpen);
		}
		public void OpenUIForm(int uiFormId, object userData = null, BaseAction<UIFormBase> onOpen = null)
		{
			//1,����
			Sys_UIFormEntity sys_UIForm = GameEntry.DataTable.Sys_UIFormDBModel.GetDic(uiFormId);
			if (sys_UIForm == null) Debug.LogError(uiFormId + "��Ӧ��UI���ڲ�����");

			if (sys_UIForm.CanMulit == 0 && IsExists(uiFormId))
			{
				Debug.LogError("���ظ���ͬһ��UI����");
				return;
			}

			UIFormBase formBase = GameEntry.UI.Dequeue(uiFormId);
			if (formBase == null)
			{
				//�첽����UI��Ҫʱ�� �˴���Ҫ������˼����е�UI
				if (IsLoading(uiFormId)) return;
				m_LoadingUIFormList.AddLast(uiFormId);

				string assetPath = string.Empty;
				switch (GameEntry.CurrLanguage)
				{
					case YouYouLanguage.Chinese:
						assetPath = sys_UIForm.AssetPath_Chinese;
						break;
					case YouYouLanguage.English:
						assetPath = sys_UIForm.AssetPath_English;
						break;
				}
				//����UI��Դ����¡
				GameEntry.Resource.ResourceLoaderManager.LoadMainAsset(AssetCategory.UIPrefab, string.Format("Assets/Download/UI/UIPrefab/{0}.prefab", assetPath), (ResourceEntity resourceEntity) =>
				{
					GameObject uiObj = Object.Instantiate((Object)resourceEntity.Target, GameEntry.UI.GetUIGroup(sys_UIForm.UIGroupId).Group) as GameObject;

					//�ѿ�¡��������Դ ����ʵ����Դ��
					GameEntry.Pool.RegisterInstanceResource(uiObj.GetInstanceID(), resourceEntity);


					RectTransform rectTransform = uiObj.GetComponent<RectTransform>();
					rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, 0);
					rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 0);
					rectTransform.anchorMin = Vector2.zero;
					rectTransform.anchorMax = Vector2.one;

					//��ʼ��UI
					formBase = uiObj.GetComponent<UIFormBase>();
					formBase.CurrCanvas.overrideSorting = true;
					formBase.Init(uiFormId, sys_UIForm, (byte)sys_UIForm.UIGroupId, sys_UIForm.DisableUILayer == 1, sys_UIForm.IsLock == 1, userData, () =>
					{
						OpenUI(sys_UIForm, formBase, onOpen);
					});
					m_OpenUIFormList.AddLast(formBase);
					m_LoadingUIFormList.Remove(uiFormId);

				});
			}
			else
			{
				formBase.Open(userData);
				m_OpenUIFormList.AddLast(formBase);
				GameEntry.UI.ShowUI(formBase);
				OpenUI(sys_UIForm, formBase, onOpen);
			}

			//��������ͷ�
			m_UIPool.CheckByOpenUI();
		}
		private void OpenUI(Sys_UIFormEntity sys_UIFormEntity, UIFormBase formBase, BaseAction<UIFormBase> onOpen)
		{
			//�жϷ���UI
			UIFormShowMode uIFormShowMode = (UIFormShowMode)sys_UIFormEntity.ShowMode;
			if (uIFormShowMode == UIFormShowMode.ReverseChange)
			{
				//���֮ǰջ������UI
				if (m_ReverseChangeUIStack.Count > 0)
				{
					//��ջ���� �õ�UI
					UIFormBase topUIForm = m_ReverseChangeUIStack.Peek();

					//���� ����
					GameEntry.UI.HideUI(topUIForm);
				}

				//���Լ�����ջ
				//Debug.LogError("��ջ==" + formBase.gameObject.GetInstanceID());
				m_ReverseChangeUIStack.Push(formBase);
			}

			onOpen?.Invoke(formBase);
		}
		#endregion

		#region CloseUIForm �ر�UI����
		public void CloseUIForm(int uiFormId)
		{
			//m_UIManager.CloseUIForm(uiFormId);
			for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
			{
				if (curr.Value.CurrUIFormId == uiFormId)
				{
					CloseUIForm(curr.Value);
					break;
				}
			}
		}
		public void CloseUIForm(UIFormBase formBase)
		{
			m_OpenUIFormList.Remove(formBase);
			formBase.ToClose();

			//�жϷ���UI
			UIFormShowMode uIFormShowMode = (UIFormShowMode)formBase.SysUIForm.ShowMode;
			if (uIFormShowMode == UIFormShowMode.ReverseChange)
			{
				m_ReverseChangeUIStack.Pop();

				if (m_ReverseChangeUIStack.Count > 0)
				{
					UIFormBase topForms = m_ReverseChangeUIStack.Peek();
					GameEntry.UI.ShowUI(topForms);
				}
			}
		}
		/// <summary>
		/// �ر�UI����
		/// </summary>
		/// <param name="uiFormName"></param>
		public void CloseUIForm(string uiFormName)
		{
			CloseUIForm(GameEntry.DataTable.Sys_UIFormDBModel.GetIdByName(uiFormName));
		}
		/// <summary>
		/// �ر�����"Default"���UI����
		/// </summary>
		public void CloseAllDefaultUIForm()
		{
			UIFormBase[] uIFormBases = m_UIGroupDic[2].Group.GetComponentsInChildren<UIFormBase>();
			for (int i = 0; i < uIFormBases.Length; i++)
			{
				CloseUIForm(uIFormBases[i]);
			}
		}
		/// <summary>
		/// ����InstanceID�ر�UI
		/// </summary>
		/// <param name="instanceID"></param>
		internal void CloseUIFormByInstanceID(int instanceID)
		{
			for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
			{
				if (curr.Value.gameObject.GetInstanceID() == instanceID)
				{
					CloseUIForm(curr.Value);
					break;
				}
			}
		}
		#endregion


		/// <summary>
		/// ��ʾ/����һ��UI
		/// </summary>
		/// <param name="uIFormBase"></param>
		public void ShowUI(UIFormBase uiFormBase)
		{
			if (uiFormBase.SysUIForm.FreezeMode == 0)
			{
				uiFormBase.IsActive = true;
				uiFormBase.CurrCanvas.enabled = true;
				uiFormBase.gameObject.layer = 5;
			}
			else
			{
				uiFormBase.gameObject.SetActive(true);
			}
			//Debug.LogError("��ʾ " + uIFormBase.gameObject.GetInstanceID());
		}
		/// <summary>
		/// ����/����һ��UI
		/// </summary>
		/// <param name="uIFormBase"></param>
		public void HideUI(UIFormBase uiFormBase)
		{
			if (uiFormBase.SysUIForm.FreezeMode == 0)
			{
				uiFormBase.IsActive = false;
				uiFormBase.CurrCanvas.enabled = false;
				uiFormBase.gameObject.layer = 0;
			}
			else
			{
				uiFormBase.gameObject.SetActive(false);
			}
			//Debug.LogError("���� " + uIFormBase.gameObject.GetInstanceID());
		}

		/// <summary>
		/// ���ò㼶
		/// </summary>
		/// <param name="formBase">����</param>
		/// <param name="isAdd">true:����  false:����</param>
		internal void SetSortingOrder(UIFormBase formBase, bool isAdd)
		{
			m_UILayer.SetSortingOrder(formBase, isAdd);
		}

		/// <summary>
		/// �ӳ��л�ȡUI����
		/// </summary>
		/// <param name="uiFormId"></param>
		/// <returns></returns>
		internal UIFormBase Dequeue(int uiFormId)
		{
			return m_UIPool.Dequeue(uiFormId);
		}

		/// <summary>
		/// UI���ڻس�
		/// </summary>
		/// <param name="form"></param>
		internal void EnQueue(UIFormBase form)
		{
			m_UIPool.EnQueue(form);
		}

		/// <summary>
		/// ���UI�Ƿ��Ѿ���
		/// </summary>
		/// <param name="uiFormId"></param>
		/// <returns></returns>
		public bool IsExists(int uiFormId)
		{
			for (LinkedListNode<UIFormBase> curr = m_OpenUIFormList.First; curr != null; curr = curr.Next)
			{
				if (curr.Value.CurrUIFormId == uiFormId)
				{
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// ���UI���ڼ�����
		/// </summary>
		private bool IsLoading(int uiFormId)
		{
			for (LinkedListNode<int> curr = m_LoadingUIFormList.First; curr != null; curr = curr.Next)
			{
				if (curr.Value == uiFormId)
				{
					GameEntry.LogError("UI���ڼ�����, �򿪵�Ƶ�ʹ���");
					return true;
				}
			}
			return false;
		}


	}
}