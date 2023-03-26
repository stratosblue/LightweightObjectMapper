namespace LightweightObjectMapper.Models;

internal struct MappingProfileItem
{
    #region Public 属性

    public bool HasMappingPrepare => MappingPrepareInvokeDescriptor is not null;

    public bool HasPostMapping => PostMappingInvokeDescriptor is not null;

    public bool HasProfile { get; }

    public bool HasTypeMapping => TypeMappingInvokeDescriptor is not null;

    /// <summary>
    /// 调用 <see cref="IMappingPrepare{TIn, TOut}"/> 的描述
    /// </summary>
    public MethodInvokeDescriptor? MappingPrepareInvokeDescriptor { get; }

    /// <summary>
    /// 调用 <see cref="IPostMapping{TIn, TOut}"/> 的描述
    /// </summary>
    public MethodInvokeDescriptor? PostMappingInvokeDescriptor { get; }

    /// <summary>
    /// 调用 <see cref="ITypeMapping{TIn, TOut}"/> 的描述
    /// </summary>
    public MethodInvokeDescriptor? TypeMappingInvokeDescriptor { get; }

    #endregion Public 属性

    #region Public 构造函数

    public MappingProfileItem(MethodInvokeDescriptor? mappingPrepareInvokeDescriptor = null, MethodInvokeDescriptor? postMappingInvokeDescriptor = null, MethodInvokeDescriptor? typeMappingInvokeDescriptor = null)
    {
        MappingPrepareInvokeDescriptor = mappingPrepareInvokeDescriptor;
        PostMappingInvokeDescriptor = postMappingInvokeDescriptor;
        TypeMappingInvokeDescriptor = typeMappingInvokeDescriptor;
        HasProfile = mappingPrepareInvokeDescriptor is not null
                     || postMappingInvokeDescriptor is not null
                     || typeMappingInvokeDescriptor is not null;
    }

    public MappingProfileItem(MappingProfileItem other, MethodInvokeDescriptor? mappingPrepareInvokeDescriptor = null, MethodInvokeDescriptor? postMappingInvokeDescriptor = null, MethodInvokeDescriptor? typeMappingInvokeDescriptor = null)
        : this(mappingPrepareInvokeDescriptor: mappingPrepareInvokeDescriptor ?? other.MappingPrepareInvokeDescriptor,
               postMappingInvokeDescriptor: postMappingInvokeDescriptor ?? other.PostMappingInvokeDescriptor,
               typeMappingInvokeDescriptor: typeMappingInvokeDescriptor ?? other.TypeMappingInvokeDescriptor)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public static implicit operator bool(MappingProfileItem value)
    {
        return value.HasProfile;
    }

    #endregion Public 方法
}
