using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Pawlio.Models;

namespace Pawlio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilsController : ControllerBase
    {
        public IConfiguration _configuration;
        private readonly PostgreSqlDbContext _context;

        public UtilsController(IConfiguration config, PostgreSqlDbContext context)
        {
            _configuration = config;
            _context = context;
        }

        [HttpGet("mapTriggers")]
        public object CreateTriggers()
        {
            var esDbNames = new List<string> { "Branch", "UserFirmsBranch", "AnimalCategory" }; // Sonunda es çoğulu olan tablolar
            var ignoreColumnList = new List<string> { "Images", "Updated" };
            var ignoreModelList = new List<Type> {
                typeof(Flavor),
                typeof(ImageType),
                typeof(IImage),
                typeof(SupplierType),
                typeof(EventTypes),
                typeof(AccountingStatus),
                typeof(AccountingTypes),
                typeof(PaymentStatus),
                typeof(PaymentTypes),
                typeof(DefinitionValueType),
                typeof(AppointmentStatus),
                typeof(FirmTypes),
                typeof(LoginType),
                typeof(ModelBase),        
                //typeof(PayStatus),
                //typeof(CreditCardBanks),
                //typeof(PayFirm),
                //typeof(City),
                //typeof(District),
                //typeof(PayRequest),
                typeof(Event),
                typeof(Symptom),
                typeof(MCustomer),
                typeof(MAnimal),
                typeof(MUser),
                typeof(ProductAmount),
                typeof(ProductPriceHistory),
                typeof(Image),
                typeof(ImageModel),
                typeof(ExaminationSymptom),
                typeof(AnimalAccounting),
                typeof(AnimalAppointment),
                typeof(AnimalImage),
                typeof(AnimalCategoryJson),
                typeof(AnimalTypeJson),
                typeof(BreedJson),
                typeof(ColorJson),
            };

            // Tablo olmayıp column olan türler
            var acceptColumnList = new List<Type>
            {
                typeof(Flavor),
                typeof(ImageType),
                typeof(SupplierType),
                typeof(EventTypes),
                typeof(AccountingStatus),
                typeof(AccountingTypes),
                typeof(PaymentStatus),
                typeof(PaymentTypes),
                typeof(DefinitionValueType),
                //typeof(PayStatus),
                //typeof(CreditCardBanks),
                typeof(LoginType),
            };

            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.FullName!.Contains("Pawlio.Models") && !ignoreModelList.Contains(t) && !t.Name.StartsWith("_") && !t.Name.StartsWith("Base"))
                .ToList();

            var mysqlTriggerCode = @"
DROP TRIGGER IF EXISTS `___trigger_##TABLE##_Insert_Log`;
DROP TRIGGER IF EXISTS `___trigger_##TABLE##_Update_Log`;

CREATE TRIGGER `___trigger_##TABLE##_Insert_Log` AFTER INSERT ON `##TABLE##` FOR EACH ROW BEGIN
    INSERT INTO _##TABLE## (##COLUMNS##)
    VALUES (##NEWCOLUMNS##);
END;
CREATE TRIGGER `___trigger_##TABLE##_Update_Log` AFTER UPDATE ON `##TABLE##` FOR EACH ROW BEGIN
    INSERT INTO _##TABLE## (##COLUMNS##, Updated)
    VALUES (##NEWCOLUMNS##, CURRENT_TIMESTAMP);
END;
";
            // SET NEW.Updated = CURRENT_TIMESTAMP;

            int errorCount = 0, successCount = 0;
            var res = new List<string>();

            //var sql = Environment.NewLine + "SET GLOBAL log_bin_trust_function_creators = 1;" + Environment.NewLine;
            //try
            //{
            //    _context.Database.ExecuteSqlRaw(sql);
            //    sql = "/* OK */" + sql;
            //}
            //catch (Exception e)
            //{
            //    errorCount++;
            //    sql = "/* ERRROR: " + e.Message + "*/" + sql;
            //}

            //res.Add(sql);
            var sql = "";

            foreach (var t in types)
            {
                var dbName = t.Name + "s";
                if (dbName.Contains("<")) continue;
                if (esDbNames.Contains(t.Name)) dbName = t.Name + "es";

                var properties = t.GetProperties();
                var colums = "";
                var newColums = "";

                foreach (var p in properties)
                {
                    bool pass = false;

                    if (p.Name == "Status")
                    {

                    }

                    foreach (var a in p.CustomAttributes)
                        if (a.AttributeType.Name == "NotMappedAttribute")
                        {
                            pass = true;
                            break;
                        }

                    if (pass) continue;

                    //if (name == "Key") name = "`Key`";
                    // Column adı türlerinde var ise başka kontrol yapmaya gerek yok
                    if (!acceptColumnList.Contains(p.PropertyType))
                    {
                        var search = p.ToString() ?? "";
                        if (search.Contains("Pawlio") && p.Name != "ImageType") continue;
                        if (search.Contains("Byte[]")) continue;
                        if (ignoreColumnList.Contains(p.Name)) continue;
                    }

                    if (colums != "")
                    {
                        colums += ", ";
                        newColums += ", ";
                    }

                    var name = $"`{p.Name}`";
                    colums += name;
                    newColums += "NEW." + name;
                }

                sql = mysqlTriggerCode.Replace("##TABLE##", dbName).Replace("##COLUMNS##", colums).Replace("##NEWCOLUMNS##", newColums);
                try
                {
                    _context.Database.ExecuteSqlRaw(sql);
                    successCount++;
                    sql = "/* OK */" + sql;
                }
                catch (Exception e)
                {
                    errorCount++;
                    sql = "/* ERROR: " + e.Message + "*/" + sql;
                }

                res.Add(sql);
            }

            res.Insert(0, $"Sucsess: {successCount}, Error: {errorCount}{Environment.NewLine}");
            return res.Aggregate((a, b) => a + Environment.NewLine + b);
        }

        [HttpGet("createProcedures")]
        public object CreateProcedures()
        {
            var sql = @"
DROP PROCEDURE IF EXISTS UpdateBalance;
DROP FUNCTION IF EXISTS UpdateBalance;

CREATE OR REPLACE FUNCTION UpdateBalance(
    pFirmId INT,
    pBranchId INT,
    pCustomerId INT,
    pSupplierId INT,
    pAmount NUMERIC(18,2)
)
RETURNS void
LANGUAGE plpgsql
AS $$
DECLARE
    vIsCustomer BOOLEAN := FALSE;
    vIsSupplier BOOLEAN := FALSE;
    v_rowcount  INT;
BEGIN
    -- müşteri mi tedarikçi mi?
    IF pCustomerId IS NOT NULL THEN
        vIsCustomer := TRUE;
    ELSIF pSupplierId IS NOT NULL THEN
        vIsSupplier := TRUE;
    ELSE
        RETURN;
    END IF;

    IF vIsCustomer THEN
        UPDATE Balances
        SET Balance = Balance + pAmount,
            Updated = now()
        WHERE FirmId = pFirmId
          AND CustomerId = pCustomerId;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;

        IF v_rowcount = 0 THEN
            INSERT INTO Balances (
                Created, FirmId, BranchId, CustomerId, SupplierId, Balance
            )
            VALUES (
                now(), pFirmId, pBranchId, pCustomerId, NULL, pAmount
            );
        END IF;

    ELSIF vIsSupplier THEN
        UPDATE Balances
        SET Balance = Balance + pAmount,
            Updated = now()
        WHERE FirmId = pFirmId
          AND SupplierId = pSupplierId;

        GET DIAGNOSTICS v_rowcount = ROW_COUNT;

        IF v_rowcount = 0 THEN
            INSERT INTO Balances (
                Created, FirmId, BranchId, CustomerId, SupplierId, Balance
            )
            VALUES (
                now(), pFirmId, pBranchId, NULL, pSupplierId, pAmount
            );
        END IF;
    END IF;
END;
$$;


DROP PROCEDURE IF EXISTS UpdateProductAmount;
DROP FUNCTION IF EXISTS UpdateProductAmount;

CREATE OR REPLACE FUNCTION UpdateProductAmount(
    pType INT,
    pBranchId INT,
    pProductId INT,
    pQuantity NUMERIC(18,2)
)
RETURNS void
LANGUAGE plpgsql
AS $$
DECLARE
    vAmountChange NUMERIC(18,2);
    v_rowcount    INT;
BEGIN
    -- Type kontrolü
    IF pType = 1 THEN
        vAmountChange := pQuantity;      -- stok girişi
    ELSIF pType = 2 THEN
        vAmountChange := -pQuantity;     -- stok çıkışı
    ELSE
        RETURN;
    END IF;

    UPDATE ProductAmounts
    SET Amount = Amount + vAmountChange
    WHERE BranchId = pBranchId
      AND ProductId = pProductId;

    GET DIAGNOSTICS v_rowcount = ROW_COUNT;

    IF v_rowcount = 0 THEN
        INSERT INTO ProductAmounts (
            BranchId,
            ProductId,
            Amount
        )
        VALUES (
            pBranchId,
            pProductId,
            vAmountChange
        );
    END IF;
END;
$$;";
            try
            {
                _context.Database.ExecuteSqlRaw(sql);
                sql = "/* OK */" + sql;
            }
            catch (Exception e)
            {
                sql = "/* ERRROR: " + e.Message + "*/" + sql;
            }

            return sql;
        }

        [HttpGet("accountingsTriggers")]
        public object AccountingsTriggers()
        {
            var sql = @"
CREATE OR REPLACE FUNCTION trg_accountings_before_insert()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    -- Ürün stok güncelle
    PERFORM UpdateProductAmount(
        NEW.""Type"",
        NEW.""BranchId"",
        NEW.""EventId"",
        NEW.""Quantity""
    );

    -- Bakiye güncelle (negatif tutar)
    PERFORM UpdateBalance(
        NEW.""FirmId"",
        NEW.""BranchId"",
        NEW.""CustomerId"",
        NEW.""SupplierId"",
        -(NEW.""Quantity"" * NEW.""Amount"")
    );

    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS ___accountings_insert ON ""Accountings"";

CREATE TRIGGER ___accountings_insert
BEFORE INSERT ON ""Accountings""
FOR EACH ROW
EXECUTE FUNCTION trg_accountings_before_insert();

CREATE OR REPLACE FUNCTION trg_accountings_before_update()
RETURNS trigger
LANGUAGE plpgsql
AS $$
DECLARE
    lastQuantity NUMERIC(10,4) := 0.0;
    newQuantity  NUMERIC(10,4) := 0.0;
    lastAmount   NUMERIC(10,4) := 0.0;
    newAmount    NUMERIC(10,4) := 0.0;
BEGIN
    -- eski / yeni miktarlar
    lastQuantity := OLD.""Quantity"";
    newQuantity  := NEW.""Quantity"";

    lastAmount := lastQuantity * OLD.""Amount"";
    newAmount  := newQuantity  * NEW.""Amount"";

    -- eski status pasifse sıfırla
    IF OLD.""Status"" <> 1 THEN
        lastQuantity := 0;
        lastAmount   := 0;
    END IF;

    -- yeni status pasifse sıfırla
    IF NEW.""Status"" <> 1 THEN
        newQuantity := 0;
        newAmount   := 0;
    END IF;

    -- miktar farkı varsa stok güncelle
    IF lastQuantity <> newQuantity THEN
        PERFORM UpdateProductAmount(
            NEW.""Type"",
            NEW.""BranchId"",
            NEW.""EventId"",
            newQuantity - lastQuantity
        );
    END IF;

    -- tutar farkı varsa bakiye güncelle
    IF lastAmount <> newAmount THEN
        PERFORM UpdateBalance(
            NEW.""FirmId"",
            NEW.""BranchId"",
            NEW.""CustomerId"",
            NEW.""SupplierId"",
            lastAmount - newAmount
        );
    END IF;

    -- type 1 veya 2 ise ekstra stok güncellemesi
    IF NEW.""Type"" = 1 OR NEW.""Type"" = 2 THEN
        IF OLD.""Quantity"" <> NEW.""Quantity"" THEN
            PERFORM UpdateProductAmount(
                NEW.""Type"",
                NEW.""BranchId"",
                NEW.""EventId"",
                NEW.""Quantity"" - OLD.""Quantity""
            );
        END IF;
    END IF;

    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS ___accountings_update ON ""Accountings"";

CREATE TRIGGER ___accountings_update
BEFORE UPDATE ON ""Accountings""
FOR EACH ROW
EXECUTE FUNCTION trg_accountings_before_update();


            ";

            try
            {
                _context.Database.ExecuteSqlRaw(sql);
                sql = "/* OK */" + sql;
            }
            catch (Exception e)
            {
                sql = "/* ERRROR: " + e.Message + "*/" + sql;
            }

            return sql;
        }

        //[HttpGet("fb")]
        //public async Task<object> FirebaseTokenTest()
        //{
        //    //string token = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImY0NTEzNDVmYWQwODEwMWJmYjM0NWNmNjQyYTJkYTkyNjdiOWViZWIiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhenAiOiI0MTI5NzA3MTc2MTgtcGt2c283a283dmJlcWcxMjBsYThrOHJ1MXJpcWNmMWwuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJhdWQiOiI0MTI5NzA3MTc2MTgtNmFlcThzYmIwYnVjdGVvOWg4ZXFiaXMzYzBkcjdjNnQuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJzdWIiOiIxMDI0NzY4MDE1ODYyOTQwMjk1MTgiLCJlbWFpbCI6InNhZmFrYXJhcGtpcmxpQGdtYWlsLmNvbSIsImVtYWlsX3ZlcmlmaWVkIjp0cnVlLCJuYW1lIjoixZ5hZmFrIEFyYXBraXJsaSIsInBpY3R1cmUiOiJodHRwczovL2xoMy5nb29nbGV1c2VyY29udGVudC5jb20vYS9BTG01d3UxdlYxQ1NEY19YNFhiQ1ZIU3FQRmxyYV8tNGxqU0N3QWZEd1ZiYWJRPXM5Ni1jIiwiZ2l2ZW5fbmFtZSI6IsWeYWZhayIsImZhbWlseV9uYW1lIjoiQXJhcGtpcmxpIiwibG9jYWxlIjoidHIiLCJpYXQiOjE2NjgyNDAwMDUsImV4cCI6MTY2ODI0MzYwNX0.jhBD6MjEErGDythxcK9escGXzGALdWQQVtlUjesA1ckaSzVLW51SF1gqK7PbXsnd4CEIOKsfCQiOIcEQFRCIuzRNqRjVg5z-NcC11PdpR61yhiVNBh_JXBkLWSxMujnKN_mkzrR5BL7kMZOG4v6qPb0AlVvqYnvo7vNbVY56uGF8Hnw7ESJZUdAfMlLyr37anPv8gsqdhRxRM2WtXIf6FcDFBWiAGjWLXb7IxUdzBntgDVfGQBaI1xInndrEEo0nnh0s_xJwCf2OL2aONBVTxHzZCsMPfBSMENVr0ixOzU6nuZJvFzhX1u1cCOV8K46fH60P6DDQKaTvg5f4P_7QkQ";
        //    var token = "eyJhbGciOiJSUzI1NiIsImtpZCI6ImY0NTEzNDVmYWQwODEwMWJmYjM0NWNmNjQyYTJkYTkyNjdiOWViZWIiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhenAiOiI0MTI5NzA3MTc2MTgtcGt2c283a283dmJlcWcxMjBsYThrOHJ1MXJpcWNmMWwuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJhdWQiOiI0MTI5NzA3MTc2MTgtNmFlcThzYmIwYnVjdGVvOWg4ZXFiaXMzYzBkcjdjNnQuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJzdWIiOiIxMDI0NzY4MDE1ODYyOTQwMjk1MTgiLCJlbWFpbCI6InNhZmFrYXJhcGtpcmxpQGdtYWlsLmNvbSIsImVtYWlsX3ZlcmlmaWVkIjp0cnVlLCJuYW1lIjoixZ5hZmFrIEFyYXBraXJsaSIsInBpY3R1cmUiOiJodHRwczovL2xoMy5nb29nbGV1c2VyY29udGVudC5jb20vYS9BTG01d3UxdlYxQ1NEY19YNFhiQ1ZIU3FQRmxyYV8tNGxqU0N3QWZEd1ZiYWJRPXM5Ni1jIiwiZ2l2ZW5fbmFtZSI6IsWeYWZhayIsImZhbWlseV9uYW1lIjoiQXJhcGtpcmxpIiwibG9jYWxlIjoidHIiLCJpYXQiOjE2NjgyNDQ5MzUsImV4cCI6MTY2ODI0ODUzNX0.Lj-LeyG-d_lOteLRQtzHueZpC4ros99Gr59YAyqj_Ru7U5Dsf7lXmF6UC1P5Womi0n354X-ZlvDp_ar3Iz3XzUB4UNBb8ZAk7_Cs1T5PCDVRLVqqB538RXIlOY1y4Kyj7bmZsPjUjle6gdI_26a4fT8TiYB7RVkec-tQEwY_7uW6O3LSjWL_vUbUSP64_pWD20a6dImr-ic5lDJy6D6N85tBE-7vGduMRhLUyscfxUk2PZaMgp-WPtew1K0taz3ZJ98KUOuuSNQp8fJlArWfTFsqxNCtJbo85kBpgpS3BWSpJSeltyEodu3kge_eFlRW-OeEbOa8CbGPNwNXS5sDNg";
        //    //var auth = await p.SignInWithCustomTokenAsync(token);
        //    //var auth = await FirebaseUtils.provider.SignInWithGoogleIdTokenAsync(token);
        //    var credential = GoogleProvider.GetCredential(token);
        //    var fbUser = await FirebaseUtils.client.SignInWithCredentialAsync(credential);
        //    return fbUser;
        //}

        [HttpGet("appointmentNotificationTest/{id}")]
        public void AppointmentNotificationTest(int id)
        {
            _ = Notifications.AppoinmentNotify(id);
        }

        [HttpGet("defaultdefs")]
        public async Task<string> DefaultDefs()
        {
            int firmId = 1;
            var defs = await FirmController.GetDefaultDefinitions(firmId);
            _context.Definitions.AddRange(defs);
            await _context.SaveChangesAsync();
            return "OK";
        }
    }
}
