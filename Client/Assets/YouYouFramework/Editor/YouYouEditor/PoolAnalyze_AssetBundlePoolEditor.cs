using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YouYou;

[CustomEditor(typeof(PoolAnalyze_AssetBundlePool))]
public class PoolAnalyze_AssetBundlePoolEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		#region ��Դ����
		GUILayout.Space(10);

		GUIStyle titleStyle = new GUIStyle();
		titleStyle.normal.textColor = new Color(102, 232, 255);

		if (GameEntry.Pool != null)
		{
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("�´��ͷ�ʣ��ʱ��: " + Mathf.Abs(Time.time - (GameEntry.Pool.ReleaseAssetBundleNextRunTime + GameEntry.Pool.ReleaseAssetBundleInterval)), titleStyle);
			GUILayout.EndHorizontal();
		}
		//===================��Դ�ر�������==================
		GUILayout.Space(10);
		GUILayout.BeginVertical("box");
		GUILayout.BeginHorizontal("box");
		GUILayout.Label("��Դ��");
		GUILayout.Label("ʣ��ʱ��", GUILayout.Width(50));
		GUILayout.EndHorizontal();

		if (GameEntry.Pool != null)
		{
			foreach (var item in GameEntry.Pool.AssetBundlePool.InspectorDic)
			{
				GUILayout.BeginHorizontal("box");
				GUILayout.Label(item.Key);

				float remain = Mathf.Max(0, GameEntry.Pool.ReleaseAssetBundleNextRunTime - (Time.time - item.Value.LastUseTime));

				titleStyle.fixedWidth = 50;
				GUILayout.Label(remain.ToString(), titleStyle);
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
		//=================================������������==========================

		serializedObject.ApplyModifiedProperties();
		//�ػ�
		Repaint();
		#endregion
	}
}
