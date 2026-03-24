using UnityEngine;
using System.Diagnostics;

public class PaymentOpener : MonoBehaviour
{
    [SerializeField] private int callbackPort = 5001;

    private PaymentListener paymentListener;

    void Awake()
    {
        paymentListener = FindObjectOfType<PaymentListener>();
        if (paymentListener != null)
        {
            callbackPort = paymentListener.ListenPort;
        }
    }

    public void Buy100() => OpenPayment("0.01", "100");
    public void Buy200() => OpenPayment("0.02", "200");
    public void Buy300() => OpenPayment("0.03", "300");

    void OpenPayment(string eth, string frag)
    {
        string url = $"http://localhost:3000/pay.html?eth={eth}&frag={frag}&cbPort={callbackPort}";

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}