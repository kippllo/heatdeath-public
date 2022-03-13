
namespace Keybored.BackendServer.Network {
    
    public static class StringFormat {

        //These are cached static-readonly vars that every class and access to save CPU time for quotes...
        public static readonly string q0 = nestQ(0); //"const" vars can't use a function to assign a value because they are assigned at compile time not run time. Instead use "static readonly" which is assigned at run time.    See: https://exceptionnotfound.net/const-vs-static-vs-readonly-in-c-sharp-applications/
        public static readonly string q1 = nestQ(1);
        public static readonly string q2 = nestQ(2);
        public static readonly string q3 = nestQ(3);
        public static readonly string q4 = nestQ(4);
        public static readonly string q5 = nestQ(5);

        //Double of last Nest level plus one = how many slashes before the quote.
        private static int getSlashCount(int lvlOfNest2){
            if (lvlOfNest2 == 0){ return 0; }

            int count = getSlashCount(lvlOfNest2-1)*2 +1;
            return count;
        }

        public static string nestQ(int lvlOfNest){ //Returns the correct nested quote level string. Something like: ///" or ///////"
            string q = "";
            int slashCount = getSlashCount(lvlOfNest);

            for(int i=0; i<slashCount; i++){
                q += "\\"; //Add a slash for each
            }

            q += "\""; //Actually add the quote mark.
            return q;
        }

    }

}