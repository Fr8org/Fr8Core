using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Infrastructure;
using Newtonsoft.Json;

namespace Fr8Data.Managers
{
    partial class CrateManager
    {
        public class UpdatableCrateStorageStorage : IUpdatableCrateStorage
        {
            private readonly Expression _expr;
            private readonly Func<object> _getValue;
            private Action<ICrateStorage> _setValue;
            private bool _discardChanges;
            private ICrateStorage _crateStorage;

            public int Count
            {
                get { return _crateStorage.Count; }
            }

            public UpdatableCrateStorageStorage(Expression<Func<CrateStorageDTO>> expr)
            {
                var memberExpr = expr.Body as MemberExpression;

                if (memberExpr == null)
                {
                    throw new ArgumentException("Only member expressions is supported");
                }

                var me = (MemberExpression)memberExpr.Expression;
                var ce = (ConstantExpression)me.Expression;
                var fieldInfo = ce.Value.GetType().GetField(me.Member.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                InitializeAccessors(memberExpr.Member, fieldInfo.GetValue(ce.Value), ReadStorage, x => CrateStorageSerializer.Default.ConvertToDto(x));
            }

            public UpdatableCrateStorageStorage(Expression<Func<string>> expr)
            {
                var memberExpr = expr.Body as MemberExpression;

                if (memberExpr == null)
                {
                    throw new ArgumentException("Only member expressions is supported");
                }

                var me = (MemberExpression)memberExpr.Expression;
                var ce = (ConstantExpression)me.Expression;
                var fieldInfo = ce.Value.GetType().GetField(me.Member.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                InitializeAccessors(memberExpr.Member, fieldInfo.GetValue(ce.Value), ReadStorage, x => JsonConvert.SerializeObject(CrateStorageSerializer.Default.ConvertToDto(x)));
            }

            public void Replace(ICrateStorage crateStorage)
            {
                _crateStorage = crateStorage;
            }

            public void Flush()
            {
                _setValue(_crateStorage ?? new CrateStorage());
            }

            public void Add(Crate crate)
            {
                _crateStorage.Add(crate);
            }

            public void Add(params Crate[] crates)
            {
                foreach (var crate in crates)
                {
                    _crateStorage.Add(crate);
                }
            }

            public void Clear()
            {
                _crateStorage.Clear();
            }

            public int Remove(Predicate<Crate> predicate)
            {
                return _crateStorage.Remove(predicate);
            }

            public int Replace(Predicate<Crate> predicate, Crate crate)
            {
                return _crateStorage.Replace(predicate, crate);
            }

            public IEnumerator<Crate> GetEnumerator()
            {
                return _crateStorage.GetEnumerator();
            }

            private void InitializeAccessors(MemberInfo memberInfo, object instance, Func<object, ICrateStorage> readConverter, Func<ICrateStorage, object> writeConverter)
            {
                if (memberInfo is FieldInfo)
                {
                    _crateStorage = readConverter(((FieldInfo)memberInfo).GetValue(instance));
                    _setValue = x => ((FieldInfo)memberInfo).SetValue(instance, writeConverter(x));
                }
                else if (memberInfo is PropertyInfo)
                {
                    _crateStorage = readConverter(((PropertyInfo)memberInfo).GetValue(instance));
                    _setValue = x => ((PropertyInfo)memberInfo).SetValue(instance, writeConverter(x));
                }
            }

            private ICrateStorage ReadStorage(object value)
            {
                if (value is string)
                {
                    return CrateStorageSerializer.Default.ConvertFromDto(StringToCrateStorageDTOConverter.Convert((string)value));
                }

                if (value is CrateStorageDTO)
                {
                    return CrateStorageSerializer.Default.ConvertFromDto((CrateStorageDTO)value);
                }

                return new CrateStorage();
            }

            public void DiscardChanges()
            {
                _discardChanges = true;
            }

            public void Dispose()
            {
                if (!_discardChanges)
                {
                   Flush();
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_crateStorage).GetEnumerator();
            }
        }
    }
}
