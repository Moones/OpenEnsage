﻿using System;
using System.IO;
using System.Security.Cryptography;

namespace Loader
{
    internal class Md5 : IDisposable
    {
        private readonly MD5 _md5;

        public Md5()
        {
            _md5 = MD5.Create();
        }

        public void Dispose()
        {
            _md5.Dispose();
        }

        public string GetHash(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException();
            byte[] buffer;
            using (FileStream fileStream = File.OpenRead(fileName))
            {
                int length = (int)fileStream.Length;
                buffer = new byte[length];
                int offset = 0;
                int num;
                do
                {
                    num = fileStream.Read(buffer, offset, length - offset);
                    offset += num;
                }
                while (num > 0);
                for (int index = 0; index < buffer.Length; ++index)
                    buffer[index] ^= 39;
            }
            return BitConverter.ToString(_md5.ComputeHash(buffer)).Replace("-", "").ToLower();
        }
    }
}
