namespace SysManager.Models
{
    public class Gestiune
    {
        public int Id { get; set; }
        public string Nume { get; set; }           // NUME - cod scurt (afișat pe buton)
        public string NumeGest { get; set; }       // NUME_GEST - nume complet
        public int Status { get; set; }            // 1 = activ, 0 = inactiv

        // ✅ Pentru afișare - PRIORITATE: NUME (nu NUME_GEST)
        public string DisplayName => !string.IsNullOrWhiteSpace(Nume) ? Nume : NumeGest;
        public bool IsActive => Status == 1;
    }
}
