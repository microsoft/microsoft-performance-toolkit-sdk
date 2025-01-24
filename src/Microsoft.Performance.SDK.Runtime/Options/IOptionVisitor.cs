using Microsoft.Performance.SDK.Options;

namespace Microsoft.Performance.SDK.Runtime.Options;

public interface IOptionVisitor
{
    void Visit(BooleanOption option);

    void Visit(FieldOption option);

    void Visit(FieldArrayOption option);
}