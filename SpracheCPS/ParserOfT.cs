using System;
using System.Collections.Generic;

namespace Sprache
{
    /// <summary>
    /// Represents a parser.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="input">The input to parse.</param>
    /// <returns>The result of the parser.</returns>
    public delegate IResult<object> Parser<T>(IInput input, OnSuccess<T> onSuccess, OnFailure onFailure);

    public delegate IResult<object> OnSuccess<T>(T value, IInput remainder);

    public delegate IResult<object> OnFailure(IInput remainder, string message, IEnumerable<string> expectations);

    /// <summary>
    /// Contains some extension methods for <see cref="Parser&lt;T&gt;" />.
    /// </summary>
    public static class ParserExtensions
    {
        /// <summary>
        /// Tries to parse the input without throwing an exception.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="parser">The parser.</param>
        /// <param name="input">The input.</param>
        /// <returns>The result of the parser</returns>
        public static IResult<T> TryParse<T>(this Parser<T> parser, string input)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (input == null) throw new ArgumentNullException(nameof(input));

            return new Result<T>(parser(new Input(input), Result.Success<T>, Result.Failure<object>));
        }

        /// <summary>
        /// Parses the specified input string.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="parser">The parser.</param>
        /// <param name="input">The input.</param>
        /// <returns>The result of the parser.</returns>
        /// <exception cref="Sprache.ParseException">It contains the details of the parsing error.</exception>
        public static T Parse<T>(this Parser<T> parser, string input)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (input == null) throw new ArgumentNullException(nameof(input));

            var result = parser.TryParse(input);

            if (result.WasSuccessful)
                return result.Value;

            throw new ParseException(result.ToString());
        }
    }
}
