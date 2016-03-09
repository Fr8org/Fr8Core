using System;
using System.Collections.Generic;
using System.Text;
using Data.Repositories.MultiTenant.Ast;

namespace Data.Repositories.MultiTenant.Sql
{
    public class AstToSqlConverter
    {
        private static readonly Dictionary<Type, string> ClrToSqlMapping = new Dictionary<Type, string>
        {
            {typeof (int), "int"},
            {typeof (short), "int"},
            {typeof (byte), "int"},
            {typeof (float), "float"},
            {typeof (double), "float"},
            {typeof (DateTime), "datetimeoffset"},
            {typeof (string), "nvarchar"},
        };

        private readonly StringBuilder _result = new StringBuilder();
        private readonly MtTypeDefinition _mtType;
        private readonly IMtObjectConverter _converter;
        private readonly string _tableName;

        public readonly List<object> Constants = new List<object>();

        public string SqlCommand
        {
            get { return _result.ToString(); }
        }

        public AstToSqlConverter(MtTypeDefinition mtType, IMtObjectConverter converter, string tableName = null)
        {
            _tableName = tableName;
            _mtType = mtType;
            _converter = converter;
        }

        private bool ResolveNullCheck(BinaryOpNode node, out int index)
        {
            LoadConstNode constNode;
            LoadFieldNode fieldNode;

            if (node.Left is LoadFieldNode && node.Right is LoadConstNode)
            {
                constNode = (LoadConstNode) node.Right;
                fieldNode = (LoadFieldNode) node.Left;
            }
            else if (node.Right is LoadFieldNode && node.Left is LoadConstNode)
            {
                fieldNode = (LoadFieldNode) node.Right;
                constNode = (LoadConstNode) node.Left;
            }
            else
            {
                index = 0;
                return false;
            }

            if (constNode.Value != null)
            {
                index = 0;
                return false;
            }

            index = fieldNode.PropertyIndex;

            return true;
        }

        private bool CheckBooleanConstSpecialCase(AstNode node)
        {
            if (node is LoadConstNode)
            {
                var val = ((LoadConstNode)node).Value;

                if (val == null)
                {
                    _result.Append("0 = 1");
                }

                if (val is bool)
                {
                    if ((bool) val)
                    {
                        _result.Append("1 = 1");
                    }
                    else
                    {
                        _result.Append("0 = 1");
                    }
                }
                else
                {
                    _result.Append("0 = 1");
                }

                return true;
            }

            return false;
        }

        public void Convert(AstNode node)
        {
            if (CheckBooleanConstSpecialCase(node))
            {
                return;
            }

            ConvertInt(node);
        }

        private void ConvertInt(AstNode node)
        {
            if (node is BinaryOpNode)
            {
                var binNode = (BinaryOpNode) node;

               

                int propIndex;

                if (ResolveNullCheck(binNode, out propIndex))
                {
                    WritePropAccess(propIndex);

                    switch (binNode.Op)
                    {
                        case MtBinaryOp.Eq:
                            _result.Append(" is null");
                            break;
                        case MtBinaryOp.Neq:
                            _result.Append(" is not null");
                            break;
                    }

                    return;
                }

                _result.Append('(');
                ConvertInt(binNode.Left);

                switch (binNode.Op)
                {
                    case MtBinaryOp.Gt:
                        _result.Append(" > ");
                        break;
                    case MtBinaryOp.Lt:
                        _result.Append(" < ");
                        break;
                    case MtBinaryOp.Eq:
                        _result.Append(" = ");
                        break;
                    case MtBinaryOp.Neq:
                        _result.Append(" != ");
                        break;
                    case MtBinaryOp.Gte:
                        _result.Append(" >= ");
                        break;
                    case MtBinaryOp.Lte:
                        _result.Append(" <= ");
                        break;
                    case MtBinaryOp.And:
                        _result.Append(" AND ");
                        break;
                    case MtBinaryOp.Or:
                        _result.Append(" OR ");
                        break;
                    case MtBinaryOp.Plus:
                        _result.Append(" + ");
                        break;
                    case MtBinaryOp.Minus:
                        _result.Append(" - ");
                        break;
                    case MtBinaryOp.Mul:
                        _result.Append(" * ");
                        break;
                    case MtBinaryOp.Div:
                        _result.Append(" / ");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ConvertInt(binNode.Right);

                _result.Append(')');
            }
            else if (node is LoadConstNode)
            {
                _result.Append("@param" + Constants.Count);

                var val = ((LoadConstNode)node).Value;

                if (val == null)
                {
                    val = DBNull.Value;
                }
                else if (val is DateTime)
                {
                    val = new DateTimeOffset((DateTime)val);
                }
               
                Constants.Add(val);
            }
            else if (node is LoadFieldNode)
            {
                var type = _mtType.Properties[((LoadFieldNode) node).PropertyIndex].MtPropertyType.ClrType;
                WriteConversion(type, ((LoadFieldNode)node).PropertyIndex);
            }
        }

        private void WritePropAccess(int propIndex)
        {
            if (_tableName != null)
            {
                _result.Append(_tableName);
                _result.Append(".");
            }
            _result.Append("Value" + (propIndex+1));
        }

        private void WriteConversionExpression(string sqlType, int propOffset, int format)
        {
            _result.Append("CONVERT(");
            _result.Append(sqlType);
            _result.Append(',');
            WritePropAccess(propOffset);
            if (format != 0)
            {
                _result.Append(',');
                _result.Append(format);
            }

            _result.Append(')');
        }

        private void WriteConversion(Type type, int propOffset)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
            {
                WriteConversion(type.GetGenericArguments()[0], propOffset);
                return;
            }

            if (_converter.IsPrimitiveType(type))
            {
                if (type == typeof (string))
                {
                    WritePropAccess(propOffset);
                    return;
                }
               
                string sqlTypeName;

                if (!ClrToSqlMapping.TryGetValue(type, out sqlTypeName))
                {
                    throw new NotSupportedException(string.Format("Type {0} is not supported", type.FullName));
                }

                WriteConversionExpression(sqlTypeName, propOffset, type == typeof(DateTime) ? 127: 0);

            }
            else
            {
                WritePropAccess(propOffset);
            }
        }
    }
}
