using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LDBot
{
    [Serializable]
    public class ScriptException : Exception
    {
        public ScriptException()
        {
        }

        public ScriptException(string message) : base(message)
        {
        }

        public ScriptException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ScriptException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
