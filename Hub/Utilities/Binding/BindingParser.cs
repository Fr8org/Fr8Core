using System;
using System.Collections.Generic;

namespace Hub.Utilities.Binding
{
    public class BindingParser
    {
        /**********************************************************************************/

        private int _pos = -1;
        private readonly string _binding;

        public readonly List<string> SourcePropertyPath = new List<string>();
        public readonly List<string> TargetPropertyPath = new List<string>();

        /**********************************************************************************/

        public BindingDirection Direction
        {
            get;
            private set;
        }

        /**********************************************************************************/

        private char Current
        {
            get
            {
                if (_pos == _binding.Length)
                {
                    return char.MinValue;
                }

                return _binding[_pos];
            }
        }

        /**********************************************************************************/
        
        public BindingParser(string binding)
        {
            _binding = binding;
        }

        /**********************************************************************************/

        private bool Next()
        {
            _pos++;

            if (_pos > _binding.Length)
            {
                return false;
            }

            return true;
        }

        /**********************************************************************************/
        
        public void Parse()
        {
            CaptureSource();
        }

        /**********************************************************************************/

        private void Skip()
        {
            while (Next() && char.IsWhiteSpace(Current)) ;
        }

        /**********************************************************************************/

        private void CaptureSource()
        {
            Skip();
            CapturePath(SourcePropertyPath);
            Skip();
            CaptureDirection();
            Skip();
            CapturePath(TargetPropertyPath);
        }

        /**********************************************************************************/

        private void CaptureDirection()
        {
            BindingDirection direction = 0;

            do
            {
                if (Current == '>')
                {
                    direction |= BindingDirection.ToTarget;
                }
                else if (Current == '<')
                {
                    direction |= BindingDirection.ToSource;
                }
                else
                {
                    break;
                }
            } while (Next());

            if (direction == 0)
            {
                throw new Exception("Invalid binding syntax: incorrect direction specifier");
            }

            Direction = direction;
        }

        /**********************************************************************************/

        private void CapturePath(List<string> path)
        {
            int varStrart = -1;

            do
            {
                if (varStrart == -1)
                {
                    varStrart = _pos;
                }

                if (Current == '.')
                {
                    if (varStrart == _pos)
                    {
                        throw new Exception("Invalid binding syntax: invalid property path");
                    }
                    path.Add(_binding.Substring(varStrart, (_pos - varStrart)));
                    varStrart = -1;
                    continue;
                }

                if (!char.IsNumber(Current) && !char.IsLetter(Current) && Current != '_')
                {
                    if (varStrart == _pos)
                    {
                        throw new Exception("Invalid binding syntax: invalid property path");
                    }
                    path.Add(_binding.Substring(varStrart, (_pos - varStrart)));
                    break;
                }
            } while (Next());
        }

        /**********************************************************************************/
    }
}