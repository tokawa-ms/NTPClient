using System.Net.Sockets;
using System.Net;

namespace NTPClient
{
    //NTP Client Class
    internal class NtpClient
    {
        // NTP サーバーのホスト名を設定。既定では Windows の NTP サーバーを設定しておきます。
		private static string hostname = "time.windows.com";
        // NTP のポート番号は 123 なので一応ここで定数として宣言
		private const int port = 123;
        // タイムアウト時間を設定。既定ではなんとなくで 5 秒にしてある。
		private static int timeout = 5000;

        // NTP の基準となる日時の設定
		private const int BASE_YEAR = 1900;
		private const int BASE_MONTH = 1;
		private const int BASE_DAY = 1;
        
        // NTP のバージョンの設定。Ver.3 じゃないと NICT の NTP が応答を返してこない
        private const int ntp_ver = 3;

        static public DateTime GetTimeFromNTP(string remotehost)
        {
            // NTP サーバーのホスト名を設定
            hostname = remotehost;

            // NTP サーバーの基準時刻について設定
            DateTime baseDate = new DateTime(BASE_YEAR, BASE_MONTH, BASE_DAY);
            long baseTicks = baseDate.Ticks;

            // NTP サーバーからデータを受信
            byte[] d = ReceiveData_FromNTPServer();

            // NTP サーバーからの応答データを TimeTicks に変換
            long elapsedTimeTicks = ConvertNTPDataToTimeTicks(d);

            // NTP サーバーの基準時刻に応答データを足して NTP から受け取った時刻を表す tick を生成
            long ticks = baseTicks + elapsedTimeTicks;
            DateTime currentTimeFromNTP = new DateTime(ticks);

            // タイムゾーンを考慮して DateTime 型に変換して返す
            return TimeZoneInfo.ConvertTimeFromUtc(currentTimeFromNTP, TimeZoneInfo.Local);
        }

        // NTP サーバーから受け取ったバイト列を TimeTicks に変換
        static private long ConvertNTPDataToTimeTicks(byte[] d)
        {
            // NTP サーバーが返してきたデータはこんな構造らしいので…
            // http://juni24.blog.fc2.com/blog-entry-24.html
            // バイト列を数字に変換していく
            var r =
                d[40] * Math.Pow(2, (8 * 3)) +
                d[41] * Math.Pow(2, (8 * 2)) +
                d[42] * Math.Pow(2, (8 * 1)) +
                d[43] +
                d[44] * Math.Pow(2, (8 * -1)) +
                d[45] * Math.Pow(2, (8 * -2)) +
                d[46] * Math.Pow(2, (8 * -3)) +
                d[47] * Math.Pow(2, (8 * -4));
            return (long)(r * 10000000); 
        }

        // NTP サーバーからデータを受信
        static private byte[] ReceiveData_FromNTPServer()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, 0);
            using (var udpClient = new UdpClient(endPoint))
            {
                // UDP Client のタイムアウトを設定
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);

                // 送信データの準備
                byte[] sendData = new Byte[48];
                int length = sendData.Length;
                // 送信データの 1 Byte 目は NTP バージョンや動作モードの設定。
                // 3-5 bit 目は NTP Version の設定、3-5 bit 目にバージョン番号を入れるために 3 bit シフト
                sendData[0] = (byte)(ntp_ver << 3);
                // 0-2 bit 目はクライアント動作をさせるときは 0b011 を設定する
                sendData[0] += (byte)(0b011);
                // ここでデータを送ってコネクション確立
                udpClient.Send(sendData, length, hostname, port);
                // NTP サーバーからデータを受信
                byte[] r = udpClient.Receive(ref endPoint);

                // 受信したデータを応答
                return r;
            }
        }
    }
}
