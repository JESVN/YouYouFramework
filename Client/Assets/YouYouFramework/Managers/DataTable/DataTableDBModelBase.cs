using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
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
        internal void LoadData()
        {
            GameEntry.DataTable.TotalTableCount++;

            //1.�õ��������buffer
            GameEntry.DataTable.GetDataTableBuffer(DataTableName, (byte[] buffer) =>
            {
                using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
                {
                    LoadList(ms);
                }

                GameEntry.Event.CommonEvent.Dispatch(SysEventId.LoadOneDataTableComplete, DataTableName);
            });
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
        internal void Clear()
        {
            m_List.Clear();
            m_Dic.Clear();
        }

    }
}