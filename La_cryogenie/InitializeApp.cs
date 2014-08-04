using System.IO;

namespace La_cryogenie
{
    static class InitializeApp
    {
        public static void createDirs()
        {
            if (!Directory.Exists("log"))
            {
                Directory.CreateDirectory("log");
            }
        }
    }
}
