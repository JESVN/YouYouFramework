//===================================================
//��    �ߣ�����  http://www.u3dol.com
//����ʱ�䣺
//��    ע��
//===================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace YouYou
{
	/// <summary>
	/// Http�������ݵĻص�ί��
	/// </summary>
	/// <param name="args"></param>
	public delegate void HttpSendDataCallBack(HttpCallBackArgs args);

	/// <summary>
	/// Http������
	/// </summary>
	public class HttpRoutine
	{
		#region ����

		/// <summary>
		/// Http����ص�
		/// </summary>
		private HttpSendDataCallBack m_CallBack;

		/// <summary>
		/// Http����ص�����
		/// </summary>
		private HttpCallBackArgs m_CallBackArgs;

		/// <summary>
		/// �Ƿ�æ
		/// </summary>
		public bool IsBusy { get; private set; }

		/// <summary>
		/// ��ǰ���Դ���
		/// </summary>
		private int m_CurrRetry = 0;

		private string m_Url;
		private string m_Json;

		/// <summary>
		/// ���͵�����
		/// </summary>
		private Dictionary<string, object> m_Dic;
		#endregion

		public HttpRoutine()
		{
			m_CallBackArgs = new HttpCallBackArgs();
			m_Dic = new Dictionary<string, object>();
		}

		#region SendData ����web����
		/// <summary>
		/// ����web����
		/// </summary>
		/// <param name="url"></param>
		/// <param name="callBack"></param>
		/// <param name="isPost"></param>
		/// <param name="isGetData">�Ƿ��ȡ�ֽ�����</param>
		/// <param name="dic"></param>
		internal void Get(string url, HttpSendDataCallBack callBack = null)
		{
			if (IsBusy) return;
			IsBusy = true;

			m_Url = url;
			m_CallBack = callBack;

			GetUrl(m_Url);
		}

		internal void Post(string url, string json = null, HttpSendDataCallBack callBack = null)
		{
			if (IsBusy) return;
			IsBusy = true;

			m_Url = url;
			m_CallBack = callBack;
			m_Json = json;

			PostUrl(m_Url);
		}
		#endregion

		#region GetUrl Get����
		/// <summary>
		/// Get����
		/// </summary>
		/// <param name="url"></param>
		private void GetUrl(string url)
		{
			GameEntry.Log(LogCategory.Proto, "Get����:{0}, {1}������", m_Url, m_CurrRetry);
			UnityWebRequest data = UnityWebRequest.Get(url);
			GameEntry.Instance.StartCoroutine(Request(data));
		}
		#endregion

		#region PostUrl Post����
		/// <summary>
		/// Post����
		/// </summary>
		/// <param name="url"></param>
		/// <param name="json"></param>
		private void PostUrl(string url)
		{
			UnityWebRequest unityWeb = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
			unityWeb.downloadHandler = new DownloadHandlerBuffer();
			if (!string.IsNullOrWhiteSpace(m_Json))
			{
				if (GameEntry.ParamsSettings.PostIsEncrypt)
				{
					m_Dic["value"] = m_Json;
					//web����
					m_Dic["deviceIdentifier"] = DeviceUtil.DeviceIdentifier;
					m_Dic["deviceModel"] = DeviceUtil.DeviceModel;
					long t = GameEntry.Data.SysDataManager.CurrServerTime;
					m_Dic["sign"] = EncryptUtil.Md5(string.Format("{0}:{1}", t, DeviceUtil.DeviceIdentifier));
					m_Dic["t"] = t;

					m_Json = m_Dic.ToJson();
				}
				unityWeb.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(m_Json));

				if (!string.IsNullOrWhiteSpace(GameEntry.ParamsSettings.PostContentType))
					unityWeb.SetRequestHeader("Content-Type", GameEntry.ParamsSettings.PostContentType);
			}
			GameEntry.Log(LogCategory.Proto, "Post����:{0}, {1}������==>>{2}", m_Url, m_CurrRetry, m_Json);
			GameEntry.Instance.StartCoroutine(Request(unityWeb));
		}
		#endregion

		#region Request ���������
		/// <summary>
		/// ���������
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private IEnumerator Request(UnityWebRequest data)
		{
			yield return data.SendWebRequest();
			if (data.isNetworkError || data.isHttpError)
			{
				//������ ��������
				if (m_CurrRetry > 0) yield return new WaitForSeconds(GameEntry.Http.RetryInterval);
				m_CurrRetry++;
				if (m_CurrRetry <= GameEntry.Http.Retry)
				{
					switch (data.method)
					{
						case UnityWebRequest.kHttpVerbGET:
							GetUrl(m_Url);
							break;
						case UnityWebRequest.kHttpVerbPOST:
							PostUrl(m_Url);
							break;
					}
					yield break;
				}

				IsBusy = false;
				m_CallBackArgs.HasError = true;
				m_CallBackArgs.Value = data.error;
				m_CallBack?.Invoke(m_CallBackArgs);
			}
			else
			{
				IsBusy = false;
				m_CallBackArgs.HasError = false;
				m_CallBackArgs.Value = data.downloadHandler.text;
				m_CallBackArgs.Data = data.downloadHandler.data;
				m_CallBack?.Invoke(m_CallBackArgs);
			}

			if (!string.IsNullOrWhiteSpace(m_CallBackArgs.Value)) GameEntry.Log(LogCategory.Proto, "WebAPI�ص�:{0}, ==>>{1}", m_Url, m_CallBackArgs.ToJson());

			m_CurrRetry = 0;
			m_Url = null;
			if (m_Dic != null)
			{
				m_Dic.Clear();
				GameEntry.Pool.EnqueueClassObject(m_Dic);
			}
			m_CallBackArgs.Data = null;
			data.Dispose();
			data = null;

			//Debug.Log("��http�������س�");
			GameEntry.Pool.EnqueueClassObject(this);
		}
		#endregion
	}
}