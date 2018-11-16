namespace TinyDIFramework.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Field)]
    public class Inject : Attribute
    {
       
    }
}