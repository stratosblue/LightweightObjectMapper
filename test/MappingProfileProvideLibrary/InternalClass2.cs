namespace MappingProfileProvideLibrary;

public class InternalClass2
{
    public int Property1 { get; }

    public double Property2 { get; }

    public string Property3 { get; set; }

    public bool Property4 { get; set; }

    public int? Property5 { get; set; }

    public int? Property6 { get; set; }

    internal InternalClass2(int property1, double property2)
    {
        Property1 = property1;
        Property2 = property2;
    }
}
