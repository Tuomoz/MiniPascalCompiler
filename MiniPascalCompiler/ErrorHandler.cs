using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniPascalCompiler
{
    public enum ErrorType { LexicalError, SyntaxError, SemanticError };

    public class ErrorHandler
    {
        private Queue<Error> Errors = new Queue<Error>();

        public bool HasErrors { get { return Errors.Count > 0; } }

        public ErrorHandler()
        {
        }

        public void AddError(string errorMessage, ErrorType errorType)
        {
            Errors.Enqueue(new Error(errorMessage, errorType));
        }

        public Error[] GetErrors()
        {
            return Errors.ToArray();
        }
    }

    public class Error
    {
        public readonly ErrorType ErrorType;
        public readonly string ErrorMessage;

        public Error(string errorMessage, ErrorType errorType)
        {
            ErrorMessage = errorMessage;
            ErrorType = errorType;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", ErrorType, ErrorMessage);
        }
    }
}
