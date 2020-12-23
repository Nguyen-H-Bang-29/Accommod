using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebApi.Helpers
{
    public class FileSystemService
    {
        public static string GetOrCreateDirectory(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
        public static Stream GetOrCreateFile(string directory, string path)
        {
            var folder = GetOrCreateDirectory(directory);
            string fullPath = Path.Combine(folder, path);
            return File.Exists(fullPath) ? File.Open(fullPath, FileMode.OpenOrCreate) : File.Create(fullPath);
        }
        public static List<string> GetFileNames(string directory)
        {
            return Directory.GetFiles(directory).ToList();
        }
        public static Stream GetFile(string directory, string path)
        {
            string fullPath = Path.Combine(GetOrCreateDirectory(directory), path);
            return File.Exists(fullPath) ? File.Open(fullPath, FileMode.Open) : Stream.Null;
        }
    }
}
