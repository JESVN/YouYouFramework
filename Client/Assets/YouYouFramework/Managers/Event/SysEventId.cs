using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YouYou
{
    /// <summary>
    /// ϵͳ�¼����(ϵͳ�¼���� ����4λ 1001(10��ʾģ�� 01��ʾ���))
    /// </summary>
    public class SysEventId
    {
		/// <summary>
		/// ���ݱ�ȫ���������
		/// </summary>
		public const ushort LoadDataTableComplete = 1001;

		/// <summary>
		/// ���ݱ����������
		/// </summary>
		public const ushort LoadOneDataTableComplete = 1002;

		/// <summary>
		/// ����Lua������
		/// </summary>
		public const ushort LoadLuaDataTableComplete = 1003;

		/// <summary>
		/// ���ؽ���������
		/// </summary>
		public const ushort LoadingProgressChange = 1101;
		/// <summary>
		/// �ر�"תȦȦ"
		/// </summary>
		public const ushort CloseUICircle = 1102;

		/// <summary>
		/// ������_��ʼ����
		/// </summary>
		public const ushort CheckVersionBeginDownload = 1201;
		/// <summary>
		/// ������_������
		/// </summary>
		public const ushort CheckVersionDownloadUpdate = 1202;
		/// <summary>
		/// ������_�������
		/// </summary>
		public const ushort CheckVersionDownloadComplete = 1203;

		/// <summary>
		/// Ԥ����_��ʼ����
		/// </summary>
		public const ushort PreloadBegin = 1204;
		/// <summary>
		/// Ԥ����_��ʼ����
		/// </summary>
		public const ushort PreloadUpdate = 1205;
		/// <summary>
		/// Ԥ����_��ʼ����
		/// </summary>
		public const ushort PreloadComplete = 1206;

		/// <summary>
		/// Lua�ڴ��ͷ�
		/// </summary>
		public const ushort LuaFullGc = 1208;

		/// <summary>
		/// ��Socket���ӷ������ɹ�
		/// </summary>
		public const ushort OnConnectOKToMainSocket = 1301;
    }
}