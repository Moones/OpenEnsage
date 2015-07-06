// Decompiled with JetBrains decompiler
// Type: Loader.Repository
// Assembly: Loader, Version=0.1.5611.35443, Culture=neutral, PublicKeyToken=null
// MVID: 767D8978-23D8-4AB7-BA8A-78DBFB5F0780
// Assembly location: E:\Downloads\ensage\Dumps\Loader_fix.exe

using LibGit2Sharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Loader
{
    internal class Repository
    {
        private readonly string _url;
        private readonly string _name;
        private readonly List<LuaScript> _scriptList;
        private readonly List<LuaScript> _libList;
        private readonly string _path;

        public string Url
        {
            get
            {
                return _url;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public List<LuaScript> Scripts
        {
            get
            {
                return _scriptList;
            }
        }

        public List<LuaScript> Libs
        {
            get
            {
                return _libList;
            }
        }

        public Repository(string url, string path)
        {
            _path = path;
            _url = url;
            _name = _url.Substring("https://github.com/".Length);
            _name = _name.Replace('/', '\\');
            _scriptList = new List<LuaScript>();
            _libList = new List<LuaScript>();
            UpdateRepository();
        }

        public void UpdateRepository()
        {
            string str = Path.Combine(_path, _name);
            if (!Directory.Exists(str))
            {
                Directory.CreateDirectory(str);
                LibGit2Sharp.Repository.Clone(_url, str, null);
            }
            else
            {
                using (LibGit2Sharp.Repository repository = new LibGit2Sharp.Repository(str, null))
                {
                    Remote remote = Enumerable.First<Remote>(repository.Network.Remotes);
                    repository.Network.Fetch(remote, null, null, null);
                }
            }
            _scriptList.Clear();
            string path1 = Path.Combine(str, "Scripts");
            if (Directory.Exists(path1))
            {
                foreach (string name in Directory.GetFiles(path1))
                    _scriptList.Add(new LuaScript(name));
            }
            _libList.Clear();
            string path2 = Path.Combine(str, "Libraries");
            if (!Directory.Exists(path2))
                return;
            foreach (string name in Directory.GetFiles(path2))
                _libList.Add(new LuaScript(name));
        }

        public override string ToString()
        {
            return _url;
        }
    }
}
