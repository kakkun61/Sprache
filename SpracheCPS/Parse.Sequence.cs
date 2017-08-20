namespace Sprache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    partial class Parse
    {
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="parser"></param>
        ///// <param name="delimiter"></param>
        ///// <typeparam name="T"></typeparam>
        ///// <typeparam name="U"></typeparam>
        ///// <returns></returns>
        ///// <exception cref="ArgumentNullException"></exception>
        //public static Parser<IEnumerable<T>> DelimitedBy<T, U>(this Parser<T> parser, Parser<U> delimiter)
        //{
        //    if (parser == null) throw new ArgumentNullException(nameof(parser));
        //    if (delimiter == null) throw new ArgumentNullException(nameof(delimiter));

        //    return from head in parser.Once()
        //           from tail in
        //               (from separator in delimiter
        //                from item in parser
        //                select item).Many()
        //           select head.Concat(tail);
        //}

        ///// <summary>
        ///// Fails on the first itemParser failure, if it reads at least one character.
        ///// </summary>
        ///// <param name="itemParser"></param>
        ///// <param name="delimiter"></param>
        ///// <typeparam name="T"></typeparam>
        ///// <typeparam name="U"></typeparam>
        ///// <returns></returns>
        ///// <exception cref="ArgumentNullException"></exception>
        //public static Parser<IEnumerable<T>> XDelimitedBy<T, U>(this Parser<T> itemParser, Parser<U> delimiter)
        //{
        //    if (itemParser == null) throw new ArgumentNullException(nameof(itemParser));
        //    if (delimiter == null) throw new ArgumentNullException(nameof(delimiter));

        //    return from head in itemParser.Once()
        //           from tail in
        //               (from separator in delimiter
        //                from item in itemParser
        //                select item).XMany()
        //           select head.Concat(tail);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Parser<IEnumerable<T>> Repeat<T>(this Parser<T> parser, int count)
        {
            return Repeat(parser, count, count);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="minimumCount"></param>
        /// <param name="maximumCount"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Parser<IEnumerable<T>> Repeat<T>(this Parser<T> parser, int minimumCount, int maximumCount)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            return (input, onSuccess, onFailure) =>
            {
                var result = new List<T>();
                Func<IInput, int, IResult<object>> loop = null;
                loop = (remainder, count) =>
                {
                    if (count >= maximumCount)
                        return onSuccess(result, remainder);
                    return parser(
                        remainder,
                        (value, remainder_) =>
                        {
                            result.Add(value);
                            return loop(remainder_, count + 1);
                        },
                        (remainder_, message, expectations) =>
                        {
                            if (count < minimumCount)
                            {
                                var what = remainder_.AtEnd
                                    ? "end of input"
                                    : remainder_.Current.ToString();

                                var msg = $"Unexpected '{what}'";
                                var exp = $"'{StringExtensions.Join(", ", expectations)}' between {minimumCount} and {maximumCount} times, but found {count}";

                                return onFailure(input, msg, new[] { exp });
                            }
                            return loop(remainder_, count + 1);
                        });
                };
                return loop(input, 0);
            };
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="parser"></param>
        ///// <param name="open"></param>
        ///// <param name="close"></param>
        ///// <typeparam name="T"></typeparam>
        ///// <typeparam name="U"></typeparam>
        ///// <typeparam name="V"></typeparam>
        ///// <returns></returns>
        ///// <exception cref="ArgumentNullException"></exception>
        //public static Parser<T> Contained<T, U, V>(this Parser<T> parser, Parser<U> open, Parser<V> close)
        //{
        //    if (parser == null) throw new ArgumentNullException(nameof(parser));
        //    if (open == null) throw new ArgumentNullException(nameof(open));
        //    if (close == null) throw new ArgumentNullException(nameof(close));

        //    return from o in open
        //           from item in parser
        //           from c in close
        //           select item;
        //}
    }
}
