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
                string[] propertyNames = t.GetMembers(BindingFlags.Instance| BindingFlags.Public)
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
    }
    public class ClassAndProperty(string className, string propertyName)
    {
        public string ClassName { get; set; } = className;
        public string PropertyName { get; set; } = propertyName;
    }
}
