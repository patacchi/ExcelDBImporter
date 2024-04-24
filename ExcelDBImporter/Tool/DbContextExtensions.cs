using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
/// DBContextの拡張メソッド
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// List<TEntity>を引数に、UPSertを行う
    /// </summary>
    /// <typeparam name="TEntity">モデルクラス</typeparam>
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
        where TEntity : class
    {
        return new UpsertOperation<TEntity>(context, entities);
    }

    public class UpsertOperation<TEntity> where TEntity : class
    {
        private readonly DbContext _context;
        private readonly List<TEntity> _entities;
        private Func<TEntity, object>[] _keySelectors = null!;
        private Func<TEntity, object>[] _excludedFields = null!;

        public UpsertOperation(DbContext context, List<TEntity> entities)
        {
            _context = context;
            _entities = entities;
        }

        /// <summary>
        /// キーを独自に指定する場合、ラムダ式で指定する
        /// </summary>
        /// <param name="keySelectors">Func<TEntity, object>[](のラムダ式)</param>
        /// <returns>メソッドチェーンのために自分自身のクラスを返す</returns>
        public UpsertOperation<TEntity> WithKeys(params Func<TEntity, object>[] keySelectors)
        {
            _keySelectors = keySelectors;
            return this;
        }

        /// <summary>
        /// Upsert対象から除外するフィールドをラムダ式で指定する
        /// </summary>
        /// <param name="excludedFields">Func<TEntity, object>[](のラムダ式)</param>
        /// <returns>メソッドチェーンのために自分自身のクラスを返す</returns>
        public UpsertOperation<TEntity> WithExcludedFields(params Func<TEntity, object>[] excludedFields)
        {
            _excludedFields = excludedFields;
            return this;
        }

        public void Execute()
        {
            DbSet<TEntity> dbSet = _context.Set<TEntity>();

            foreach (var entity in _entities)
            {
                object[] keyValues;

                if (_keySelectors != null && _keySelectors.Length > 0)
                {
                    keyValues = _keySelectors.Select(selector => selector(entity)).ToArray();
                }
                else
                {
                    var entry = _context.Entry(entity);
                    var entityType = _context.Model.FindEntityType(typeof(TEntity));
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

                // 除外フィールドが指定されている場合、そのフィールドを除外する
                if (_excludedFields != null)
                {
                    var entry = _context.Entry(entity);
                    foreach (var excludedField in _excludedFields)
                    {
                        entry.Property(excludedField(entity).ToString()!).IsModified = false;
                    }
                }
            }

            _context.SaveChanges();
        }
    }
}
