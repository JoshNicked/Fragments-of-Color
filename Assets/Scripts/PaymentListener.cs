using UnityEngine;
using System.Net;
using System.Text;
using System.Threading;

public class PaymentListener : MonoBehaviour
{
    HttpListener listener;

    void Start()
    {
        Thread listenerThread = new Thread(StartServer);
        listenerThread.Start();
    }

    void StartServer()
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5000/");
        listener.Start();

        while (true)
        {
            var context = listener.GetContext();
            var request = context.Request;
            var response = context.Response;

            if (request.RawUrl.Contains("paymentSuccess"))
            {
                UnityEngine.Debug.Log("Payment Received!");

                UnityMainThread.Execute(() =>
                {
                    CurrencyManager.Instance.AddFragments(100);
                });
            }

            byte[] buffer = Encoding.UTF8.GetBytes("OK");
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }
    }
}