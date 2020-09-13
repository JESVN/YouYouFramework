using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
	/// <summary>
	/// Ԥ��������
	/// </summary>
	public class ProcedurePreload : ProcedureBase
	{
		/// <summary>
		/// Ŀ�����
		/// </summary>
		private float m_TargetProgress;
		/// <summary>
		/// ��ǰ����
		/// </summary>
		private float m_CurrProgress;

		/// <summary>
		/// Ԥ���ز���
		/// </summary>
		private BaseParams m_PreloadParams;

		internal override void OnEnter()
		{
			base.OnEnter();
			//�������ݱ����
			GameEntry.Event.CommonEvent.AddEventListener(SysEventId.LoadOneDataTableComplete, OnLoadOneDataTableComplete);
			GameEntry.Event.CommonEvent.AddEventListener(SysEventId.LoadDataTableComplete, OnLoadDataTableComplete);
			GameEntry.Event.CommonEvent.AddEventListener(SysEventId.LoadLuaDataTableComplete, OnLoadLuaDataTableComplete);

			m_PreloadParams = GameEntry.Pool.DequeueClassObject<BaseParams>();
			m_PreloadParams.Reset();
			GameEntry.Event.CommonEvent.Dispatch(SysEventId.PreloadBegin);

			m_CurrProgress = 0;
			m_TargetProgress = 85;

#if ASSETBUNDLE
			GameEntry.Resource.InitAssetInfo(() =>
			{
				GameEntry.DataTable.LoadDataAllTable();
			});
#else
			GameEntry.DataTable.LoadDataAllTable();
#endif
		}
		internal override void OnUpdate()
		{
			base.OnUpdate();

			if (m_LoadDataTableStatus == 1)
			{
				m_LoadDataTableStatus = 2;
				LoadAudio();
			}

			//���ؽ���(ģ��)
			if (m_CurrProgress < m_TargetProgress)
			{
				m_CurrProgress = Mathf.Min(m_CurrProgress + Time.deltaTime * 100, m_TargetProgress);//����ʵ����������ٶ�
				m_PreloadParams.FloatParam1 = m_CurrProgress;
				GameEntry.Event.CommonEvent.Dispatch(SysEventId.PreloadUpdate, m_PreloadParams);
			}

			if (m_TargetProgress == 100)
			{
				m_CurrProgress = 100;
				m_PreloadParams.FloatParam1 = m_CurrProgress;

				GameEntry.Event.CommonEvent.Dispatch(SysEventId.PreloadUpdate, m_PreloadParams);

				GameEntry.Event.CommonEvent.Dispatch(SysEventId.PreloadComplete);
				GameEntry.Pool.EnqueueClassObject(m_PreloadParams);

				//���뵽ҵ������
				GameEntry.Procedure.ChangeState(ProcedureState.Login);
			}
		}
		internal override void OnLeave()
		{
			base.OnLeave();
			GameEntry.Event.CommonEvent.RemoveEventListener(SysEventId.LoadOneDataTableComplete, OnLoadOneDataTableComplete);
			GameEntry.Event.CommonEvent.RemoveEventListener(SysEventId.LoadDataTableComplete, OnLoadDataTableComplete);
			GameEntry.Event.CommonEvent.RemoveEventListener(SysEventId.LoadLuaDataTableComplete, OnLoadLuaDataTableComplete);
		}

		private void OnLoadOneDataTableComplete(object userData)
		{
			//Debug.Log("���ݱ����������, TabName = " + userData);

			GameEntry.DataTable.CurrLoadTableCount++;
			if (GameEntry.DataTable.CurrLoadTableCount == GameEntry.DataTable.TotalTableCount)
			{
				GameEntry.Event.CommonEvent.Dispatch(SysEventId.LoadDataTableComplete);
			}
		}
		/// <summary>
		/// ���ر��״̬0=δ���� 1=�������
		/// </summary>
		byte m_LoadDataTableStatus = 0;
		private void OnLoadDataTableComplete(object userData)
		{
			GameEntry.Log(LogCategory.Normal, "��������C#������)");
			m_LoadDataTableStatus = 1;
		}
		private void OnLoadLuaDataTableComplete(object userData)
		{
			GameEntry.Log(LogCategory.Normal, "��������lua������");
			LoadShader();
		}

		/// <summary>
		/// ��������
		/// </summary>
		private void LoadAudio()
		{
			GameEntry.Audio.LoadBanks(() =>
			{
#if RESOURCES
				m_TargetProgress = 100;
#else
				//��ʼ��Xlua
				GameEntry.Lua.Init();
#endif

			});
		}

		/// <summary>
		/// �����Զ���Shader
		/// </summary>
		private void LoadShader()
		{
#if ASSETBUNDLE
			GameEntry.Resource.ResourceLoaderManager.LoadAssetBundle(YFConstDefine.CusShadersAssetBundlePath, onComplete: (AssetBundle bundle) =>
			{
				bundle.LoadAllAssets();
				Shader.WarmupAllShaders();
				GameEntry.Log(LogCategory.Normal, "������Դ���е��Զ���Shader���");
				m_TargetProgress = 100;
			});
#else
			m_TargetProgress = 100;
#endif
		}
	}
}