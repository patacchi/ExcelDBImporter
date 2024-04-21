using ExcelDBImporter.Context;
using ExcelDBImporter.Models;
using ExCSS;
using Microsoft.EntityFrameworkCore;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

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
            // アノテーションが存在しない場合は、要素名をそのまま返す
            return value.ToString();
        }
        /// <summary>
        /// プロパティのラムダ式を渡して、コメント属性の文字列を返す
        /// </summary>
        /// <typeparam name="T">クラス</typeparam>
        /// <param name="propertyExpression">プロパティをラムダ式で</param>
        /// <returns>コメント属性があればそれを、無ければプロパティ名そのまま</returns>
        /// <exception cref="ArgumentException">プロパティのラムダ式じゃない時エラー</exception>
        public static string? GetPropertyComment<T>(string StrPropName) where T : class
        {
            Type type = typeof(T);
            //プロパティを取得
            PropertyInfo? propertyInfo = type.GetProperty(StrPropName);
            if (propertyInfo == null) { return StrPropName; }
            //コメント属性を取得
            CommentAttribute[] attributes = (CommentAttribute[])propertyInfo.GetCustomAttributes(typeof(CommentAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Comment;
            }
            //プロパティにコメントが付いていない場合はそのままプロパティ名を返す
            return StrPropName;
        }
        /// <summary>
        /// DBモデルクラスのカラム名とファイルのフィールド名のDictionaryを返す
        /// </summary>
        /// <param name="StrModelClassName">DB登録するモデルクラスの名前</param>
        /// <param name="StrTableDBFileField">TableDbColumnテーブルのファイルクラスのカラム名</param>
        /// <returns></returns>
        public Dictionary<string,string> GetDBandFileFieldNamePair(string StrModelSpaceName,
                                                                   string StrModelClassName,
                                                                   string StrTableDBFileColumn)
        {
            //クラス名から、プロパティ一覧を得る
            List<ClassAndProperty> classAndProperties = GetPropertyByClassNameAndNamespace(StrModelSpaceName, StrModelClassName);
            //結果格納用のDictionary
            Dictionary<string, string> DicDBandField = new();
            using ExcelDbContext dbContext = new();
            foreach (ClassAndProperty clasandprop in classAndProperties)
            {
                //全てのプロパティについてチェック
                TableDBcolumnNameAndExcelFieldName? DBexisting = dbContext.TableDBcolumnNameAndExcelFieldNames
                                                                .FirstOrDefault(t =>t.StrClassName == clasandprop.ClassName
                                                                && t.StrDBColumnName == clasandprop.PropertyName );
                if (DBexisting == null)
                {
                    //見つからない場合→中段
                    MessageBox.Show($"{nameof(GetDBandFileFieldNamePair)} {clasandprop.ClassName} の {clasandprop.PropertyName}プロパティがDBから見つかりませんでした。");
                    return new Dictionary<string, string>();
                }
                string StrFieldName = DBexisting.GetType()
                    .GetMember(StrTableDBFileColumn)
                    .ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(StrFieldName))
                {
                    MessageBox.Show($"{nameof(GetDBandFileFieldNamePair)} {clasandprop.ClassName} の {clasandprop.PropertyName}プロパティがDBから見つかりませんでした。");
                    return new Dictionary<string, string>();
                }
                DicDBandField.Add(clasandprop.PropertyName,StrFieldName);
            }
            return DicDBandField;
        }
    }
        public class ClassAndProperty(string className, string propertyName)
    {
        public string ClassName { get; set; } = className;
        public string PropertyName { get; set; } = propertyName;
    }
    
}
