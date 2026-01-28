using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comercial.Data;

public class RepositoryException : Exception
{
    public RepositoryException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
