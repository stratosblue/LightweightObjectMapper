﻿// <Auto-Generated/>

using LightweightObjectMapper.Properties;

using Microsoft.CodeAnalysis;

namespace LightweightObjectMapper;

internal static class DiagnosticDescriptorCategories
{
    public const string LightweightObjectMapper = "LightweightObjectMapper";
}

/// <summary>
/// 
/// </summary>
public static class DiagnosticDescriptors
{
    public static class Error
    {
        /// <summary>
        /// 构造时出现意外的异常
        /// </summary>
        public static DiagnosticDescriptor UnexpectedExceptionWhileBuilding { get; } = new("LOM000", Resources.UnexpectedExceptionWhileBuildingTitle, Resources.UnexpectedExceptionWhileBuildingMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Error, true);

        /// <summary>
        /// 模糊的映射类型
        /// </summary>
        public static DiagnosticDescriptor AmbiguousMapType { get; } = new("LOM001", Resources.AmbiguousMapTypeDiagnosticTitle, Resources.AmbiguousMapTypeDiagnosticMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Error, true);

        /// <summary>
        /// 没有公共构造函数
        /// </summary>
        public static DiagnosticDescriptor NoAnyPublicConstructor { get; } = new("LOM002", Resources.NoAnyPublicConstructorTitle, Resources.NoAnyPublicConstructorMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Error, true);

        /// <summary>
        /// 构造函数匹配失败
        /// </summary>
        public static DiagnosticDescriptor ConstructorMatchFailed { get; } = new("LOM003", Resources.ConstructorMatchFailedTitle, Resources.ConstructorMatchFailedMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Error, true);

        /// <summary>
        /// 错误的Profile导入
        /// </summary>
        public static DiagnosticDescriptor ErrorMappingProfileInclude { get; } = new("LOM004", Resources.ErrorMappingProfileIncludeTitle, Resources.ErrorMappingProfileIncludeMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Error, true);

        /// <summary>
        /// 需要 partial 关键字
        /// </summary>
        public static DiagnosticDescriptor NeedPartialKeyword { get; } = new("LOM005", Resources.NeedPartialKeywordTitle, Resources.NeedPartialKeywordMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Error, true);

        /// <summary>
        /// 集合映射方法定义错误
        /// </summary>
        public static DiagnosticDescriptor CollectionMappingMethodDefineError { get; } = new("LOM006", Resources.CollectionMappingMethodDefineErrorTitle, Resources.CollectionMappingMethodDefineErrorMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Error, true);

        /// <summary>
        /// 集合映射错误使用方式
        /// </summary>
        public static DiagnosticDescriptor CollectionMapIncorrectUsage { get; } = new("LOM007", Resources.CollectionMapIncorrectUsageTitle, Resources.CollectionMapIncorrectUsageMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Error, true);

        /// <summary>
        /// 不支持的集合映射
        /// </summary>
        public static DiagnosticDescriptor UndefinedCollectionMapping { get; } = new("LOM008", Resources.UndefinedCollectionMappingTitle, Resources.UndefinedCollectionMappingMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Error, true);
    }

    public static class Warning
    {
        /// <summary>
        /// 属性或字段类型不匹配
        /// </summary>
        public static DiagnosticDescriptor PropertyOrFieldTypeNotMatch { get; } = new("LOM101", Resources.PropertyOrFieldTypeNotMatchTitle, Resources.PropertyOrFieldTypeNotMatchMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Warning, true);

        /// <summary>
        /// init 属性无法映射
        /// </summary>
        public static DiagnosticDescriptor InitOnlyPropertyCanNotMap { get; } = new("LOM102", Resources.InitOnlyPropertyCanNotMapTitle, Resources.InitOnlyPropertyCanNotMapMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Warning, true);

        /// <summary>
        /// 只读字段无法映射
        /// </summary>
        public static DiagnosticDescriptor ReadOnlyFieldCanNotMap { get; } = new("LOM103", Resources.ReadOnlyFieldCanNotMapTitle, Resources.ReadOnlyFieldCanNotMapMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Warning, true);

        /// <summary>
        /// 重复定义集合映射方法
        /// </summary>
        public static DiagnosticDescriptor DuplicateDefinitionCollectionMappingMethod { get; } = new("LOM104", Resources.DuplicateDefinitionCollectionMappingMethodTitle, Resources.DuplicateDefinitionCollectionMappingMethodMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Warning, true);
    }

    public static class Information
    {
        /// <summary>
        /// 没有匹配到属性或字段
        /// </summary>
        public static DiagnosticDescriptor NotFoundMatchPropertyOrField { get; } = new("LOM201", Resources.NotFoundMatchPropertyOrFieldTitle, Resources.NotFoundMatchPropertyOrFieldMessage, DiagnosticDescriptorCategories.LightweightObjectMapper, DiagnosticSeverity.Info, true);
    }
}