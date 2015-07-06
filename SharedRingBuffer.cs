// Decompiled with JetBrains decompiler
// Type: Loader.SharedRingBuffer
// Assembly: Loader, Version=0.1.5611.35443, Culture=neutral, PublicKeyToken=null
// MVID: 767D8978-23D8-4AB7-BA8A-78DBFB5F0780
// Assembly location: E:\Downloads\ensage\Dumps\Loader_fix.exe

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace Loader
{
    public class SharedRingBuffer : IDisposable
    {
        private readonly MemoryMappedFile _memory;
        private readonly MemoryMappedViewStream _memoryStream;
        private readonly BinaryWriter _writer;
        private readonly BinaryReader _reader;
        private int _parser;
        private readonly int _size;

        public int Parser
        {
            get
            {
                return _parser;
            }
            set
            {
                _parser = value;
            }
        }

        public SharedRingBuffer(string name, int size)
        {
            _size = size;
            _memory = MemoryMappedFile.CreateOrOpen(name, size + 1);
            _memoryStream = _memory.CreateViewStream();
            _writer = new BinaryWriter(_memoryStream);
            _reader = new BinaryReader(_memoryStream);
            _writer.Write(char.MinValue);
        }

        public void Write(uint value)
        {
            _writer.Seek(_parser, SeekOrigin.Begin);
            _writer.Write(value);
            _parser = (_parser + 4) % _size;
        }

        public void Write(byte[] arr)
        {
            foreach (byte c in arr)
                Write(c);
        }

        public void Write(char c)
        {
            _writer.Seek(_parser, SeekOrigin.Begin);
            _writer.Write(c);
            _parser = (_parser + 1) % _size;
        }

        public void Write(byte c)
        {
            _writer.Seek(_parser, SeekOrigin.Begin);
            _writer.Write(c);
            _parser = (_parser + 1) % _size;
        }

        public void Write(string text)
        {
            foreach (uint num in Encoding.ASCII.GetBytes(text))
                Write((char)(num ^ 39U));
            Write(char.MinValue);
            long position = _writer.BaseStream.Position;
            Write(char.MinValue);
            _parser = (int)position;
        }

        public bool HasMsg()
        {
            _memoryStream.Seek(_parser, SeekOrigin.Begin);
            return _reader.ReadChar() != 0;
        }

        public string GetMsg()
        {
            _memoryStream.Seek(_parser, SeekOrigin.Begin);
            StringBuilder stringBuilder = new StringBuilder();
            char ch;
            while ((ch = _reader.ReadChar()) != 0)
            {
                stringBuilder.Append(ch);
                _parser = (_parser + 1) % _size;
            }
            _parser = (_parser + 1) % _size;
            return stringBuilder.ToString();
        }

        public void Reset()
        {
            _parser = 0;
            _writer.Seek(0, SeekOrigin.Begin);
            _writer.Write(char.MinValue);
        }

        public void Dispose()
        {
            _memoryStream.Close();
            _memory.Dispose();
        }
    }
}
