// Decompiled with JetBrains decompiler
// Type: Loader.CustomPropertyDescriptor
// Assembly: Loader, Version=0.1.5611.35443, Culture=neutral, PublicKeyToken=null
// MVID: 767D8978-23D8-4AB7-BA8A-78DBFB5F0780
// Assembly location: E:\Downloads\ensage\Dumps\Loader_fix.exe

using System;
using System.ComponentModel;

namespace Loader
{
    public class CustomPropertyDescriptor : PropertyDescriptor
    {
        private readonly CustomProperty _property;

        public override Type ComponentType
        {
            get
            {
                return null;
            }
        }

        public override string Description
        {
            get
            {
                return _property.Name;
            }
        }

        public override string Category
        {
            get
            {
                return _property.Category;
            }
        }

        public override string DisplayName
        {
            get
            {
                return _property.Name;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return _property.ReadOnly;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return _property.Value.GetType();
            }
        }

        public CustomPropertyDescriptor(ref CustomProperty myProperty, Attribute[] attrs)
          : base(myProperty.Name, attrs)
        {
            _property = myProperty;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return _property.Value;
        }

        public override void ResetValue(object component)
        {
            _property.Value = _property.OriginalValue;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override void SetValue(object component, object value)
        {
            _property.Value = value;
        }
    }
}
