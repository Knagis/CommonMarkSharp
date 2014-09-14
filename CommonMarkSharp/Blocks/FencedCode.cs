﻿using CommonMarkSharp.Inlines;
using CommonMarkSharp.InlineParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonMarkSharp.Blocks
{
    public class FencedCode : LeafBlock
    {
        private static Regex _spaceRe = new Regex(" +", RegexOptions.Compiled);

        public string Info { get; private set; }
        public int Length { get; set; }
        public char Char { get; set; }
        public int Offset { get; set; }
        public IEnumerable<string> InfoWords { get { return _spaceRe.Split(Info); } }

        public override bool AcceptsLines { get { return true; } }

        public override void Close(ParserContext context, int lineNumber)
        {
            base.Close(context, lineNumber);
            Info = string.Join("", context.Parsers.EscapedStringParser.ParseMany(context, Strings.First()).Cast<InlineString>().Select(s => s.Value)).Trim();
            Contents = Strings.Count() > 1 ? string.Join("\n", Strings.Skip(1)) + "\n" : "";
        }

        public override bool MatchNextLine(Subject subject)
        {
            // skip optional spaces of fence offset
            subject.AdvanceWhile(c => c == ' ', Offset);
            return true;
        }
    }
}
