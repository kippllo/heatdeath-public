using System;
using System.IO;
using System.Text;

namespace Keybored.BackendServer.Logging {

    public static class FileLogger {

        private static string filepath = "./log/";


        public static void logToFile(string log){
			checkDir();
			byte[] data = formatLogStr(log);
            writeLogAsync("log.txt", data); //Do I need to add "await"?
		}

        public static void logToFileSynchronous(string log){
			checkDir();
			byte[] data = formatLogStrNoNewLine(log);
            writeLog("log_synchronous.txt", data);
		}

        public static void logConnectionToFile(string log){
			checkDir();
			byte[] data = formatLogStr(log);
            writeLogAsync("connectionLog.txt", data);
		}

        public static void logErrorToFile(string log){
            checkDir();
            log = "----------------------------------------------------------------\nError Time: " + DateTimeOffset.Now.ToString("G") + "\n\n" + log + "\n----------------------------------------------------------------";
			byte[] data = formatLogStr(log);
            writeLog("errLog.txt", data); //Call the non-async function version for the error logger, I think the non-async won't produce the simultaneous write error.
		}

        private static void writeLog(string filename, byte[] data) {
            try{
                using(FileStream fs = File.Open(filepath + filename, FileMode.OpenOrCreate)){
                    fs.Seek(0, SeekOrigin.End);
                    fs.Write(data, 0, data.Length);
                }
            } catch { //If this is used for some besides the "logErrorToFile" function it will need to actually catch the error!
				//Do nothing if we error in the error catcher...
			}
        }

        private static async void writeLogAsync(string filename, byte[] data) {
            try{
                using(FileStream fs = File.Open(filepath + filename, FileMode.OpenOrCreate)){
                    fs.Seek(0, SeekOrigin.End);
                    await fs.WriteAsync(data, 0, data.Length);
                }
            } catch(Exception err){
				logErrorToFile(err.ToString());
			}
        }

        private static byte[] formatLogStr(string log){
            log = log.Replace("\n", Environment.NewLine); //Replace the use put newlines with platform-independent newline.
            log = log + Environment.NewLine + Environment.NewLine;
			byte[] data = Encoding.ASCII.GetBytes(log);
            return data;
        }

        private static byte[] formatLogStrNoNewLine(string log){
            log = log.Replace("\n", Environment.NewLine); //Replace the use put newlines with platform-independent newline.
			byte[] data = Encoding.ASCII.GetBytes(log);
            return data;
        }

        private static void checkDir(){
            if(!Directory.Exists("./log")){
				Directory.CreateDirectory("./log");
			}
        }

    }
}