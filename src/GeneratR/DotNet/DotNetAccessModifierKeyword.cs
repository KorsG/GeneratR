using System;

namespace GeneratR.DotNet
{
    [Flags]
    public enum DotNetAccessModifierKeyword
    {
        @None = 0,
        @Public = 2,
        @Private = 4,
        @Internal = 8,
        @Protected = 16,
    }
}
