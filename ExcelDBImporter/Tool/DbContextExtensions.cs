using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ExcelDBImporter.Tool
{
    /// <summary>
    /// デフォルトキーと除外パターンを実装しているクラスに付けるインターフェイス
    /// </summary>
    /// <typeparam name="TEntity">モデルクラス</typeparam>
    public interface IHaveDefaultPattern<TEntity> where TEntity : class
    {
        Expression<Func<TEntity, object>> DefaultKeyPattern { get; }
        Expression<Func<TEntity, object>>? DefauldExcludePattern { get; }
    }

    public static class TypeExtensions
    {
        // Nullable型かどうかを判定する拡張メソッド
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }

    /// <summary>
    /// DBContextの拡張メソッド
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// List<TEntity>を引数に、UPSertを行う
        /// </summary>
        /// <typeparam name="TEntity">モデルクラス,IHaveDefaultPatternインターフェースの実装を必須とする</typeparam>
        /// <param name="context"></param>
        /// <param name="entities">UPSert対象のList<TEntity></param>
        /// <returns>UpsertOprationのインスタンス、このインスタンスに対してメソッドチェーンを実行する</returns>
        /*
        使用例:

        context.UpsertEntities(entitiesList)
            .WithKeys(key => new
            {
                key.DateInOut,
                key.StrOrderOrSeiban,
                key.DblInputNum,
                key.DblDeliverNum,
                key.StrTehaiCode
            })
            .WithExcludedFields(ex => ex.StrDummy!)
         */

        public static UpsertOperation<TEntity> UpsertEntities<TEntity>(this DbContext context, List<TEntity> entities)
        where TEntity : class,IHaveDefaultPattern<TEntity>
        {
            return new UpsertOperation<TEntity>(context, entities);
        }

        /// <summary>
        /// エンティティクラスのオートインクリメントフィールドのStringをIEnumerableで返す
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IEnumerable<string> FindAutoIncrementFields<TEntity>(this DbContext context)
        {
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            if (entityType == null)
            {
                //エンティティタイプが見つからなかった場合
                yield break;
            }
            foreach (var property in entityType.GetProperties())
            {
                if (property.ValueGenerated == ValueGenerated.OnAdd)
                {
                    yield return property.Name;
                }
            }
        }
        /// <summary>
        /// 指定されたフィールド群のうち、相違があればエンティティを返す
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity">検査対象のエンティティ</param>
        /// <param name="fieldSelector">対象フィールドをラムダ式形式で指定</param>
        /// <returns></returns>
        public static TEntity? GetEntitySpecFieldEqual<TEntity>(this DbContext context, TEntity entity,
                                                          Expression<Func<TEntity, object>> fieldSelector) where TEntity : class
        {
            // フィールド名を取得
            var fieldNames = GetFieldNames(fieldSelector);
            /*
            // 比較関数を自動的に生成
            var equalityComparer = GenerateEqualityComparer<TEntity>(fieldNames);
            */
            //フィールド名と値のDictinaoryを得る
            Dictionary<string,object> dicEqualField = ConvertToFieldValueDic(entity, fieldNames);
            //得られたDictionaryから比較関数を取得
            var FirstExpression = CreateFilterExpression<TEntity>(dicEqualField);

            // 比較関数を使用してエンティティを取得
            var existingEntity = context.Set<TEntity>().FirstOrDefault(FirstExpression);
            return existingEntity;
        }

        /// <summary>
        /// デフォルトパターンで指定されたフィールドで、相違があればエンティティを返す
        /// </summary>
        /// <typeparam name="TEntity">フィールドパターン指定しない場合、IHaveDefaultPattern<TEntity>の実装が必須</typeparam>
        /// <param name="context"></param>
        /// <param name="entity">検査対象のエンティティ</param>
        /// <returns></returns>
        public static TEntity? GetEntitySpecFieldEqual<TEntity>(this DbContext context, TEntity entity) 
                                                                where TEntity : class,IHaveDefaultPattern<TEntity>
        {
            IHaveDefaultPattern<TEntity>? IFaceDefault = GetDefaultPatternInterFace<TEntity>();
            return entity;
        }
        /// <summary>
        /// IHaveDefaultPatternインターフェースを実装していれば、デフォルトパターンを得る
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <returns>IHaveDefaultPattern<TEntity>のインターフェース、インターフェース実装無い場合はnull</returns>
        /// <exception cref="ArgumentException"></exception>
        private static IHaveDefaultPattern<TEntity>? GetDefaultPatternInterFace<TEntity>() where TEntity : class
        {
            // モデルクラスが IHaveDefaultPattern インターフェースを実装している場合、デフォルトのパターンを自動的に適用する
            if (typeof(IHaveDefaultPattern<TEntity>).IsAssignableFrom(typeof(TEntity)))
            {
                var defaultPattern = (IHaveDefaultPattern<TEntity>?)Activator.CreateInstance(typeof(TEntity));
                //IHaveDefaultPatternインターフェースが実装されていれば、インターフェースを返す
                if (defaultPattern != null)
                {
                    return defaultPattern;
                }
                return null;
            }
            throw new ArgumentException($"{nameof(TEntity)} クラスは {nameof(IHaveDefaultPattern<TEntity>)} インターフェースを実装していませんでした");
        }
        // ラムダ式からフィールド名を取得するメソッド
        private static string[] GetFieldNames<TEntity>(Expression<Func<TEntity, object>> fieldSelector)
        {
            var body = fieldSelector.Body;
            if (body is NewExpression newExpression)
            {
                if (newExpression.Members == null)
                {
                    //式本体が見つからなかった場合(?)
                    return [];
                }
                return newExpression.Members.Select(m => m.Name).ToArray();
            }
            throw new ArgumentException("Invalid field selector expression.");
        }

        /// <summary>
        /// モデルクラスTentityとフィールド名の配列を引数にして、Dictionary<strFieldName,objectValue>を返す
        /// </summary>
        /// <typeparam name="TEntity">モデルクラス</typeparam>
        /// <param name="entity"></param>
        /// <param name="fields">フィールド名の配列</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private static Dictionary<string, object> ConvertToFieldValueDic<TEntity>(TEntity entity, string[] fields)
        {
            var dictionary = new Dictionary<string, object>();
            var type = typeof(TEntity);

            foreach (var field in fields)
            {
                var property = type.GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    var value = property.GetValue(entity);
                    if(value != null)
                    {
                        dictionary.Add(field, value);
                    }
                }
                else
                {
                    throw new ArgumentException($"Property '{field}' not found in type {type.Name}");
                }
            }
            return dictionary;
        }

        /// <summary>
        /// フィールド名とValueのDictionaryを引数に取り、比較関数を自動的に作成するメソッド
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="criteria">Dictionary<string,object> で、キーがフィールド名、Valueがその値</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>(Dictionary<string, object> criteria)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var expressions = new List<Expression>();

            foreach (var criterion in criteria)
            {
                var property = typeof(TEntity).GetProperty(criterion.Key);
                if (property == null)
                {
                    throw new ArgumentException($"Property '{criterion.Key}' not found in type {typeof(TEntity).Name}");
                }

                var propertyExpression = Expression.Property(parameter, property);

                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var value = Convert.ChangeType(criterion.Value, targetType);
                ConstantExpression valueExpression;
                Expression equalExpression;
                if (property.PropertyType.IsNullable() && targetType.IsValueType)
                {
                    // Nullable型のプロパティと非Nullable型の値を比較する場合、Nullable型のプロパティもNullable型に変換する
                    var nullableType = typeof(Nullable<>).MakeGenericType(targetType);
                    var nullablePropertyExpression = Expression.Convert(propertyExpression, targetType);
                    valueExpression = Expression.Constant(value, targetType);
                    equalExpression = Expression.Equal(nullablePropertyExpression, valueExpression);
                }
                else
                {
                    valueExpression = Expression.Constant(value, targetType);
                    equalExpression = Expression.Equal(propertyExpression, valueExpression);
                }
                expressions.Add(equalExpression);
            }

            var body = expressions.Aggregate(Expression.AndAlso);
            return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        }

        // 比較関数を自動的に生成するメソッド
        private static Func<TEntity, TEntity, bool> GenerateEqualityComparer<TEntity>(string[] propertyNames)
        {
            var entityType = typeof(TEntity);
            var parameter1 = Expression.Parameter(entityType, "entity");
            var parameter2 = Expression.Parameter(entityType, "entity2");

            var expressions = propertyNames.Select(propertyName =>
            {
                var property = entityType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                    throw new ArgumentException($"Property '{propertyName}' not found in entity '{entityType.Name}'.");

                var property1 = Expression.Property(parameter1, property);
                var property2 = Expression.Property(parameter2, property);
                return Expression.Equal(property1, property2);
            });

            var body = expressions.Aggregate(Expression.AndAlso);

            var lambda = Expression.Lambda<Func<TEntity, TEntity, bool>>(body, parameter1, parameter2);
            return lambda.Compile();
        }


        public class UpsertOperation<TEntity> where TEntity : class
        {
            public Expression<Func<TEntity, object>>? KeySelectors { get; set; } = null!;
            public Expression<Func<TEntity, object>>? ExcludedFields { get; set; }

            public DbContext Context { get; }

            public List<TEntity> Entities { get; }
            /// <summary>
            /// 除外フィールドのフィールド名の配列
            /// </summary>
            private string[] StrExcludeArray { get; set; } = null!;

            public UpsertOperation(DbContext context, List<TEntity> entities)
            {
                Context = context;
                Entities = entities;
                IHaveDefaultPattern<TEntity>? IFaceDefault = GetDefaultPatternInterFace<TEntity>();
                if (IFaceDefault != null)
                {
                    //デフォルトパターンが見つかったら設定する
                    KeySelectors = IFaceDefault.DefaultKeyPattern;
                    if (IFaceDefault.DefauldExcludePattern != null)
                    {
                        //除外パターンはオプションなので、指定され手入れば設定する
                        ExcludedFields = IFaceDefault.DefauldExcludePattern;
                    }
                }
            }

            /// <summary>
            /// キーを独自に指定する場合、ラムダ式で指定する
            /// </summary>
            /// <param name="keySelectors">Func<TEntity, object>[](のラムダ式)</param>
            /// <returns>メソッドチェーンのために自分自身のクラスを返す</returns>
            public UpsertOperation<TEntity> WithKeys(Expression<Func<TEntity, object>>? keySelectors)
            {
                KeySelectors = keySelectors;
                return this;
            }

            /// <summary>
            /// Upsert対象から除外するフィールドをラムダ式で指定する
            /// </summary>
            /// <param name="excludedFields">Func<TEntity, object>[](のラムダ式)</param>
            /// <returns>メソッドチェーンのために自分自身のクラスを返す</returns>
            public UpsertOperation<TEntity> WithExcludedFields(Expression<Func<TEntity, object>> excludedFields)
            {
                ExcludedFields = excludedFields;
                return this;
            }

            //private static bool IsNewEntity(DbContext dbContext, TEntity entity, Func<TEntity, object>[] keySelectors,out TEntity? existingEntity)
            private static bool IsNewEntity(DbContext dbContext, TEntity entity, Func<TEntity, object>[] keySelectors)
            {
                var existingEntitiesQuery = dbContext.Set<TEntity>().AsQueryable();

                foreach (var keySelector in keySelectors)
                {
                    var keyFieldValue = keySelector(entity);
                    existingEntitiesQuery = existingEntitiesQuery.Where(existingEntity => keySelector(existingEntity).Equals(keyFieldValue));
                }
                
                return existingEntitiesQuery.FirstOrDefault() == null;
            }
            /// <summary>
            /// UPSertを実行する
            /// </summary>
            /// <param name="NotExcludeAutoIncrement">false 指定するとオートインクリメント列を除外リストに自動追加しない、デフォルトは追加する</param>
            /// <exception cref="KeyNotFoundException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public void Execute(bool ExcludeAutoIncrement = true)
            {
                DbSet<TEntity> dbSet = Context.Set<TEntity>();
                //キーパターンが定義されているかどうかチェックする
                if (KeySelectors == null)
                {
                    //この時点でKeySelectorsにラムダ式が設定されていなかったら例外を投げて終了する
                    throw new ArgumentException($"{nameof(TEntity)} クラスは {nameof(IHaveDefaultPattern<TEntity>)} インターフェースを実装していませんでした");
                }
                if (ExcludedFields != null)
                {
                    //除外パターンが設定されていた場合、除外フィールドのフィールド名の配列を得る
                    StrExcludeArray = GetFieldNames(ExcludedFields);
                }


                foreach (var entity in Entities)
                {
                    /*
                    object[] keyValues;

                    if (KeySelectors != null && KeySelectors.Length > 0)
                    {
                        keyValues = KeySelectors.Select(selector => selector(entity)).ToArray();
                    }
                    else
                    {
                        var entry = Context.Entry(entity);
                        IEntityType? entityType = Context.Model.FindEntityType(typeof(TEntity));
                        if (entityType != null)
                        {
                            var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties;
                            if (primaryKeyProperties != null)
                            {
                                keyValues = primaryKeyProperties
                                    .Select(x => entry.Property(x.Name)?.CurrentValue ?? DBNull.Value)
                                    .ToArray();
                            }
                            else
                            {
                                // primaryKeyProperties が null の場合の処理
                                MessageBox.Show($"指定のエンティティでキーが見つかりませんでした。 {nameof(TEntity)}");
                                throw new KeyNotFoundException(nameof(TEntity));
                            }
                        }
                        else
                        {
                            MessageBox.Show($"指定のエンティティタイプが見つかりませんでした {nameof(TEntity)}");
                            throw new ArgumentException(nameof(TEntity));
                        }
                    }
                    */
                    /*
                    var existingEntity = dbSet.Find(keyValues);

                    if (existingEntity != null)
                    {
                        // 複合キー条件に一致するエンティティが存在する場合、Updateを行う
                        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                    }
                    else
                    {
                        // 複合キー条件に一致するエンティティが存在しない場合、Insertを行う
                        dbSet.Add(entity);
                    }
                    */

                    /*
                    //既存エンティティかどうかにより処理を分岐する
                    if (KeySelectors != null && KeySelectors.Length > 0)
                    {
                        TEntity? existingEntity = null;
                        //if (IsNewEntity(_context, entity, _keySelectors,out existingEntity))
                        if (IsNewEntity(Context, entity, KeySelectors))
                            {
                            //新規エンティティの場合
                            //Insert
                            dbSet.Add(entity);
                        }
                        else
                        {
                            //既存のエンティティの場合
                            //Update
                            if (existingEntity != null)
                            {
                                //複合キー条件に一致するエンティティが存在する場合、Updateを行う
                                Context.Entry(existingEntity).CurrentValues.SetValues(entity);
                            }
                        }
                    }
                    */

                    //既存エンティティかどうかにより処理を分岐する
                    TEntity? existing = Context.GetEntitySpecFieldEqual(entity, KeySelectors);

                    switch (existing != null)
                    {
                        case true:
                            {
                                //既存エンティティだった場合
                                //Update処理を行う
                                Context.Entry(existing).CurrentValues.SetValues(entity);
                                // 除外フィールドが指定されている場合、そのフィールドを除外する
                                if (StrExcludeArray.Length > 0)
                                {
                                    var entry = Context.Entry(existing);
                                    foreach (string strExclude in StrExcludeArray)
                                    {
                                        entry.Property(strExclude!).IsModified = false;
                                    }
                                }
                                if (ExcludeAutoIncrement)
                                {
                                    //オートインクリメント列自動除外有効の時
                                    //オートインクリメント列を取得し、除外フィールドとしてマーク
                                    IEnumerable<string> AutoIncrements = Context.FindAutoIncrementFields<TEntity>();
                                    if (AutoIncrements.Any())
                                    {
                                        //オートインクリメント列が見つかったら、除外フィールドとしてマークする
                                        var entry = Context.Entry(existing);
                                        foreach (string StrAutoProp in AutoIncrements)
                                        {
                                            entry.Property(StrAutoProp).IsModified = false;
                                        }
                                    }
                                }
                                break;
                            }
                        case false:
                            {
                                //新規エンティティだった場合
                                // 除外フィールドが指定されている場合、そのフィールドを除外する
                                if (StrExcludeArray.Length > 0)
                                {
                                    var entry = Context.Entry(entity);
                                    foreach (string strExclude in StrExcludeArray)
                                    {
                                        //除外フィールドの値をnullに(無かったことに)
                                        entry.Property(strExclude!).CurrentValue = null;
                                    }
                                }
                                if (ExcludeAutoIncrement)
                                {
                                    //オートインクリメント列自動除外有効の時
                                    //オートインクリメント列を取得し、除外フィールドとしてマーク
                                    IEnumerable<string> AutoIncrements = Context.FindAutoIncrementFields<TEntity>();
                                    if (AutoIncrements.Any())
                                    {
                                        //オートインクリメント列が見つかったら、除外フィールドとしてマークする
                                        var entry = Context.Entry(entity);
                                        foreach (string StrAutoProp in AutoIncrements)
                                        {
                                            //オートインクリメントフィールドの値をnullに(無かったことに)
                                            entry.Property(StrAutoProp).CurrentValue = null;
                                        }
                                    }
                                }
                                //Insert処理を行う
                                Context.Add(entity);
                                break;
                            }
                     }
                }
                Context.SaveChanges();
            }
        }
    }

}
