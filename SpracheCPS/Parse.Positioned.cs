
namespace Sprache
{
    partial class Parse
    {
        /// <summary>
        /// Construct a parser that will set the position to the position-aware
        /// T on succsessful match.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static Parser<T> Positioned<T>(this Parser<T> parser) where T : IPositionAware<T>
        {
            return (input, onSuccess, onFailure) =>
                parser(
                    input,
                    (value, remainder) => onSuccess(value.SetPos(Position.FromInput(input), remainder.Position - input.Position), remainder),
                    onFailure);
        }
    }
}
