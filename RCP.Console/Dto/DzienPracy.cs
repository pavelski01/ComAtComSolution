namespace RCP.Console.Dto
{
    public class DzienPracy : IEquatable<DzienPracy>
    {
        public string KodPracownika { get; set; } = "";
        public DateTime Data { get; set; }
        public TimeSpan GodzinaWejscia { get; set; }
        public TimeSpan GodzinaWyjscia { get; set; }

        public bool Equals(DzienPracy? other)
        {
            if (other == null)
            {
                return false;
            }
            if (KodPracownika == other.KodPracownika
                && Data == other.Data
                && GodzinaWejscia == other.GodzinaWejscia
                && GodzinaWyjscia == other.GodzinaWyjscia)
            {
                return true;
            }
            return false;
        }
    }
}
