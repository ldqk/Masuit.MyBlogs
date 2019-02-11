using Masuit.LuceneEFCore.SearchEngine;
using Masuit.MyBlogs.Core.Models.Enum;
using System.ComponentModel;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    /// <summary>
    /// 基类型
    /// </summary>
    public class BaseEntity : LuceneIndexableBaseEntity
    {
        [DefaultValue(Status.Default), LuceneIndex]
        public Status Status { get; set; }
    }
}