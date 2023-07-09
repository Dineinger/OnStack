using System;

namespace OnStackShared
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class OnStackAttribute : Attribute
    {
        public OnStackAttribute()
        {
        }
    }
}