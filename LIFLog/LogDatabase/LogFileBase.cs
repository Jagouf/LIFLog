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

                int instanceNum = 0;
                


                string[] separatingChars = { "spop", "spush", "<", ">", "(", ")", "color:C65F5F", "{}", ". Bonus de vitesse : " };
                int i = 0;
                DateTime date = DateTime.Now;
                int TotalFileNumber = filenames.Count;
                int currentFileNumber = 0;
                foreach (string filename in filenames)
                {
                    String instanceType = "OpenWorld";
                    String instance = "OpenWorld";
                    FileStream logFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    StreamReader logFileReader = new StreamReader(logFileStream);

                    while (!logFileReader.EndOfStream)
                    {

                        String currentline = logFileReader.ReadLine();
                        if (currentline.Contains("cible")) continue;

                        String dateLine = "";
                        if (currentline.Contains("minutes pour vous préparer à la Bataille")){
                            instanceNum++;
                            instanceType = "Bataille ";
                            instance = instanceType + instanceNum;
                        }
                        if (currentline.Contains("arène a ouvert ses portes"))
                        {
                            instanceNum++;
                            instanceType = "Arene ";
                            instance = instanceType + instanceNum;
                        }
                        if (currentline.Contains("Leave battle!"))
                        {
                            instanceNum++;
                            instanceType = "OpenWorld";
                            instance = "OpenWorld";
                        }
                        

                        if (currentline.Contains("touché"))
                        {
                            Double damage = 0d;
                            Double hitConscience= 0d;
                            string[] damageStrimedLine = currentline.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);
                            String damageType = damageStrimedLine[9];
                            String bodyPart = damageStrimedLine[5];
                            if (damageType == "contondant" || bodyPart == "bouclier")
                            {
                                Double.TryParse(damageStrimedLine[7], style, culture, out hitConscience);
                            }
                            else {
                                Double.TryParse(damageStrimedLine[7], style, culture, out damage);
                            }
                            int.TryParse(damageStrimedLine[10], out int speed);
                            String name = damageStrimedLine[3];
                            
                            

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

                            Hit hit = new Hit(i++, instance, date, name, directionEnum, bodyPart, damageType, speed, damage, hitConscience);
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
