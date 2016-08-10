using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.Infrastructure.StructureMap;
using Data.Repositories.MultiTenant.Ast;
using Data.Repositories.SqlBased;
using Data.States;

namespace Data.Repositories.MultiTenant.InMemory
{
    class InMemoryMtObjectsStorage : IMtObjectsStorage
    {
        private readonly IMtObjectConverter _converter;
        private readonly List<MtObject> _mtObjects = new List<MtObject>();

        public InMemoryMtObjectsStorage(IMtObjectConverter converter)
        {
            _converter = converter;
        }

        public int Insert(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtObject newObject, AstNode uniqueConstraint)
        {
            if (uniqueConstraint != null)
            {
                foreach (var obj in _mtObjects)
                {
                    var eval = new AstEvaluator(obj, _converter);
                    if (eval.Evaluate(uniqueConstraint))
                    {
                        throw new Exception("Object already exists");
                    }
                }
            }

            _mtObjects.Add(newObject);

            return 1;
        }

        public int Upsert(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtObject newObject, AstNode @where)
        {
            if (where == null)
            {
                _mtObjects.Add(newObject);
                return 1;
            }

            int changed = 0;
            for (int index = 0; index < _mtObjects.Count; index++)
            {
                var obj = _mtObjects[index];
                var eval = new AstEvaluator(obj, _converter);
                if (eval.Evaluate(@where))
                {
                    _mtObjects[index] = newObject;
                    changed ++;
                }
            }

            if (changed == 0)
            {
                _mtObjects.Add(newObject);
            }

            return changed;
        }

        public int Update(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtObject newObject, AstNode @where)
        {
            int changed = 0;
            for (int index = 0; index < _mtObjects.Count; index++)
            {
                var obj = _mtObjects[index];
                var eval = new AstEvaluator(obj, _converter);
                if (eval.Evaluate(@where))
                {
                    _mtObjects[index] = newObject;
                    changed++;
                }
            }

            return changed;
        }

        public IEnumerable<MtObject> Query(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode @where)
        {
            for (int index = 0; index < _mtObjects.Count; index++)
            {
                var obj = _mtObjects[index];
                var eval = new AstEvaluator(obj, _converter);
                if (eval.Evaluate(@where))
                {
                    yield return obj;
                }
            }
        }
        
        public int QueryScalar(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode @where)
        {
            int count = 0;
            
            for (int index = 0; index < _mtObjects.Count; index++)
            {
                var obj = _mtObjects[index];
                var eval = new AstEvaluator(obj, _converter);
                if (eval.Evaluate(@where))
                {
                    count++;
                }
            }

            return count;
        }

        public Guid? GetObjectId(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode where)
        {
            return null;
        }

        public int Delete(ISqlConnectionProvider connectionProvider, string fr8AccountId, MtTypeDefinition type, AstNode @where)
        {
            int changed = 0;

            for (int index = _mtObjects.Count - 1; index >= 0; index--)
            {
                var obj = _mtObjects[index];
                var eval = new AstEvaluator(obj, _converter);
                if (eval.Evaluate(@where))
                {
                    _mtObjects.RemoveAt(index);
                }
            }

            return changed;
        }
    }


    class AstEvaluator
    {
        private readonly Stack<object> _callStack = new Stack<object>();
        private readonly MtObject _context;
        private readonly IMtObjectConverter _converter;

        public AstEvaluator(MtObject context, IMtObjectConverter converter)
        {
            _context = context;
            _converter = converter;
        }

        private object UnwrapNullabe(object value)
        {
            if (value == null)
            {
                return null;
            }

            var type = value.GetType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetProperty("Value").GetValue(value);
            }

            return value;
        }

        private int CompareValues(object left, object right)
        {
            left = UnwrapNullabe(left);
            right = UnwrapNullabe(right);

            if (left == right)
            {
                return 0;
            }

            if (left == null || right == null)
            {
                throw new InvalidOperationException("Unable to compare null withh non null");
            }

            if (left.GetType() != right.GetType())
            {
                throw new InvalidOperationException(string.Format("Unable to compare values of type {0} and {1}", left.GetType().FullName, right.GetType().FullName));
            }

            return Comparer.Default.Compare(left, right);
        }

        private bool ValuesEquals(object left, object right)
        {
            left = UnwrapNullabe(left);
            right = UnwrapNullabe(right);

            return Object.Equals(left, right);
        }

        public bool Evaluate(AstNode node)
        {
            EvaluateRecusive(node);
            return (bool) _callStack.Pop();
        }

        private void EvaluateRecusive (AstNode node)
        {
            if (node is LoadConstNode)
            {
                _callStack.Push(((LoadConstNode)node).Value);
            }
            else if (node is LoadFieldNode)
            {
                var propIndex = ((LoadFieldNode) node).PropertyIndex;
              
                _callStack.Push(_converter.ConvertFromDbCanonicalFormat(_context.Values[propIndex], _context.MtTypeDefinition.Properties.First(x => x.Index == propIndex).MtPropertyType));
            }
            else if (node is BinaryOpNode)
            {
                var bin = (BinaryOpNode) node;

                EvaluateRecusive(bin.Left);
                EvaluateRecusive(bin.Right);
                
                var right = _callStack.Pop();
                var left = _callStack.Pop();
                
                switch (bin.Op)
                {
                    case MtBinaryOp.Gt:
                        _callStack.Push(CompareValues(left, right) > 0);
                        break;

                    case MtBinaryOp.Lt:
                        _callStack.Push(CompareValues(left, right) < 0);
                        break;

                    case MtBinaryOp.Gte:
                        _callStack.Push(CompareValues(left, right) >= 0);
                        break;

                    case MtBinaryOp.Lte:
                        _callStack.Push(CompareValues(left, right) <= 0);
                        break;

                    case MtBinaryOp.Eq:
                        _callStack.Push(ValuesEquals(left, right));
                        break;

                    case MtBinaryOp.Neq:
                        _callStack.Push(!ValuesEquals(left, right));
                        break;

                    case MtBinaryOp.And:
                        _callStack.Push((bool)left && (bool)right);
                        break;

                    case MtBinaryOp.Or:
                        _callStack.Push((bool)left || (bool)right);
                        break;

                    case MtBinaryOp.Xor:
                        _callStack.Push((bool)left ^ (bool)right);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
