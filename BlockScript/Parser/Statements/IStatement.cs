﻿using BlockScript.Reader;

namespace BlockScript.Parser.Statements;

public interface IStatement
{
    Position Position { get; }
}