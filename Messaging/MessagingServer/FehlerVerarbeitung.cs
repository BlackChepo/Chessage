using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chepo.MessagingServer
{
    /// <summary>
    /// Erweiterungsklasse
    /// </summary>
    public static class FehlerVerarbeitung
    {
        private static string fehlerPräfix = "Fehler: ";

        /// <summary>
        /// Verarbeitet eine Fehlermeldung
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="mitStack"></param>
        public static void verarbeite(this Exception ex, bool mitStack = false)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(fehlerPräfix + ex.ToString());
            sb.AppendLine(ex.Message);

            if (mitStack)
                sb.AppendLine("Stack:" + Environment.NewLine + ex.StackTrace);

            Console.WriteLine(sb.ToString());
        }
    }
}
