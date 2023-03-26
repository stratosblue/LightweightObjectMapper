using LightweightObjectMapper;
using MappingProfileProvideLibrary;

namespace MapperTestLibrary;

[MappingProfileInclude(typeof(InternalMappingProfile))]
[MappingProfileInclude(typeof(InternalMappingProfile), typeof(InternalMappingProfile))]
[MappingProfile]
internal partial class MappingProfileIncludeMapProfile1
{
}

internal class Extensions
{
    public Extensions()
    {
        new InternalClass1().MapTo<InternalClass2>();
        ((InternalClass2)null!).MapTo<InternalClass2>();
    }
}
