using LIFLog.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIFLog.Classes
{
    public class LogFileBase
    {
        public List<string> Filenames;

        public LogFileBase(String filename)
        {
            Filenames = new List<string>();
            if (File.Exists(filename)) {
                Filenames.Add(filename);
            }
            else
            {
                throw new Exception("Error getting file " + filename + " : file does not exist");
            }
        }

        public LogFileBase(List<string> filenames)
        {
            Filenames = new List<string>();
            if (filenames is null) { throw new ArgumentNullException(nameof(filenames)); }
            foreach (string filename in filenames)
            {
                if (File.Exists(filename))
                {
                    Filenames.Add(filename);
                }
            }
        }

        public List<Hit> GetLIFData(BackgroundWorker worker, DoWorkEventArgs e)
        {
            return GetHitsFromLogsFilesFR(Filenames, worker, e);
        }

        /// <summary>
        /// Reads Log files, get the content and parse
        /// </summary>
        /// <param name="filenames"></param>
        /// <returns>string containing damage log lines</returns>
        private List<Hit> GetHitsFromLogsFilesFR(List<string> filenames, BackgroundWorker worker, DoWorkEventArgs e)
        {
            List<Hit> hits = new List<Hit>();
            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                NumberStyles style;
                CultureInfo culture;
                style = NumberStyles.AllowDecimalPoint;
                culture = CultureInfo.CreateSpecificCulture("en-US");

                string[] separatingChars = { "spop", "spush", "<", ">", "(", ")", "color:C65F5F", "{}", ". Bonus de vitesse : " };
                int i = 0;
                DateTime date = DateTime.Now;
                int TotalFileNumber = filenames.Count;
                int currentFileNumber = 0;
                foreach (string filename in filenames)
                {
                    FileStream logFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    StreamReader logFileReader = new StreamReader(logFileStream);

                    while (!logFileReader.EndOfStream)
                    {

                        String currentline = logFileReader.ReadLine();
                        if (currentline.Contains("cible")) continue;

                        String dateLine = "";

                        if (currentline.Contains("touché"))
                        {
                            string[] damageStrimedLine = currentline.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);

                            Double.TryParse(damageStrimedLine[5], style, culture, out Double damage);
                            int.TryParse(damageStrimedLine[8], out int speed);
                            String name = damageStrimedLine[1];
                            String bodyPart = damageStrimedLine[3];
                            String damageType = damageStrimedLine[7];
                            //

                            if (!logFileReader.EndOfStream)
                            {
                                dateLine = logFileReader.ReadLine();
                                string[] datelinetable = dateLine.Split(' ');
                                DateTime.TryParse(datelinetable[1] + " " + datelinetable[2], out date);
                            }
                            Hit.DirectionEnum directionEnum = new Hit.DirectionEnum();
                            if (currentline.Contains("Vous avez touché"))
                            {
                                directionEnum = Hit.DirectionEnum.outgoing;
                            }
                            else
                            {
                                directionEnum = Hit.DirectionEnum.incoming;
                            }

                            Hit hit = new Hit(i++, date, name, directionEnum, bodyPart, damageType, speed, damage);
                            hits.Add(hit);


                        }
                    }

                    // Clean up
                    logFileReader.Close();
                    logFileStream.Close();

                    int percentComplete =
                    (int)((float)currentFileNumber++ / (float)TotalFileNumber);
                    
                    worker.ReportProgress(percentComplete);
                    
                }
            }
            return hits;
        }
    }
}
