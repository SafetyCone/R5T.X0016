using System;


namespace R5T.X0016
{
    public static class Instances
    {
        public static B0006.ISyntaxGenerator SyntaxGenerator { get; } = B0006.SyntaxGenerator.Instance;
        public static B0006.ISyntaxOperator SyntaxOperator { get; } = B0006.SyntaxOperator.Instance;
    }
}
