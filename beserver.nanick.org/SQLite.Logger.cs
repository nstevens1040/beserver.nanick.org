namespace SQLite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Data.SQLite;
    using System.Net;
    using Newtonsoft.Json;
    using System.IO;
    using System.Text.RegularExpressions;
    public class Context__
    {
        public string[] AcceptTypes { get; set; }
        public System.Text.Encoding ContentEncoding { get; set; }
        public long ContentLength64 { get; set; }
        public string ContentType { get; set; }
        public System.Net.CookieCollection Cookies { get; set; }
        public bool HasEntityBody { get; set; }
        public System.Collections.Specialized.NameValueCollection Headers { get; set; }
        public string HttpMethod { get; set; }
        public string InputStream { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsLocal { get; set; }
        public bool IsSecureConnection { get; set; }
        public bool IsWebSocketRequest { get; set; }
        public bool KeepAlive { get; set; }
        public string LocalEndPoint { get; set; }
        public System.Version ProtocolVersion { get; set; }
        public System.Collections.Specialized.NameValueCollection QueryString { get; set; }
        public string RawUrl { get; set; }
        public string RemoteEndPoint { get; set; }
        public System.Guid RequestTraceIdentifier { get; set; }
        public string ServiceName { get; set; }
        public System.Net.TransportContext TransportContext { get; set; }
        public Uri Url { get; set; }
        public Uri UrlReferrer { get; set; }
        public string UserAgent { get; set; }
        public string UserHostAddress { get; set; }
        public string UserHostName { get; set; }
        public string[] UserLanguages { get; set; }
        public Context__()
        {
        }
        public Context__(HttpListenerRequest request)
        {
            this.AcceptTypes = request.AcceptTypes;
            this.ContentEncoding = request.ContentEncoding;
            this.ContentLength64 = request.ContentLength64;
            this.ContentType = request.ContentType;
            this.Cookies = request.Cookies;
            this.HasEntityBody = request.HasEntityBody;
            this.Headers = request.Headers;
            this.HttpMethod = request.HttpMethod;
            this.InputStream = new System.IO.StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd();
            this.IsAuthenticated = request.IsAuthenticated;
            this.IsLocal = request.IsLocal;
            this.IsSecureConnection = request.IsSecureConnection;
            this.IsWebSocketRequest = request.IsWebSocketRequest;
            this.KeepAlive = request.KeepAlive;
            this.LocalEndPoint = request.LocalEndPoint.Address.ToString() + ':' + request.LocalEndPoint.Port;
            this.ProtocolVersion = request.ProtocolVersion;
            this.QueryString = request.QueryString;
            this.RawUrl = request.RawUrl;
            this.RemoteEndPoint = request.RemoteEndPoint.Address.ToString() + ':' + request.RemoteEndPoint.Port;
            this.RequestTraceIdentifier = request.RequestTraceIdentifier;
            this.ServiceName = request.ServiceName;
            this.TransportContext = request.TransportContext;
            this.Url = request.Url;
            this.UrlReferrer = request.UrlReferrer;
            this.UserAgent = request.UserAgent;
            this.UserHostAddress = request.UserHostAddress;
            this.UserHostName = request.UserHostName;
            this.UserLanguages = request.UserLanguages;
        }
    }
    public class Logger
    {
        public Logger()
        {
        }
        public static string home_ = String.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME")) ? Environment.GetEnvironmentVariable("USERPROFILE") : Environment.GetEnvironmentVariable("HOME");
        public SQLiteConnection sqliteConnection;
        public SQLiteCommand sqliteCommand;
        public string connection_string = @"Data Source=" + home_ + Path.VolumeSeparatorChar + @"WebServerLog.sqlite";
        public Double Epoch()
        {
            return Math.Round((DateTime.UtcNow - DateTime.Parse("1970-01-01")).TotalSeconds);
        }
        public System.Data.DataTable ExportDataTable()
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            this.sqliteCommand = new SQLiteCommand(this.sqliteConnection);
            this.sqliteCommand.CommandText = "SELECT * FROM WebServerLog";
            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(this.sqliteCommand))
            {
                System.Data.DataSet dataset = new System.Data.DataSet();
                try
                {
                    adapter.Fill(dataset);
                    if (dataset.Tables.Count > 0)
                    {
                        dt = dataset.Tables[0];
                    }
                }
                catch (Exception e)
                {
                    string result = e.Message;
                }
            }
            return dt;
        }
        public bool CheckForTable()
        {
            bool table_exists = false;
            this.sqliteCommand = new SQLiteCommand(this.sqliteConnection);
            this.sqliteCommand.CommandText = "SELECT * FROM WebServerLog";
            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(this.sqliteCommand))
            {
                System.Data.DataSet dataset = new System.Data.DataSet();
                try
                {
                    adapter.Fill(dataset);
                    if (dataset.Tables.Count > 0)
                    {
                        table_exists = true;
                    }
                }
                catch (Exception e)
                {
                    table_exists = false;
                    string msg = e.Message;
                }
            }
            return table_exists;
        }
        public void SQLiteConnect()
        {
            this.sqliteConnection = new SQLiteConnection() { ConnectionString = this.connection_string };
            this.sqliteConnection.Open();
            if (!CheckForTable())
            {
                this.sqliteCommand = new SQLiteCommand(this.sqliteConnection);
                this.sqliteCommand.CommandText = "CREATE TABLE WebServerLog (\n    timestamp INTEGER,\n    request_json TEXT(4000),\n    source_ip_address TEXT(4000),\n    method TEXT(4000),\n    absolute_uri TEXT(4000),\n    content_length BIGINT(255),\n    referer TEXT(4000),\n    cookies INT,\n    is_auth BIT(1)\n);";
                this.sqliteCommand.ExecuteNonQuery();
            }
        }
        public void SQLiteInsert(HttpListenerContext context)
        {
            string timestamp_string = Epoch().ToString();
            Context__ context_obj = new Context__(context.Request);
            string request_json = JsonConvert.SerializeObject(context_obj).Replace((Char)39, (Char)34);
            string source_ip = context.Request.RemoteEndPoint.Address.ToString();
            string method = context.Request.HttpMethod;
            string req_uri = context.Request.Url.AbsoluteUri;
            string req_len = context.Request.ContentLength64.ToString();
            string referer = String.Empty;
            if (context.Request.UrlReferrer != null)
            {
                referer = context.Request.UrlReferrer.AbsoluteUri;
            }
            int is_auth = 0;
            if (context.Request.IsAuthenticated)
            {
                is_auth = 1;
            }
            string insert1 = "INSERT INTO WebServerLog(timestamp,request_json,source_ip_address,method,absolute_uri,content_length,referer,cookies,is_auth)";
            string insert2 = insert1 + "VALUES(" + timestamp_string + ", '" + request_json + "', '" + source_ip + "', '" + method + "', '" + req_uri + "', '" + req_len + "', '" + referer + "', '" + context.Request.Cookies.Count.ToString() + "', '" + is_auth.ToString() + "');";
            this.sqliteCommand.CommandText = insert2;
            this.sqliteCommand.ExecuteNonQuery();
        }
    }
}
