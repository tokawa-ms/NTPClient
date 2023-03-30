namespace NTPClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var windowsntp = NtpClient.GetTimeFromNTP("time.windows.com");
            var nictntp = NtpClient.GetTimeFromNTP("ntp.nict.jp");
            Console.WriteLine("time.windows.com : " + windowsntp);
            Console.WriteLine("ntp.nict.jp : " + nictntp);
        }
    }
}