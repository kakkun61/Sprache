using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;
using Xunit;

namespace Sprache.Tests
{
    static class AssertParser
    {
        public static void SucceedsWithOne<T>(Parser<IEnumerable<T>> parser, string input, T expectedResult)
        {
            SucceedsWith(parser, input, t =>
            {
                Assert.Equal(1, t.Count());
                Assert.Equal(expectedResult, t.Single());
            });
        }

        public static void SucceedsWithMany<T>(Parser<IEnumerable<T>> parser, string input, IEnumerable<T> expectedResult)
        {
            SucceedsWith(parser, input, t => Assert.True(t.SequenceEqual(expectedResult)));
        }

        public static void SucceedsWithAll(Parser<IEnumerable<char>> parser, string input)
        {
            SucceedsWithMany(parser, input, input.ToCharArray());
        }

        public static void SucceedsWith<T>(Parser<T> parser, string input, Action<T> resultAssertion)
        {
            parser.TryParse(
                input,
                (value, remainder) =>
                {
                    resultAssertion(value);
                },
                (remainder, message, expectations) =>
                {
                    Assert.True(false, $"Parsing of \"input\" failed unexpectedly.");
                });
        }

        public static void Fails<T>(Parser<T> parser, string input)
        {
            FailsWith(parser, input, (ramainder, message, expectations) => { });
        }

        public static void FailsAt<T>(Parser<T> parser, string input, int position)
        {
            FailsWith(parser, input, (remainder, message, expectations) => { Assert.Equal(position, remainder.Position); });
        }

        public static void FailsWith<T>(Parser<T> parser, string input, OnFailure resultAssertion)
        {
            parser.TryParse(
                input,
                (value, remainder) =>
                {
                    Assert.True(false, $"Expected failure but succeeded with {value}.");
                },
                resultAssertion);
        }
    }
}
