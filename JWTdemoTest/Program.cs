using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Text;

string jsonString = readJsonFile(System.Environment.CurrentDirectory + @"\json1.json");
Console.WriteLine(jsonString);

//SendLineNotifyAlert();
Console.WriteLine("Hello, World!");



async void SendLineNotifyAlert(string Data)
{
    string dataString = Data;
    HttpClient client = new HttpClient();
    client.BaseAddress = new Uri($"http://10.20.16.153:2234/LineNotify/SendMessage/NoProxy");

    HttpContent contentPost = new StringContent(dataString, Encoding.UTF8, "application/json");

    HttpResponseMessage response = client.PostAsync("test", contentPost).GetAwaiter().GetResult();

}
string readJsonFile(string filePath)
{
    StreamReader r = new StreamReader(filePath, Encoding.UTF8);
    string jsonString = r.ReadToEnd();
    r.Dispose();
    return jsonString;
}