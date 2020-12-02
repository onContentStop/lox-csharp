namespace Lox
{
    internal enum TokenType
    {
        LeftParenthesisToken,
        RightParenthesisToken,
        LeftBraceToken,
        RightBraceToken,
        CommaToken,
        DotToken,
        MinusToken,
        PlusToken,
        SemicolonToken,
        SlashToken,
        StarToken,

        BangToken,
        BangEqualsToken,
        EqualsToken,
        EqualsEqualsToken,
        GreaterToken,
        GreaterEqualsToken,
        LessToken,
        LessEqualsToken,

        IdentifierToken,
        StringToken,
        NumberToken,

        AndKeyword,
        ClassKeyword,
        ElseKeyword,
        FalseKeyword,
        ForKeyword,
        FunKeyword,
        IfKeyword,
        NilKeyword,
        OrKeyword,
        PrintKeyword,
        ReturnKeyword,
        SuperKeyword,
        ThisKeyword,
        TrueKeyword,
        VarKeyword,
        WhileKeyword,

        EndOfFileToken,
    }
}