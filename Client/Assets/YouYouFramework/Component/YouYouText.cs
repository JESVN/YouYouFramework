using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouYou
{
    /// <summary>
    /// Text�Զ�������
    /// </summary>
    public class YouYouText : Text
    {
        [Header("���ػ����Ե�Key")]
        [SerializeField]
        private string m_Localization;

        protected override void Start()
        {
            base.Start();
            if (GameEntry.Localization != null)
            {
                text = GameEntry.Localization.GetString(m_Localization);
            }
        }
    }
}