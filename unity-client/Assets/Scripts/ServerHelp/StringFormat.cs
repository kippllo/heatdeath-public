namespace Backend.GameObj { //Changed the name space so the compiler won't complain.

    /// <summary>
    /// This class helps with nested JSON strings.
    /// </summary>
    public static class StringFormat{

        public static readonly string q0 = nestQ(0);
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