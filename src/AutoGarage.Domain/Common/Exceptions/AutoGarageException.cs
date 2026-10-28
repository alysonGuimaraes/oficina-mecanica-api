using System;
using System.Collections.Generic;
using System.Text;

namespace AutoGarage.Domain.Common.Exceptions
{
    public class AutoGarageException : SystemException
    {
        public AutoGarageException(string message) : base(message) { }
    }
}
