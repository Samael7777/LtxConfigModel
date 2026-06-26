using System.Text;

namespace LtxConfigModel;

/// <summary>
/// String extension helpers used by the parser and serializer to convert tabs/spaces and
/// to measure leading/trailing padding.
/// </summary>
public static class StringExtensions
{
    extension(string str)
    {
        /// <summary>Convert tab characters to spaces using the provided tab size.</summary>
        public string TabsToSpaces(int tabSize = 8)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(tabSize, 0);

            if (!str.Contains('\t'))
                return str;

            var sb = new StringBuilder(str.Length + 8);
            var column = 0;

            foreach (var c in str)
            {
                if (c == '\t')
                {
                    var spaces = tabSize - (column % tabSize);
                    sb.Append(' ', spaces);
                    column += spaces;
                }
                else
                {
                    sb.Append(c);
                    column++;
                }
            }

            return sb.ToString();
        }

        /// <summary>Return number of consecutive space characters at the start of the string.</summary>
        public int LeftPadding()
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            var pad = 0;
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] != ' ') 
                    return pad;

                pad++;
            }

            return str.Length;
        }

        /// <summary>Return number of consecutive space characters at the end of the string.</summary>
        public int RightPadding()
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            var pad = 0;
            for (var i = str.Length - 1; i >= 0; i--)
            {
                if (str[i] != ' ') 
                    return pad;

                pad++;
            }

            return str.Length;
        }

        /// <summary>
        /// Replace groups of spaces with tabs where it does not change the visual column positions of
        /// subsequent characters. A tab is inserted only when a run of spaces starts at a column that is
        /// aligned to a tab stop and the run reaches or crosses the next tab stop; otherwise spaces are kept.
        /// </summary>
        public string SpacesToTabs(int tabSize = 4)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(tabSize, 0);

            if (!str.Contains(' '))
                return str;

            var sb = new StringBuilder(str.Length);
            var column = 0;
            var i = 0;

            while (i < str.Length)
            {
                var c = str[i];

                if (c != ' ')
                {
                    sb.Append(c);
                    column++;
                    i++;
                    continue;
                }

                // Found the start of a run of spaces — compute its length
                var spaceStart = i;
                var j = i;
                while (j < str.Length && str[j] == ' ')
                    j++;
                var spaceCount = j - spaceStart;

                // Replace spaces with tabs in chunks that exactly fit a tab stop boundary
                var remaining = spaceCount;
                while (remaining > 0)
                {
                    var distanceToNextStop = tabSize - (column % tabSize);

                    if (distanceToNextStop <= remaining)
                    {
                        // The run covers the next tab stop — insert a tab
                        sb.Append('\t');
                        column += distanceToNextStop;
                        remaining -= distanceToNextStop;
                    }
                    else
                    {
                        // The remainder does not reach the tab stop — keep spaces
                        sb.Append(' ', remaining);
                        column += remaining;
                        remaining = 0;
                    }
                }

                i = j;
            }

            return sb.ToString();
        }

        /// <summary>Case-insensitive equality comparison.</summary>
        public bool EqualIgnoreCase(string other) => 
            string.Equals(str, other, StringComparison.OrdinalIgnoreCase);
    }
}
