namespace OnStackGenerator.General
{
    public class OnStackTypeInfo
    {
        public string Namespace { get; set; } = null!;

        public string TypeName { get; set; } = null!;

        public Field[] Fields { get; set; } = null!;

        public bool IsNullableEnabled { get; set; }

        public TypeAccessModifier AccessModifier { get; set; }
    }

    public enum TypeAccessModifier
    {
        Public,
        Internal,
    }
}
