// Decompiled with JetBrains decompiler
// Type: Loader.CustomClass
// Assembly: Loader, Version=0.1.5611.35443, Culture=neutral, PublicKeyToken=null
// MVID: 767D8978-23D8-4AB7-BA8A-78DBFB5F0780
// Assembly location: E:\Downloads\ensage\Dumps\Loader_fix.exe

using System;
using System.Collections;
using System.ComponentModel;

namespace Loader
{
    public class CustomClass : CollectionBase, ICustomTypeDescriptor
    {
        public CustomProperty this[int index]
        {
            get
            {
                return (CustomProperty)List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        public void Add(CustomProperty value)
        {
            List.Add(value);
        }

        public void Remove(string name)
        {
            foreach (CustomProperty customProperty in List)
            {
                if (customProperty.Name == name)
                {
                    List.Remove(customProperty);
                    break;
                }
            }
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptor[] properties = new PropertyDescriptor[Count];
            for (int index = 0; index < Count; ++index)
            {
                CustomProperty myProperty = this[index];
                properties[index] = new CustomPropertyDescriptor(ref myProperty, attributes);
            }
            return new PropertyDescriptorCollection(properties);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return TypeDescriptor.GetProperties(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
    }
}
