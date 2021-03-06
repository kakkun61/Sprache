﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sprache
{
    /// <summary>
    /// Parsers and combinators.
    /// </summary>
    public static partial class Parse
    {
        /// <summary>
        /// TryParse a single character matching 'predicate'
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static Parser<char> Char(Predicate<char> predicate, string description)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (description == null) throw new ArgumentNullException(nameof(description));

            return (i, onSuccess, onFailure) =>
            {
                if (!i.AtEnd)
                {
                    if (predicate(i.Current))
                        return onSuccess(i.Current, i.Advance());
                    return onFailure(i,
                        $"unexpected '{i.Current}'",
                        new[] { description });
                }
                return onFailure(i,
                    "Unexpected end of input reached",
                    new[] { description });
            };
        }

        /// <summary>
        /// Parse a single character except those matching <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">Characters not to match.</param>
        /// <param name="description">Description of characters that don't match.</param>
        /// <returns>A parser for characters except those matching <paramref name="predicate"/>.</returns>
        public static Parser<char> CharExcept(Predicate<char> predicate, string description)
        {
            return Char(c => !predicate(c), "any character except " + description);
        }

        /// <summary>
        /// Parse a single character c.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Parser<char> Char(char c)
        {
            return Char(ch => c == ch, char.ToString(c));
        }


        /// <summary>
        /// Parse a single character of any in c
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Parser<char> Chars(params char[] c)
        {
            return Char(c.Contains, StringExtensions.Join("|", c));
        }

        /// <summary>
        /// Parse a single character of any in c
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Parser<char> Chars(string c)
        {
            return Char(c.ToEnumerable().Contains, StringExtensions.Join("|", c.ToEnumerable()));
        }


        /// <summary>
        /// Parse a single character except c.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Parser<char> CharExcept(char c)
        {
            return CharExcept(ch => c == ch, char.ToString(c));
        }

        /// <summary>
        /// Parses a single character except for those in the given parameters
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Parser<char> CharExcept(IEnumerable<char> c)
        {
            var chars = c as char[] ?? c.ToArray();
            return CharExcept(chars.Contains, StringExtensions.Join("|", chars));
        }

        /// <summary>
        /// Parses a single character except for those in c
        /// </summary>  
        /// <param name="c"></param>
        /// <returns></returns> 
        public static Parser<char> CharExcept(string c)
        {
            return CharExcept(c.ToEnumerable().Contains, StringExtensions.Join("|", c.ToEnumerable()));
        }

        /// <summary>
        /// Parse a single character in a case-insensitive fashion.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Parser<char> IgnoreCase(char c)
        {
            return Char(ch => char.ToLower(c) == char.ToLower(ch), char.ToString(c));
        }

        /// <summary>
        /// Parse a string in a case-insensitive fashion.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Parser<IEnumerable<char>> IgnoreCase(string s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            return s
                .ToEnumerable()
                .Select(IgnoreCase)
                .Aggregate(Return(Enumerable.Empty<char>()),
                    (a, p) => a.Concat(p.Once()))
                .Named(s);
        }

        /// <summary>
        /// Parse any character.
        /// </summary>
        public static readonly Parser<char> AnyChar = Char(c => true, "any character");

        /// <summary>
        /// Parse a whitespace.
        /// </summary>
        public static readonly Parser<char> WhiteSpace = Char(char.IsWhiteSpace, "whitespace");

        /// <summary>
        /// Parse a digit.
        /// </summary>
        public static readonly Parser<char> Digit = Char(char.IsDigit, "digit");

        /// <summary>
        /// Parse a letter.
        /// </summary>
        public static readonly Parser<char> Letter = Char(char.IsLetter, "letter");

        /// <summary>
        /// Parse a letter or digit.
        /// </summary>
        public static readonly Parser<char> LetterOrDigit = Char(char.IsLetterOrDigit, "letter or digit");

        /// <summary>
        /// Parse a lowercase letter.
        /// </summary>
        public static readonly Parser<char> Lower = Char(char.IsLower, "lowercase letter");

        /// <summary>
        /// Parse an uppercase letter.
        /// </summary>
        public static readonly Parser<char> Upper = Char(char.IsUpper, "uppercase letter");

        /// <summary>
        /// Parse a numeric character.
        /// </summary>
        public static readonly Parser<char> Numeric = Char(char.IsNumber, "numeric character");

        /// <summary>
        /// Parse a string of characters.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Parser<IEnumerable<char>> String(string s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            return s
                .ToEnumerable()
                .Select(Char)
                .Aggregate(Return(Enumerable.Empty<char>()),
                    (a, p) => a.Concat(p.Once()))
                .Named(s);
        }

        /// <summary>
        /// Constructs a parser that will fail if the given parser succeeds,
        /// and will succeed if the given parser fails. In any case, it won't
        /// consume any input. It's like a negative look-ahead in regex.
        /// </summary>
        /// <typeparam name="T">The result type of the given parser</typeparam>
        /// <param name="parser">The parser to wrap</param>
        /// <returns>A parser that is the opposite of the given parser.</returns>
        public static Parser<object> Not<T>(this Parser<T> parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            return (input, onSuccess, onFailure) =>
                parser(
                    input,
                    (value, remainder) => onFailure(input, $"{value} was not expected", new string[0]),
                    (remainder, message, expectations) => onSuccess(null, input));
        }

        /// <summary>
        /// Parse first, and if successful, then parse second.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Parser<U> Then<T, U>(this Parser<T> first, Func<T, Parser<U>> second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            return (i, onSuccess, onFaulure) =>
            {
                OnSuccess<T> onSucc = ((value, remainder) => second(value)(remainder, onSuccess, onFaulure));
                return first(i, onSucc, onFaulure);
            };
        }

        /// <summary>
        /// Parse a stream of elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        /// <remarks>Implemented imperatively to decrease stack usage.</remarks>
        public static Parser<IEnumerable<T>> Many<T>(this Parser<T> parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            // equivalent to
            //     return parser.AtLeastOnce().Or(Return(Enumerable.Empty<T>()));
            return (i, onSuccess, onFaulure) =>
            {
                var remainder = i;
                var result = new List<T>();
                var r = parser(i, Result.Success<T>, Result.Failure);

                while (r.WasSuccessful)
                {
                    if (remainder.Equals(r.Remainder))
                        break;

                    result.Add((T)r.Value);
                    remainder = r.Remainder;
                    r = parser(remainder, Result.Success<T>, Result.Failure);
                }

                return onSuccess(result, remainder);
            };
        }

        /// <summary>
        /// Parse a stream of elements, failing if any element is only partially parsed.
        /// </summary>
        /// <typeparam name="T">The type of element to parse.</typeparam>
        /// <param name="parser">A parser that matches a single element.</param>
        /// <returns>A <see cref="Parser{T}"/> that matches the sequence.</returns>
        /// <remarks>
        /// <para>
        /// Using <seealso cref="XMany{T}(Parser{T})"/> may be preferable to <seealso cref="Many{T}(Parser{T})"/>
        /// where the first character of each match identified by <paramref name="parser"/>
        /// is sufficient to determine whether the entire match should succeed. The X*
        /// methods typically give more helpful errors and are easier to debug than their
        /// unqualified counterparts.
        /// </para>
        /// </remarks>
        /// <seealso cref="XOr"/>
        public static Parser<IEnumerable<T>> XMany<T>(this Parser<T> parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            return parser.Many().Then(m => parser.Once().XOr(Return(m)));
        }

        /// <summary>
        /// TryParse a stream of elements with at least one item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Parser<IEnumerable<T>> AtLeastOnce<T>(this Parser<T> parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            return parser.Once().Then(t1 => parser.Many().Select(ts => t1.Concat(ts)));
        }

        /// <summary>
        /// TryParse a stream of elements with at least one item. Except the first
        /// item, all other items will be matched with the <code>XMany</code> operator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Parser<IEnumerable<T>> XAtLeastOnce<T>(this Parser<T> parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            return parser.Once().Then(t1 => parser.XMany().Select(ts => t1.Concat(ts)));
        }

        /// <summary>
        /// Parse end-of-input.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Parser<T> End<T>(this Parser<T> parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            return (input, onSuccess, onFailure) =>
            {
                return parser(
                    input,
                    (value, remainder) =>
                    {
                        if (remainder.AtEnd)
                            return onSuccess(value, remainder);
                        return onFailure(remainder, string.Format("unexpected '{0}'", remainder.Current), new[] { "end of input" });
                    },
                    onFailure);
            };
        }

        /// <summary>
        /// Take the result of parsing, and project it onto a different domain.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="parser"></param>
        /// <param name="convert"></param>
        /// <returns></returns>
        public static Parser<U> Select<T, U>(this Parser<T> parser, Func<T, U> convert)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (convert == null) throw new ArgumentNullException(nameof(convert));

            return parser.Then(t => Return(convert(t)));
        }

        /// <summary>
        /// Parse the token, embedded in any amount of whitespace characters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Parser<T> Token<T>(this Parser<T> parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            return from leading in WhiteSpace.Many()
                   from item in parser
                   from trailing in WhiteSpace.Many()
                   select item;
        }

        /// <summary>
        /// Refer to another parser indirectly. This allows circular compile-time dependency between parsers.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static Parser<T> Ref<T>(Func<Parser<T>> reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));

            Parser<T> parser = null;

            return (input, onSuccess, onFailure) =>
            {
                if (parser == null)
                    parser = reference();

                if (input.Memos.ContainsKey(parser))
                    throw new ParseException(input.Memos[parser].ToString());

                input.Memos[parser] = Result.Failure<T>(
                    input,
                    "Left recursion in the grammar.",
                    new string[0]);
                return parser(
                    input,
                    (value, remainder) =>
                    {
                        input.Memos[parser] = Result.Success(value, remainder);
                        return onSuccess(value, remainder);
                    },
                    (remainder, message, expectations) =>
                    {
                        input.Memos[parser] = Result.Failure<T>(remainder, message, expectations);
                        return onFailure(remainder, message, expectations);
                    });
            };
        }

        /// <summary>
        /// Convert a stream of characters to a string.
        /// </summary>
        /// <param name="characters"></param>
        /// <returns></returns>
        public static Parser<string> Text(this Parser<IEnumerable<char>> characters)
        {
            return characters.Select(chs => new string(chs.ToArray()));
        }

        /// <summary>
        /// Parse first, if it succeeds, return first, otherwise try second.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Parser<T> Or<T>(this Parser<T> first, Parser<T> second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            return (input, onSuccess, onFailure) => {
                OnFailure onFailure_ = (firstRemainder, firstMessage, firstExpectations) =>
                        second(input, onSuccess, DetermineBestError(firstRemainder, firstMessage, firstExpectations, onFailure));
                return first(input, onSuccess, onFailure_);
            };
        }

        /// <summary>
        /// Names part of the grammar for help with error messages.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Parser<T> Named<T>(this Parser<T> parser, string name)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (name == null) throw new ArgumentNullException(nameof(name));

            return (input, onSuccess, onFailure) =>
            {
                return parser(input, onSuccess, (remainder, message, expectations) =>
                {
                    return onFailure(
                        remainder,
                        message,
                        remainder.Equals(input)?
                            new[] { name }:
                            expectations);
                });
            };
        }

        /// <summary>
        /// Parse first, if it succeeds, return first, otherwise try second.
        /// Assumes that the first parsed character will determine the parser chosen (see Try).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Parser<T> XOr<T>(this Parser<T> first, Parser<T> second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            return (input, onSuccess, onFailure) =>
            {
                OnSuccess<T> onSuccess_ = (value, remainder) =>
                {
                    if (remainder.Equals(input))
                        return second(input, onSuccess, (_remainder, _message, _expectations) => onSuccess(value, remainder));

                    return onSuccess(value, remainder);
                };
                OnFailure onFailure_ = (remainder, message, expectations) =>
                {
                    if (!remainder.Equals(input))
                        return onFailure(remainder, message, expectations);

                    return second(input, onSuccess, DetermineBestError(remainder, message, expectations, onFailure));
                };
                return first(input, onSuccess_, onFailure_);
            };
        }

        // Examines two results presumably obtained at an "Or" junction; returns the result with
        // the most information, or if they apply at the same input position, a union of the results.
        static OnFailure DetermineBestError(IInput firstRemainder, string firstMessage, IEnumerable<string> firstExpectations, OnFailure onFailure)
        {
            return (secondRemainder, secondMessage, secondExpectation) =>
            {
                if (firstRemainder.Position > secondRemainder.Position)
                    return onFailure(secondRemainder, secondMessage, secondExpectation);

                if (secondRemainder.Position == firstRemainder.Position)
                    return onFailure(firstRemainder, firstMessage, firstExpectations.Union(secondExpectation));

                return onFailure(firstRemainder, firstMessage, firstExpectations);
            };
        }

        /// <summary>
        /// Parse a stream of elements containing only one item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Parser<IEnumerable<T>> Once<T>(this Parser<T> parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            return parser.Select(r => (IEnumerable<T>)new[] { r });
        }

        /// <summary>
        /// Concatenate two streams of elements.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static Parser<IEnumerable<T>> Concat<T>(this Parser<IEnumerable<T>> first, Parser<IEnumerable<T>> second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            return first.Then(f => second.Select(f.Concat));
        }

        /// <summary>
        /// Succeed immediately and return value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Parser<T> Return<T>(T value)
        {
            return (i, onSuccess, onFailure) => onSuccess(value, i);
        }

        /// <summary>
        /// Version of Return with simpler inline syntax.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="parser"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Parser<U> Return<T, U>(this Parser<T> parser, U value)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            return parser.Select(t => value);
        }

        /// <summary>
        /// Attempt parsing only if the <paramref name="except"/> parser fails.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="parser"></param>
        /// <param name="except"></param>
        /// <returns></returns>
        public static Parser<T> Except<T, U>(this Parser<T> parser, Parser<U> except)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (except == null) throw new ArgumentNullException(nameof(except));

            // Could be more like: except.Then(s => s.Fail("..")).XOr(parser)
            return (input, onSuccess, onFailure) =>
            {
                return except(
                    input,
                    (value, remainder) => onFailure(input, "Excepted parser succeeded.", new[] { "other than the excepted input" }),
                    (remainder, message, expectations) => parser(input, onSuccess, onFailure));
            };
        }

        /// <summary>
        /// Parse a sequence of items until a terminator is reached.
        /// Returns the sequence, discarding the terminator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="parser"></param>
        /// <param name="until"></param>
        /// <returns></returns>
        public static Parser<IEnumerable<T>> Until<T, U>(this Parser<T> parser, Parser<U> until)
        {
            return parser.Except(until).Many().Then(r => until.Return(r));
        }

        /// <summary>
        /// Succeed if the parsed value matches predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static Parser<T> Where<T>(this Parser<T> parser, Func<T, bool> predicate)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            return (input, onSeccess, onFailure) =>
            parser(
                input,
                (value, remainder) =>
                    predicate(value)?
                        onSeccess(value, remainder):
                        onFailure(input, string.Format("Unexpected {0}.", value), new string[0]),
                onFailure);
        }

        /// <summary>
        /// Monadic combinator Then, adapted for Linq comprehension syntax.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="parser"></param>
        /// <param name="selector"></param>
        /// <param name="projector"></param>
        /// <returns></returns>
        public static Parser<V> SelectMany<T, U, V>(
            this Parser<T> parser,
            Func<T, Parser<U>> selector,
            Func<T, U, V> projector)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (projector == null) throw new ArgumentNullException(nameof(projector));

            return parser.Then(t => selector(t).Select(u => projector(t, u)));
        }

        /// <summary>
        /// Chain a left-associative operator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOp"></typeparam>
        /// <param name="op"></param>
        /// <param name="operand"></param>
        /// <param name="apply"></param>
        /// <returns></returns>
        public static Parser<T> ChainOperator<T, TOp>(
            Parser<TOp> op,
            Parser<T> operand,
            Func<TOp, T, T, T> apply)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (operand == null) throw new ArgumentNullException(nameof(operand));
            if (apply == null) throw new ArgumentNullException(nameof(apply));
            return operand.Then(first => ChainOperatorRest(first, op, operand, apply, Or));
        }

        /// <summary>
        /// Chain a left-associative operator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOp"></typeparam>
        /// <param name="op"></param>
        /// <param name="operand"></param>
        /// <param name="apply"></param>
        /// <returns></returns>
        public static Parser<T> XChainOperator<T, TOp>(
            Parser<TOp> op,
            Parser<T> operand,
            Func<TOp, T, T, T> apply)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (operand == null) throw new ArgumentNullException(nameof(operand));
            if (apply == null) throw new ArgumentNullException(nameof(apply));
            return operand.Then(first => ChainOperatorRest(first, op, operand, apply, XOr));
        }

        static Parser<T> ChainOperatorRest<T, TOp>(
            T firstOperand,
            Parser<TOp> op,
            Parser<T> operand,
            Func<TOp, T, T, T> apply,
            Func<Parser<T>, Parser<T>, Parser<T>> or)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (operand == null) throw new ArgumentNullException(nameof(operand));
            if (apply == null) throw new ArgumentNullException(nameof(apply));
            return or(op.Then(opvalue =>
                          operand.Then(operandValue =>
                              ChainOperatorRest(apply(opvalue, firstOperand, operandValue), op, operand, apply, or))),
                      Return(firstOperand));
        }

        /// <summary>
        /// Chain a right-associative operator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOp"></typeparam>
        /// <param name="op"></param>
        /// <param name="operand"></param>
        /// <param name="apply"></param>
        /// <returns></returns>
        public static Parser<T> ChainRightOperator<T, TOp>(
            Parser<TOp> op,
            Parser<T> operand,
            Func<TOp, T, T, T> apply)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (operand == null) throw new ArgumentNullException(nameof(operand));
            if (apply == null) throw new ArgumentNullException(nameof(apply));
            return operand.Then(first => ChainRightOperatorRest(first, op, operand, apply, Or));
        }

        /// <summary>
        /// Chain a right-associative operator.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TOp"></typeparam>
        /// <param name="op"></param>
        /// <param name="operand"></param>
        /// <param name="apply"></param>
        /// <returns></returns>
        public static Parser<T> XChainRightOperator<T, TOp>(
            Parser<TOp> op,
            Parser<T> operand,
            Func<TOp, T, T, T> apply)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (operand == null) throw new ArgumentNullException(nameof(operand));
            if (apply == null) throw new ArgumentNullException(nameof(apply));
            return operand.Then(first => ChainRightOperatorRest(first, op, operand, apply, XOr));
        }

        static Parser<T> ChainRightOperatorRest<T, TOp>(
            T lastOperand,
            Parser<TOp> op,
            Parser<T> operand,
            Func<TOp, T, T, T> apply,
            Func<Parser<T>, Parser<T>, Parser<T>> or)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (operand == null) throw new ArgumentNullException(nameof(operand));
            if (apply == null) throw new ArgumentNullException(nameof(apply));
            return or(op.Then(opvalue =>
                        operand.Then(operandValue =>
                            ChainRightOperatorRest(operandValue, op, operand, apply, or)).Then(r =>
                                Return(apply(opvalue, lastOperand, r)))),
                      Return(lastOperand));
        }

        /// <summary>
        /// Parse a number.
        /// </summary>
        public static readonly Parser<string> Number = Numeric.AtLeastOnce().Text();

        static Parser<string> DecimalWithoutLeadingDigits(CultureInfo ci = null)
        {
            return from nothing in Return("")
                       // dummy so that CultureInfo.CurrentCulture is evaluated later
                   from dot in String((ci ?? CultureInfo.CurrentCulture).NumberFormat.NumberDecimalSeparator).Text()
                   from fraction in Number
                   select dot + fraction;
        }

        static Parser<string> DecimalWithLeadingDigits(CultureInfo ci = null)
        {
            return Number.Then(n => DecimalWithoutLeadingDigits(ci).XOr(Return("")).Select(f => n + f));
        }

        /// <summary>
        /// Parse a decimal number using the current culture's separator character.
        /// </summary>
        public static readonly Parser<string> Decimal = DecimalWithLeadingDigits().XOr(DecimalWithoutLeadingDigits());

        /// <summary>
        /// Parse a decimal number with separator '.'.
        /// </summary>
        public static readonly Parser<string> DecimalInvariant = DecimalWithLeadingDigits(CultureInfo.InvariantCulture)
                                                                     .XOr(DecimalWithoutLeadingDigits(CultureInfo.InvariantCulture));
    }
}
