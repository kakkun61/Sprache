using System;

namespace Sprache
{
    partial class Parse
    {
        /// <summary>
        /// Construct a parser that indicates the given parser
        /// is optional. The returned parser will succeed on
        /// any input no matter whether the given parser
        /// succeeds or not.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Parser<IOption<T>> Optional<T>(this Parser<T> parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            return (input, onSuccess, onFailure) =>
            {
                return parser(
                    input,
                    (value, remainder) => onSuccess(new Some<T>(value), remainder),
                    (remainder, message, expectations) => onSuccess(new None<T>(), input));
            };
        }
    }
}
