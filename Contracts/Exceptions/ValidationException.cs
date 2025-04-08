using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace OnlineStore.Contracts.Exceptions
{
    public class ValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException() : base("Ошибка валидации")
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(IEnumerable<ValidationFailure> failures) : this()
        {
            var failureGroups = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage);

            foreach (var failureGroup in failureGroups)
            {
                Errors.Add(failureGroup.Key, failureGroup.ToArray());
            }
        }
    }
}
