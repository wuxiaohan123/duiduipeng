using System;

namespace DuiDuiPeng
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (DuiDuiPengGUI game = new DuiDuiPengGUI())
            {
                game.Run();
            }
        }
    }
#endif
}

