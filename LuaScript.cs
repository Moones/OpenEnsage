using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Loader
{
    [Serializable]
    internal class LuaScript
    {
        private readonly Dictionary<string, LuaScript.RequirementState> _requirements = new Dictionary<string, LuaScript.RequirementState>();
        private bool _load;
        private string _scriptName;
        private LuaScript.RequirementState _state;

        public string ScriptName
        {
            get
            {
                return _scriptName;
            }
            set
            {
                _scriptName = value;
            }
        }

        public bool LoadScript
        {
            get
            {
                return _load;
            }
            set
            {
                _load = value;
            }
        }

        public bool HasConfig { get; private set; }

        public string ScriptDescription { get; private set; }

        public Dictionary<string, LuaScript.RequirementState> Requirements
        {
            get
            {
                return _requirements;
            }
        }

        public LuaScript.RequirementState State
        {
            get
            {
                return _state;
            }
            private set
            {
                if (_state == LuaScript.RequirementState.StateUnknown)
                    return;
                _state = value;
            }
        }

        public LuaScript(string name)
        {
            HasConfig = false;
            _scriptName = name;
            CheckRequirements();
        }

        public override string ToString()
        {
            return _scriptName.Substring(_scriptName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
        }

        private void CheckRequirements()
        {
            try
            {
                string[] strArray = File.ReadAllLines(Path.Combine("Scripts", _scriptName + ".lua"));
                Regex regexConfig = new Regex("ScriptConfig\\.new\\(", RegexOptions.Compiled);
                using (IEnumerator<string> enumerator = Enumerable.Where<string>(strArray, line => regexConfig.IsMatch(line)).GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        string current = enumerator.Current;
                        HasConfig = true;
                    }
                }
                Regex regex1 = new Regex("--<<(.*?)>>", RegexOptions.Compiled);
                ScriptDescription = strArray.Length <= 0 || !regex1.IsMatch(strArray[0]) ? string.Empty : regex1.Match(strArray[0]).Groups[1].Value;
                Regex regex = new Regex("require[ ]*\\([ ]*\"libs\\.([A-Za-z0-9]+)\"[ ]*\\)", RegexOptions.Compiled);
                List<string> list = Enumerable.ToList<string>(Enumerable.Select<string, string>(Enumerable.Where<string>(strArray, line => regex.IsMatch(line)), line => regex.Match(line).Groups[1].Value));
                _state = LuaScript.RequirementState.StateOkay;
                foreach (string index in list)
                {
                    _requirements[index] = LuaScript.RequirementState.StateOkay;
                    try
                    {
                        if (!File.Exists(Path.Combine("Scripts", "libs", index + ".lua")))
                        {
                            _requirements[index] = LuaScript.RequirementState.StateError;
                            _state = LuaScript.RequirementState.StateError;
                        }
                    }
                    catch (Exception)
                    {
                        _requirements[index] = LuaScript.RequirementState.StateUnknown;
                        _state = LuaScript.RequirementState.StateUnknown;
                    }
                }
            }
            catch (Exception)
            {
                _state = LuaScript.RequirementState.StateUnknown;
            }
        }

        public enum RequirementState
        {
            StateUnknown,
            StateOkay,
            StateError,
        }
    }
}
