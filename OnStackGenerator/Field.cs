namespace OnStackGenerator.General
{
    public class Field
    {
        public FieldAccessModifier AccessModifier { get; set; }
        public bool IsNullable { get; set; }
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}