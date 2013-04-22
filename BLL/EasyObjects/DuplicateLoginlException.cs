using System;
using System.Collections.Generic;
using System.Text;

namespace BWA.bigWebDesk.BLL
{
    public class DuplicateLoginException : Exception
    {
        public DuplicateLoginException()
        {
        }

        public DuplicateLoginException(string message): base(message)
        {
        }

        public DuplicateLoginException(string message, Exception inner)      : base(message, inner)
        {
        }
    }
}
