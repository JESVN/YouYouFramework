using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YouYou
{
	/// <summary>
	/// ������
	/// </summary>
	public class ClassObjectPool : IDisposable
	{
		/// <summary>
		/// ������ڳ��еĳ�פ����
		/// </summary>
		public Dictionary<int, byte> ClassObjectCount
		{
			get;
			private set;
		}

		/// <summary>
		/// �������ֵ�
		/// </summary>
		private Dictionary<int, Queue<object>> m_ClassObjectPoolDic;

#if UNITY_EDITOR
		/// <summary>
		/// �ڼ��������ʾ����Ϣ
		/// </summary>
		public Dictionary<Type, int> InspectorDic = new Dictionary<Type, int>();
#endif

		public ClassObjectPool()
		{
			ClassObjectCount = new Dictionary<int, byte>();
			m_ClassObjectPoolDic = new Dictionary<int, Queue<object>>();
		}

		#region SetResideCount �����ೣפ����
		/// <summary>
		/// �����ೣפ����
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="count"></param>
		public void SetResideCount<T>(byte count) where T : class
		{
			int key = typeof(T).GetHashCode();
			ClassObjectCount[key] = count;
		}
		#endregion

		#region Dequeue ȡ��һ������
		/// <summary>
		/// ȡ��һ������
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Dequeue<T>() where T : class, new()
		{
			lock (m_ClassObjectPoolDic)
			{
				//���ҵ������Ĺ�ϣ
				int key = typeof(T).GetHashCode();

				Queue<object> queue = null;
				m_ClassObjectPoolDic.TryGetValue(key, out queue);

				if (queue == null)
				{
					queue = new Queue<object>();
					m_ClassObjectPoolDic[key] = queue;
				}

				//��ʼ��ȡ����
				if (queue.Count > 0)
				{
					//˵�������� �����õ�
					//Debug.Log("���� " + key + "���� �ӳ��л�ȡ");
					object obj = queue.Dequeue();
#if UNITY_EDITOR
					Type t = obj.GetType();
					if (InspectorDic.ContainsKey(t))
					{
						InspectorDic[t]--;
					}
					else
					{
						InspectorDic[t] = 0;
					}
#endif

					return (T)obj;
				}
				else
				{
					//���������û�� ��ʵ����һ��
					//Debug.Log("���� " + key + "������ ����ʵ����");
					return new T();
				}
			}
		}
		#endregion

		#region Enqueue ����س�
		/// <summary>
		/// ����س�
		/// </summary>
		/// <param name="obj"></param>
		public void Enqueue(object obj)
		{
			lock (m_ClassObjectPoolDic)
			{
				int key = obj.GetType().GetHashCode();
				//Debug.Log("���� " + key + "�س���");

				Queue<object> queue = null;
				m_ClassObjectPoolDic.TryGetValue(key, out queue);

#if UNITY_EDITOR
				Type t = obj.GetType();
				if (InspectorDic.ContainsKey(t))
				{
					InspectorDic[t]++;
				}
				else
				{
					InspectorDic[t] = 1;
				}
#endif

				if (queue != null)
				{
					queue.Enqueue(obj);
				}
			}
		}
		#endregion

		/// <summary>
		/// �ͷ�������
		/// </summary>
		public void Release()
		{
			lock (m_ClassObjectPoolDic)
			{
				int queueCount = 0; //���е�����

				//1.���������
				var enumerator = m_ClassObjectPoolDic.GetEnumerator();
				while (enumerator.MoveNext())
				{
					int key = enumerator.Current.Key;

					//�õ�����
					Queue<object> queue = m_ClassObjectPoolDic[key];

#if UNITY_EDITOR
					Type t = null;
#endif
					queueCount = queue.Count;

					//�û��ͷŵ�ʱ�� �ж�
					byte resideCount = 0;
					ClassObjectCount.TryGetValue(key, out resideCount);
					while (queueCount > resideCount)
					{
						//�������п��ͷŵĶ���
						queueCount--;
						object obj = queue.Dequeue(); //�Ӷ�����ȡ��һ�� �������û���κ����ã��ͱ����Ұָ�� �ȴ�GC����

#if UNITY_EDITOR
						t = obj.GetType();
						InspectorDic[t]--;
#endif
					}

					if (queueCount == 0)
					{
#if UNITY_EDITOR
						if (t != null)
						{
							InspectorDic.Remove(t);
						}
#endif
					}
				}

				//GC ������Ŀ�У���һ��GC����
				GC.Collect();
			}
		}

		public void Dispose()
		{
			m_ClassObjectPoolDic.Clear();
		}
	}
}