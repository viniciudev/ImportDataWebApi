using System;

namespace ImportDataWebApi
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class SensitiveDataAttribute : Attribute
    {
    }
}
