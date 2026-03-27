using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class PaymentListener : MonoBehaviour
{
    private static PaymentListener instance;

    [SerializeField] private int listenPort = 5001;

    private HttpListener listener;
    private Thread listenerThread;
    private volatile bool isRunning;

    public int ListenPort => listenPort;

    void Start()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("PaymentListener already exists. Destroying duplicate instance.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        isRunning = true;

        Debug.Log($"PaymentListener initializing on http://localhost:{listenPort}/");

        listenerThread = new Thread(StartServer);
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            StopServer("OnDestroy");
            instance = null;
        }
    }

    void OnApplicationQuit()
    {
        StopServer("OnApplicationQuit");
    }

    void StopServer(string reason)
    {
        if (!isRunning && listener == null)
        {
            return;
        }

        isRunning = false;
        Debug.Log($"PaymentListener stopping. Reason: {reason}");

        if (listener != null)
        {
            try
            {
                listener.Stop();
                listener.Close();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error stopping PaymentListener: " + ex.Message);
            }
            finally
            {
                listener = null;
            }
        }
    }

    void StartServer()
    {
        string prefix = $"http://localhost:{listenPort}/";

        try
        {
            listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();
            Debug.Log($"PaymentListener started and listening on {prefix}");
        }
        catch (SocketException ex)
        {
            Debug.LogError($"PaymentListener socket bind failed on {prefix}. Another process may already be using this port. " + ex.Message);
            isRunning = false;
            return;
        }
        catch (HttpListenerException ex)
        {
            Debug.LogError("PaymentListener failed to start. " + ex.Message);
            isRunning = false;
            return;
        }

        while (isRunning)
        {
            HttpListenerContext context;

            try
            {
                context = listener.GetContext();
            }
            catch (HttpListenerException)
            {
                // Thrown when listener is stopped; exit loop quietly.
                Debug.Log("PaymentListener listener loop ended due to HttpListenerException.");
                break;
            }
            catch (ObjectDisposedException)
            {
                Debug.Log("PaymentListener listener loop ended because listener was disposed.");
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError("PaymentListener listener loop error: " + ex.Message);
                break;
            }

            var request = context.Request;
            var response = context.Response;

            Debug.Log($"PaymentListener request received: {request.HttpMethod} {request.Url.AbsolutePath}{request.Url.Query}");

            if (request.Url.AbsolutePath == "/paymentSuccess")
            {
                string amountStr = request.QueryString["amount"];
                string timeStr = request.QueryString["timeSeconds"];
                Debug.Log($"PaymentListener payment callback received. amount={amountStr}, timeSeconds={timeStr}");

                int fragmentAmount = 0;
                int timeSeconds = 0;

                if (!string.IsNullOrEmpty(timeStr) && int.TryParse(timeStr, out timeSeconds) && timeSeconds > 0)
                {
                    int paymentAmount = 0;
                    if (!string.IsNullOrEmpty(amountStr))
                        int.TryParse(amountStr, out paymentAmount);

                    UnityMainThread.Execute(() =>
                    {
                        if (ShopManager.Instance != null)
                        {
                            ShopManager.Instance.OnBlockchainTimePurchaseSuccess(timeSeconds, paymentAmount);
                        }
                        else
                        {
                            Debug.LogWarning("ShopManager not found for blockchain time purchase callback.");
                        }
                    });
                }
                else if (!string.IsNullOrEmpty(amountStr) && int.TryParse(amountStr, out fragmentAmount))
                {
                    UnityMainThread.Execute(() =>
                    {
                        CurrencyManager.Instance.AddFragments(fragmentAmount);
                        Debug.Log("Fragments Added: " + fragmentAmount);
                    });
                }
                else
                {
                    Debug.LogWarning("PaymentListener payment callback had invalid or missing amount/time.");
                }
            }

            byte[] buffer = Encoding.UTF8.GetBytes("OK");
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        StopServer("Listener loop ended");
    }
}