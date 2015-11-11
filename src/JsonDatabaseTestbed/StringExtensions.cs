﻿using System;
using System.Linq;

namespace JsonDatabaseTestbed
{
    public static partial class StringExtensions
    {
        public static bool IsRoughly(this string input, params string[] matches)
        {
            return matches.Any(match => input.Trim().Equals(match.Trim(), StringComparison.InvariantCultureIgnoreCase));
        }
    }
}