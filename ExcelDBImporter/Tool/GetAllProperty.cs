using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ExcelDBImporter.Tool
{
    public class GetAllProperty
    {
        /// <summary>
        /// 指定された名前空間以下にあるクラスと公開プロパティ名一覧を返す
        /// </summary>
        /// <param name="StrNameSpace">名前空間をStringで指定</param>
        /// <returns>ClassAndProperty型のList</returns>
        public List<ClassAndProperty> GetClassnameAndPropertyPairBasedNameSpace(string StrNameSpace)
        {
            //実行中のアセンブリ(DLL)取得
            Assembly assm = Assembly.GetExecutingAssembly();
            //指定した名前空間のクラスをすべて取得
            IEnumerable<Type> types = assm.GetTypes()
                .Where(p => p.Namespace == StrNameSpace)
                .OrderBy(o => o.Name)
                .Select(s => s);
            List<ClassAndProperty> listProperty = [];
            foreach (Type t in types)
            {
                string[] propertyNames = t.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                                    .Where(o => o.MemberType.ToString() == "Property")
                                    .Select(p => p.Name)
                                    .ToArray();
                foreach (string propertyName in propertyNames)
                {
                    listProperty.Add(new ClassAndProperty(t.Name, propertyName));
                }
            }
            return listProperty;
        }
        /// <summary>
        /// 指定された名前空間、クラス名のプロパティ一覧を取得する
        /// </summary>
        /// <param name="StrNameSpace">名前空間</param>
        /// <param name="StrClassName">クラス名</param>
        /// <returns>ClassAndProperty型のリスト</returns>
        public List<ClassAndProperty> GetPropertyByClassNameAndNamespace(string? StrNameSpace, string StrClassName)
        {
            //実行中のアセンブリ(DLL)取得
            Assembly assm = Assembly.GetExecutingAssembly();
            //指定したクラス名のTypeを取得
            IEnumerable<Type> typeone = assm.GetTypes()
            .Where(p => p.Namespace == StrNameSpace && p.Name == StrClassName)
            .Select(s => s);
            List<ClassAndProperty> listProperty = [];
            if (typeone == null)
            {
                MessageBox.Show("指定されたクラス名は見つかりませんでした\n" + StrClassName);
                return listProperty;
            }
            foreach (Type t in typeone) 
            {
                //クラスのPublicなPropertyの配列を得る
                string[] propertyNames = t.GetMembers(BindingFlags.Instance| BindingFlags.Public)
                                        .Where(o => o.MemberType.ToString() == "Property")
                                        .Select(p => p.Name)
                                        .ToArray();
                foreach (string propertyName in propertyNames)
                {
                    listProperty.Add(new ClassAndProperty(t.Name,propertyName));
                }
            }
            return listProperty;
        }

        // Enumの要素がMicrosoft.Entity.FrameworkCoreのCommentアノテーションを持っているかどうかをチェックし、内容を取得するジェネリック型メソッド
        public static string GetEnumComment<T>(T value) where T : Enum
        {
            // Enumの型を取得
            Type type = typeof(T);

            // Enumのフィールド情報を取得
            FieldInfo fieldInfo = type.GetField(value.ToString())!;

            // Microsoft.Entity.FrameworkCoreのCommentアノテーションを取得
            CommentAttribute[] attributes = (CommentAttribute[])fieldInfo.GetCustomAttributes(typeof(CommentAttribute), false);

            if (attributes.Length > 0)
            {
                // アノテーションが存在する場合は、その内容を返す
                return attributes[0].Comment;
            }
            else
            {
                // アノテーションが存在しない場合は、要素名をそのまま返す
                return value.ToString();
            }
        }

    }
    public class ClassAndProperty(string className, string propertyName)
    {
        public string ClassName { get; set; } = className;
        public string PropertyName { get; set; } = propertyName;
    }
}
