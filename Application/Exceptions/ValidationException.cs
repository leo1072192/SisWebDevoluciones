using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException() : base("SE HA PRODUCIDO UNO O MAS ERRORES DE VALIDACION") { 
            Errors = new List<string>();
        }
        public List<string> Errors { get;}
        public ValidationException(IEnumerable<ValidationFailure> failures) : this() { 
            foreach (var failure in failures)
            {

                Errors.Add(failure.ErrorMessage);
            }
        
        }
    }
}
