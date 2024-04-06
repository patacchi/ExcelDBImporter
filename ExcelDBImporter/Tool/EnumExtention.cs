using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelDBImporter.Tool
{
    /// <summary>
    /// enum型の拡張メソッドを管理するクラス
    /// </summary>
    static class EnumExtention
    {
        //=================================================================================
        //ビットフラグ
        //=================================================================================
        /*
        //usage:
        //フラグ2追加
        EnumExtensions.AddFlag(ref flag, SampleFlag.Flag2);
		
        //フラグ1削除
        EnumExtensions.RemoveFlag(ref flag, SampleFlag.Flag1);
        */

        /// <summary>
        /// ビットフラグから一つの状態を除く
        /// </summary>
        public static void RemoveFlag<T>(ref T self, T flag) where T : struct, Enum
        {
            self = (T)Enum.ToObject(typeof(T), (int)(object)self & ~(int)(object)flag);
        }

        /// <summary>
        /// ビットフラグに一つの状態を追加
        /// </summary>
        public static void AddFlag<T>(ref T self, T flag) where T : struct, Enum
        {
            self = (T)Enum.ToObject(typeof(T), (int)(object)self | (int)(object)flag);
        }
        //==============================================================
        //ビットフラグ操作 直接Uint引数 ビット演算子の替わり
        //==============================================================
        /// <summary>
        /// ビット追加( | ビット演算子の代用)
        /// </summary>
        /// <param name="OriginFlagValue">ビット追加するオリジナルのUint</param>
        /// <param name="TargetFlagValue">追加したいフラグのUint</param>
        /// <returns></returns>
        public static uint AddFlag(uint OriginFlagValue, uint TargetFlagValue)
        {
            return OriginFlagValue | TargetFlagValue;
        }

        /// <summary>
        /// ビット削除( &~ ビット演算子の代用)
        /// </summary>
        /// <param name="OriginFlagValue">ビット削除したいオリジナルのUint</param>
        /// <param name="TargetFlagValue">削除したいフラグのUint</param>
        /// <returns></returns>
        public static uint RemoveFlag(uint  OriginFlagValue,uint TargetFlagValue)
        {
            return OriginFlagValue & ~TargetFlagValue;
        }
    }
}
