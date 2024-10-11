namespace Microsoft.Performance.SDK.Runtime.ColumnVariants
{
    public interface IColumnVariantsVisitor
    {
        void Visit(ToggleableColumnVariant toggleableColumnVariant);

        void Visit(ModesToggleColumnVariant modesToggle);

        void Visit(ModesColumnVariant modesColumnVariant);

        void Visit(ModeColumnVariant modeColumnVariant);

        void Visit(IntermediateModeColumnVariant intermediateModeColumnVariant);
    }
}