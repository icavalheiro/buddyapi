using Buddy.API.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Buddy.API
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PreferedInputAttribute : Attribute
    {
        public string Value { get; set; }

        public PreferedInputAttribute(InputType type)
        {
            Value = type.ToString();
        }

        public PreferedInputAttribute(string customName)
        {
            Value = customName;
        }
    }
}
