using LightweightObjectMapper;

namespace MappingProfileProvideLibrary;

[MappingProfile]
public partial class InternalMappingProfile
    : IMappingPrepare<InternalClass1, InternalClass2>
    , IMappingPrepare<InternalClass2, InternalClass2>
    , ITypeMemberIgnoreMapping<InternalClass1>
    , ITypeMemberIgnoreMapping<InternalClass2>
{
    public object IgnoreMapping(InternalClass1 target)
    {
        return new
        {
            target.Property6
        };
    }

    public object IgnoreMapping(InternalClass2 target)
    {
        return new
        {
            target.Property6
        };
    }

    public InternalClass2 MappingPrepare(InternalClass1 source)
    {
        return new InternalClass2(source.Property1, source.Property2);
    }

    public InternalClass2 MappingPrepare(InternalClass2 source)
    {
        return new InternalClass2(source.Property1, source.Property2);
    }
}
