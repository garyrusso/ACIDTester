using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Specialized;
using System.Xml;
using System.Web;
using System.Xml.Linq;

namespace acid
{
    class Program
    {
        private const string hostUri                      = "http://glm-ml-dev1.amers2.ciscloud:8030/v1/resources";
        private const string docRequestEndpoint           = "/document?rs:uri=";
        private const string commitTransRequestEndpoint   = "/transactions?rs:result=commit&rs:txid=";
        private const string rollbackTransRequestEndpoint = "/transactions?rs:result=rollback&rs:txid=";
        private const string createTransRequestEndpoint   = "/transactions";

        static void Main(string[] args)
        {
            string action, docUri, txId;
            string response = "";

            if (args == null || args.Length < 2)
            {
                Console.WriteLine("requires at least 2 params:");
                Console.WriteLine("params: action, file, docUri, txid");
                Console.WriteLine("action params: (get | put | post | trans)");
                Console.WriteLine("  get   params: docUri, txid (optional)");
                Console.WriteLine("  put   params: file, docUri, txid (optional)");
                Console.WriteLine("  post  params: file, docUri, txid (optional)");
                Console.WriteLine("  trans params: action (create | commit | rollback), txid (not needed for create)");
            }
            else
            {
                action = args[0];

                switch (action)
                {
                    case "get":
                    {
                        docUri = args[1];

                        if (args.Length > 2)
                        {
                            txId = args[2];
                            display("GET Request using: " + action + " | " + docUri + " | " + txId);
                        }
                        else
                        {
                            txId = "";
                            display("GET Request using: " + action + " | " + docUri);
                        }

                        response = getRequest(docUri, txId);
                        break;
                    }

                    case "put":
                    {
                        string[] putPostArgs = new string[args.Length];

                        putPostArgs = getArgs(action, args);

                        response = putRequest(putPostArgs);

                        break;
                    }

                    case "post":
                    {
                        string[] putPostArgs = getArgs(action, args);

                        response = "Not yet implemented";

                        break;
                    }

                    case "trans":
                    {
                        string[] putPostArgs = getArgs(action, args);

                        response = transactionRequest(putPostArgs);

                        break;
                    }

                    default:
                    {
                        response = "Invalid action";
                        break;
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("Result: ");
            Console.WriteLine();
            Console.WriteLine(response);
        }

        static string[] getArgs(string action, string[] args)
        {
            string[] items = new string[args.Length];
            string output = "";

            int i = 0;

            foreach (string arg in args)
            {
                if (arg != args[0])
                {
                    items[i++] = arg;
                }
            }

            output = action.ToUpper() + " Request using: " + action + ", ";

            foreach (string item in items)
            {
                if (item != null)
                {
                    if (item == args[args.Length - 1])
                        output += item;
                    else
                        output += item + ", ";
                }
            }

            display(output);

            return items;
        }

        static void display(string output)
        {
            Console.WriteLine();
            Console.WriteLine(output);
            Console.WriteLine();
        }

        static string getRequest(string docUri, string txId)
        {
            string url = hostUri + docRequestEndpoint + docUri;

            string response = "";

            if (txId.Length > 0)
                url += "&rs:txid=" + txId;

            Console.WriteLine("Request Url: " + url);
            Console.WriteLine();

            try
            {
                using (var wb = new WebClient())
                {
                    response = wb.DownloadString(url);
                }
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine("Exception was thrown: " + ex.Message);
            }

            return response;
        }

        static string putRequest(string[] putArgs)
        {
            string file = "", txId = "", docUri = "", url = "";
            string response = "";

            if (putArgs[0].Length > 0)
                file = putArgs[0];

            if (putArgs[1].Length > 0)
                docUri = putArgs[1];

            if (putArgs[2] == null)
            {
                txId = "";
                url = hostUri + docRequestEndpoint + docUri;
            }
            else
            {
                txId = putArgs[2];
                url = hostUri + docRequestEndpoint + docUri + "&rs:txid=" + txId;
            }

            XmlDocument doc = new XmlDocument();

            XElement xDoc = null;

            if (file.Length <= 0)
                return "no file was specified";

            xDoc = XElement.Load(file);
            response = xDoc.ToString();

            byte[] xmlBytes = Encoding.ASCII.GetBytes(xDoc.ToString());

            try
            {
                using (var wb = new WebClient())
                {
                    wb.Headers[HttpRequestHeader.ContentType] = "text/xml";

                    byte[] byteResponse = wb.UploadData(url, "PUT", xmlBytes);

                    //response = Encoding.ASCII.GetString(byteResponse);
                }
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine("Exception was thrown: " + ex.Message);
            }

            return response;
        }

        static string transactionRequest(string[] transArgs)
        {
            string action = "", txId = "", url = "";
            string response = "";

            if (transArgs[0].Length > 0)
                action = transArgs[0];

            if (transArgs[1] == null)
            {
                if (action == "create")
                {
                    url = hostUri + createTransRequestEndpoint;
                }
            }
            else
            {
                txId = transArgs[1];

                if (action == "commit")
                {
                    url = hostUri + commitTransRequestEndpoint + txId;
                }
                else
                if (action == "rollback")
                {
                    url = hostUri + rollbackTransRequestEndpoint + txId;
                }
                else
                   response = "Invalid Transaction Request";
            }

            Console.WriteLine("transactionRequest: " + action + " | url: " + url + " | txid: " + txId);

            if (action == "create")
                response = transactionCreateRequest(url);
            else
                if ((action == "commit") || (action == "rollback"))
                    response = transactionPostRequest(url);
                else
                    response = "Invalid Transaction Request";

            return response;
        }

        static string transactionPostRequest(string url)
        {
            string strPost = "";
            string response = "";

            try
            {
                using (var wb = new WebClient())
                {
                    //wb.Headers[HttpRequestHeader.ContentType] = "text/xml";
                    wb.Encoding = System.Text.Encoding.UTF8;

                    response = wb.UploadString(url, "POST", strPost);

                    //string resultString = Encoding.ASCII.GetString(response);
                }
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine("Exception was thrown: " + ex.Message);
            }

            return response;
        }

        static string transactionCreateRequest(string url)
        {
            string strPost = "";
            string response = "";

            try
            {
                using (var wb = new WebClient())
                {
                    wb.Encoding = System.Text.Encoding.UTF8;

                    response = wb.UploadString(url, "POST", strPost);
                }
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine("Exception was thrown: " + ex.Message);
            }

            return response;
        }

        static string postRequest(string[] args)
        {
            string response = putRequest(args);

            return response;
        }
    }
}
