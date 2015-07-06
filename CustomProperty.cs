// Decompiled with JetBrains decompiler
// Type: Loader.CustomProperty
// Assembly: Loader, Version=0.1.5611.35443, Culture=neutral, PublicKeyToken=null
// MVID: 767D8978-23D8-4AB7-BA8A-78DBFB5F0780
// Assembly location: E:\Downloads\ensage\Dumps\Loader_fix.exe

namespace Loader
{
    public class CustomProperty
    {
        private readonly string _name = string.Empty;
        private readonly string _category = string.Empty;
        private readonly bool _visible = true;
        private readonly bool _readOnly;
        private readonly object _originalValue;

        public bool ReadOnly
        {
            get
            {
                return _readOnly;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string Category
        {
            get
            {
                return _category;
            }
        }

        public bool Visible
        {
            get
            {
                return _visible;
            }
        }

        public object Value { get; set; }

        public object OriginalValue
        {
            get
            {
                return _originalValue;
            }
        }

        public CustomProperty(string category, string name, object value, bool readOnly, bool visible)
        {
            _name = name;
            _category = category;
            Value = value;
            _originalValue = value;
            _readOnly = readOnly;
            _visible = visible;
        }

        public CustomProperty(string name, object value, bool readOnly, bool visible)
        {
            _name = name;
            Value = value;
            _originalValue = value;
            _readOnly = readOnly;
            _visible = visible;
        }
    }
}
