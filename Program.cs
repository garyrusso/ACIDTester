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

        static void Main(string[] args)
        {
            string action, docUri, txId;

            if (args == null || args.Length < 2)
            {
                Console.WriteLine("requires at least 2 params:");
                Console.WriteLine("params: action, file, docUri, txid");
                Console.WriteLine("action params: (get | put | post | trans)");
                Console.WriteLine("  get   params: docUri, txid (optional)");
                Console.WriteLine("  put   params: file, docUri, txid (optional)");
                Console.WriteLine("  post  params: file, docUri, txid (optional)");
                Console.WriteLine("  trans params: action (commit | rollback), txid");
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

                        getRequest(docUri, txId);
                        break;
                    }

                    case "put":
                    {
                        string[] putPostArgs = new string[args.Length];

                        putPostArgs = getArgs(action, args);

                        //display("GR001: using: " + putPostArgs[0] + ", " + putPostArgs[1] + ", " + putPostArgs[2] + ", " + putPostArgs[3]);
                        //Console.ReadLine();

                        putRequest(putPostArgs);

                        break;
                    }

                    case "post":
                    {
                        string[] putPostArgs = getArgs(action, args);

                        //postRequest(putPostArgs[0], putPostArgs[1]);
                        Console.WriteLine("Not yet implemented");

                        break;
                    }

                    case "trans":
                    {
                        string[] putPostArgs = getArgs(action, args);

                        transactionRequest(putPostArgs);

                        break;
                    }

                    default:
                    {
                        Console.WriteLine("Invalid action");
                        break;
                    }
                }
            }
            //Console.ReadLine();
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

        static void getRequest(string docUri, string txId)
        {
            string url = hostUri + docRequestEndpoint + docUri;

            if (txId.Length > 0)
                url += "&rs:txid=" + txId;

            Console.WriteLine("Request Url: " + url);
            Console.WriteLine();

            try
            {
                using (var wb = new WebClient())
                {
                    var response = wb.DownloadString(url);

                    Console.WriteLine("Result: " + response);
                }
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine("Exception was thrown: " + ex.Message);
            }
        }

        static void putRequest(string[] putArgs)
        {
            string file = "", txId = "", docUri = "", url = "";

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

            if (file.Length > 0)
                xDoc = XElement.Load(file);
            else
            {
                Console.Write("no file was specified");
                return;
            }

            Console.Write("xDoc: " + xDoc.ToString());

            byte[] xmlBytes = Encoding.ASCII.GetBytes(xDoc.ToString());

            try
            {
                using (var wb = new WebClient())
                {
                    wb.Headers[HttpRequestHeader.ContentType] = "text/xml";

                    byte[] response = wb.UploadData(url, "PUT", xmlBytes);

                    string resultString = Encoding.ASCII.GetString(response);

                    Console.WriteLine("Result: " + resultString);
                }
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine("Exception was thrown: " + ex.Message);
            }
        }

        static void transactionRequest(string[] transArgs)
        {
            string action = "", txId = "", url = "";

            if (transArgs[0].Length > 0)
                action = transArgs[0];

            if (transArgs[1].Length > 0)
                txId = transArgs[1];

            if (transArgs[1] != null)
            {
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
                   Console.WriteLine("Invalid Transaction Request");
            }
            else
            {
                Console.WriteLine("Invalid Transaction Request");
            }

            Console.WriteLine("transactionRequest: " + action + " | url: " + url);

            string strPost = "rs:result=commit";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(strPost);

            try
            {
                using (var wb = new WebClient())
                {

                    //wb.Headers[HttpRequestHeader.ContentType] = "text/xml";
                    wb.Encoding = System.Text.Encoding.UTF8;

                    var response = wb.UploadString(url, strPost);

                    //string resultString = Encoding.ASCII.GetString(response);

                    Console.WriteLine("Result: " + response);
                }
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine("Exception was thrown: " + ex.Message);
            }
        }

        static void postRequest(string[] args)
        {
            putRequest(args);
        }
    }
}
