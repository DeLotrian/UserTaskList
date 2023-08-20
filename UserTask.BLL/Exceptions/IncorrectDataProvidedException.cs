using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTask.BLL.Exceptions
{
    public class IncorrectDataProvidedException : Exception
    {
        public IncorrectDataProvidedException() { }

        public IncorrectDataProvidedException(string message)
            : base(message) { }

        public IncorrectDataProvidedException(string message, Exception inner)
            : base(message, inner) { }
    }
}
