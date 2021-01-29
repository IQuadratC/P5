using System.IO;

namespace Utility
{
    public static class FileHelper
    {
        public static string Read(string path)
        {
            return File.ReadAllText(path);
        }

        public static void Write(string path, string content)
        {
            File.WriteAllText(path, content);
        }
    }
}