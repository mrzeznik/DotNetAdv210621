using System;
using System.Collections.Generic;
using System.Text;

namespace LevelUpCSharp
{
    internal static class ResultExtensions
    {
        public static Result<T> ToSuccess<T>(this T source)
        {
            return Result<T>.Success(source);
        }
    }
}
