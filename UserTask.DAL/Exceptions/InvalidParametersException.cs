using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTask.DAL.Exceptions
{
    public class InvalidParametersException : Exception
    {
        public InvalidParametersException() { }

        public InvalidParametersException(string message)
            : base(message) { }

        public InvalidParametersException(string message, Exception inner)
            : base(message, inner) { }
    }
}
