using System;
using System.Linq.Expressions;
using System.Reflection;
using Data.Crates;
using Data.Entities;
using Data.Infrastructure.AutoMapper;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hub.Managers
{
    partial class CrateManager
    {
        public class CrateStorageStorageUpdater : ICrateStorageUpdater
        {
            private readonly Expression _expr;
            private readonly Func<object> _getValue;
            private Action<CrateStorage> _setValue;
            private bool _discardChanges;

            public CrateStorage CrateStorage
            {
                get;
                set;
            }

            public CrateStorageStorageUpdater(Expression<Func<CrateStorageDTO>> expr)
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

            public CrateStorageStorageUpdater(Expression<Func<string>> expr)
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
            
            private void InitializeAccessors(MemberInfo memberInfo, object instance, Func<object, CrateStorage> readConverter, Func<CrateStorage, object> writeConverter)
            {
                if (memberInfo is FieldInfo)
                {
                    CrateStorage = readConverter(((FieldInfo)memberInfo).GetValue(instance));
                    _setValue = x => ((FieldInfo)memberInfo).SetValue(instance, writeConverter(x));
                }
                else if (memberInfo is PropertyInfo)
                {
                    CrateStorage = readConverter(((PropertyInfo)memberInfo).GetValue(instance));
                    _setValue = x => ((PropertyInfo)memberInfo).SetValue(instance, writeConverter(x));
                }
            }

            private CrateStorage ReadStorage(object value)
            {
                if (value is string)
                {
                    return CrateStorageSerializer.Default.ConvertFromDto(CrateStorageFromStringConverter.Convert((string)value));
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
                    _setValue(CrateStorage ?? new CrateStorage());
                }
            }
        }
    }
}
