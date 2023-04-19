using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Main
{
    public class SysDataMgr
    {
        /// <summary>
        /// ������������
        /// </summary>
        public ChannelConfigEntity CurrChannelConfig { get; private set; }

        /// <summary>
        /// Http����ʧ�ܺ����Դ���
        /// </summary>
        public int HttpRetry { get; private set; }
        /// <summary>
        /// Http����ʧ�ܺ����Լ�����룩
        /// </summary>
        public int HttpRetryInterval { get; private set; }

        /// <summary>
        /// ���ڼ���ʱ����ı��ط�����ʱ��
        /// </summary>
        public long CurrServerTime
        {
            get
            {
                if (CurrChannelConfig == null) return (long)Time.unscaledTime;
                return CurrChannelConfig.ServerTime + (long)Time.unscaledTime;
            }
        }

        public SysDataMgr()
        {
            CurrChannelConfig = new ChannelConfigEntity();

            HttpRetry = MainEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Http_Retry, MainEntry.CurrDeviceGrade);
            HttpRetryInterval = MainEntry.ParamsSettings.GetGradeParamData(YFConstDefine.Http_RetryInterval, MainEntry.CurrDeviceGrade);
        }
    }
}