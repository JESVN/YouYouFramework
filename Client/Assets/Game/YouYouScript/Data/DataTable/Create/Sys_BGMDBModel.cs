
using System.Collections;
using System.Collections.Generic;
using System;

namespace YouYou
{
    /// <summary>
    /// Sys_BGM数据管理
    /// </summary>
    public partial class Sys_BGMDBModel : DataTableDBModelBase<Sys_BGMDBModel, Sys_BGMEntity>
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public override string DataTableName { get { return "Sys_BGM"; } }

        /// <summary>
        /// 加载列表
        /// </summary>
        protected override void LoadList(MMO_MemoryStream ms)
        {
            int rows = ms.ReadInt();
            int columns = ms.ReadInt();

            for (int i = 0; i < rows; i++)
            {
                Sys_BGMEntity entity = new Sys_BGMEntity();
                entity.Id = ms.ReadInt();
                entity.AssetPath = ms.ReadUTF8String();
                entity.Volume = ms.ReadFloat();
                entity.IsLoop = (byte)ms.ReadByte();
                entity.IsFadeIn = (byte)ms.ReadByte();
                entity.IsFadeOut = (byte)ms.ReadByte();
                entity.Priority = (byte)ms.ReadByte();

                m_List.Add(entity);
                m_Dic[entity.Id] = entity;
            }
        }
    }
}