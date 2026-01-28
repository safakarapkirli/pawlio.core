using System.Text;
using Pawlio.Models;

public static class SendSMS
{
    public static void SendOTPCode(int userId, string phoneNumber, int code, string signature)
    {
        Send(userId, phoneNumber, $"{code.ToString("00000")} doğrulama kodu ile işleminize devam edebilirsiniz. HIZLIIS");
    }

    public static string Send(int userId, string phoneNumber, string message)
    {
        try
        {
            if (phoneNumber == "905555555555") phoneNumber = "905552709430";
            if (phoneNumber == "905550000000") phoneNumber = "905552709430";
            return InteraktifSms(userId, phoneNumber, message);
        }
        catch (Exception x)
        {
            return "Hata:" + x.Message;
        }
    }

    private static string InteraktifSms(int userId, string phoneNumber, string message)
    {
        if (phoneNumber.StartsWith("90")) phoneNumber = phoneNumber.Remove(0, 2);
        if (phoneNumber.StartsWith("0")) phoneNumber = phoneNumber.Remove(0, 1);

        string req = $@"<?xml version='1.0' encoding='UTF-8'?>
<sms>
<username>safakarapkirli</username>
<password>cae7e34467cb1c7d6921c3c44b119039</password>
<header>ADVISOR LTD</header>
<validity>2880</validity>
<message>
    <gsm><no>{phoneNumber}</no></gsm>
    <msg><![CDATA[{message}]]></msg>
</message>
</sms>";

        try
        {
            var task = PostData("http://panel.1sms.com.tr:8080/api/smspost/v1", req);
            task.Wait();
            var res = task.Result;

            if (res.StartsWith("00")) res = "0";
            var succes = res == "0";
            if (succes) return "";
            return "Error:" + res;
        }
        catch (Exception x) { return x.Message; }
    }

    private async static Task<string> PostData(string address, string postData)
    {
        using (var client = new HttpClient())
        {
            var response = await client.PostAsync(address, new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded"));
            return await response.Content.ReadAsStringAsync();
        }
    }
}