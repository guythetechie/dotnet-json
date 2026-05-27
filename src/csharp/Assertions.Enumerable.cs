using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using TUnit.Assertions.Conditions;
using TUnit.Assertions.Core;
using TUnit.Assertions.Enums;

namespace common;

public static class AsyncEnumerableAssertionExtensions
{
    extension<T>(IAssertionSource<IAsyncEnumerable<T>> source)
    {
        public IsEquivalentToAssertion<IEnumerable<T>, T> IsEquivalentTo(IEnumerable<T> expected, CollectionOrdering ordering = CollectionOrdering.Any, CancellationToken cancellationToken = default, [CallerArgumentExpression(nameof(expected))] string? expectedExpression = null, [CallerArgumentExpression(nameof(ordering))] string? orderingExpression = null)
        {
            var context = source.Context;

            context.ExpressionBuilder.Append(".IsEquivalentTo(");
            context.ExpressionBuilder.Append(expectedExpression);

            if (orderingExpression is not null)
            {
                context.ExpressionBuilder.Append(", ");
                context.ExpressionBuilder.Append(orderingExpression);
            }

            context.ExpressionBuilder.Append(')');

            var mappedContext = context.Map<IEnumerable<T>>(async actual =>
                actual is null
                    ? null
                    : await actual.ToArrayAsync(cancellationToken));

            return new(mappedContext, expected, ordering);
        }
    }
}