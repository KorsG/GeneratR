using System;

namespace GeneratR.DotNet
{
    [Flags]
    public enum DotNetModifierKeyword
    {
        @None = 0,
        @Public = 2,
        @Private = 4,
        @Protected = 8,
        @Partial = 16,
        @Internal = 32,
        @Virtual = 64,
        @Override = 128,
        @Static = 256,
        @Abstract = 512,
        @Sealed = 1024,
        @ReadOnly = 2048,
        @Const = 4096,
    }
}
