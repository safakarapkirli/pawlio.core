using Pawlio.Models;

namespace Pawlio;

public static class Settings
{
    public static int WaitingTime = 10;
    public static int SelectUserTime = 60;
    public static int SmsExpirationMinute = 3;
    public static int EMailExpirationMinute = 60 * 24;
    public static decimal BankCommissions = 0.89m;
    public static int DefinitionVersion = 0;
    public static Definition StartUrl = null!;

    //private static List<Package> _Packages = new List<Package>();
    //public static List<Package> Packages
    //{
    //    get
    //    {
    //        lock (_Packages)
    //        {
    //            if (_Packages.Count == 0)
    //            {
    //                var _context = new MySqlDbContext();
    //                _Packages = _context.Packages.Include(p => p.SubPackages).ToList();
    //                foreach (var p in _Packages)
    //                    foreach (var sp in p.SubPackages)
    //                        sp.Package = null;
    //            }

    //            return _Packages;
    //        }
    //    }
    //}

    //public static Package getPackage(int packageId) => Packages.FirstOrDefault(p => p.Id == packageId) ?? new Package { Flavor = Flavor.None };

    public static void Load()
    {
        Console.WriteLine("Varsayılan ayarlar kullanılıyor...");
        //Console.WriteLine("Ayarlar alınıyor!");
        //var _context = new PostgreSqlDbContext();
        //var defs = _context.Definitions.Where(d => d.ParentId == 1).ToList();
        //DefinitionVersion = defs.First(d => d.Id == 2).Value!.ToInt();
        //BankCommissions = defs.First(d => d.Id == 5).Value!.ToDecimal();
        //StartUrl = defs.First(d => d.Id == 6);
    }
}