using System;

namespace LightweightObjectMapper.Models;

internal struct CollectionMappingProfileItem
{
    #region Public 属性

    public bool HasProfile { get; }

    public MethodInvokeDescriptor? InvokeDescriptor { get; }

    #endregion Public 属性

    #region Public 构造函数

    public CollectionMappingProfileItem(MethodInvokeDescriptor? invokeDescriptor = null)
    {
        InvokeDescriptor = invokeDescriptor;
        HasProfile = invokeDescriptor is not null;
    }

    public CollectionMappingProfileItem(CollectionMappingProfileItem other, MethodInvokeDescriptor? invokeDescriptor = null)
        : this(invokeDescriptor: invokeDescriptor ?? other.InvokeDescriptor)
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public static implicit operator bool(CollectionMappingProfileItem value)
    {
        return value.HasProfile;
    }

    #endregion Public 方法
}
