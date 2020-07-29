using Masuit.LuceneEFCore.SearchEngine;
using Masuit.MyBlogs.Core.Models.Enum;
using Masuit.Tools.Core.AspNetCore;
using System.ComponentModel;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 基类型
    /// </summary>
    public class BaseEntity : LuceneIndexableBaseEntity
    {
        [DefaultValue(Status.Default), LuceneIndex, UpdateIgnore]
        public Status Status { get; set; }
    }
}