using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouYou;

public class TestAnimation : MonoBehaviour
{
	//private BaseSprite testRoleCtrl;
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.J))
		{
			//testRoleCtrl.PlayAnim("Skill6");
			//float animLen = m_RoleAnimInfoDic[GetRoleAnimInfoId("Skill6")].CurrPlayable.GetAnimationClip().length;//动画的长度
			//Debug.LogError(animLen);
		}
		if (Input.GetKeyDown(KeyCode.K))
		{
			//testRoleCtrl.PlayAnim("Run");
		}
	}
}
