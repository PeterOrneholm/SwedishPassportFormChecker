namespace SwedishPassportFormChecker.Console
{
    internal class PassportFormCheckerModel
    {
        public string Meta_Datum { get; set; }
        public string Meta_Diarienummer { get; set; }

        public bool Typ_EndastPass { get; set; }
        public bool Typ_EndastNationelltIdKort { get; set; }
        public bool Typ_BådePassOchNationelltIdKort { get; set; }

        public string Minderårig_Personnummer { get; set; }
        public string Minderårig_Efternamn { get; set; }
        public string Minderårig_Förnamn { get; set; }
        public string Minderårig_Längd { get; set; }

        public string Vårdnadshavare1_IdKontroll { get; set; }
        public string Vårdnadshavare2_IdKontroll { get; set; }
    }
}