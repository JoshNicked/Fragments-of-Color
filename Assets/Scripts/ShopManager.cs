using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("References")]
    public GameTimer gameTimer;
    public CurrencyManager currencyManager;

    [Header("Shop Buttons")]
    public Button buttonBuy30s;
    public Button buttonBuy60s;
    public Button buttonBuy120s;

    [Header("UI Feedback")]
    public GameObject processingPanel;
    public TMP_Text statusText;

    [Header("Purchase Result Panels")]
    public GameObject panel_time30;
    public GameObject panel_time1mins;
    public GameObject panel_time2mins;

    [Header("Panel UI Elements")]
    public TMP_Text panel_time30_fragmentsText;
    public TMP_Text panel_time30_timerText;
    public TMP_Text panel_time1mins_fragmentsText;
    public TMP_Text panel_time1mins_timerText;
    public TMP_Text panel_time2mins_fragmentsText;
    public TMP_Text panel_time2mins_timerText;

    [Header("Purchase Settings")]
    public bool allowBlockchainFallback = false; // set true if you want fallback to external payment

    private bool isProcessing = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("ShopManager already exists. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (currencyManager == null)
            currencyManager = CurrencyManager.Instance;

        if (gameTimer == null)
            gameTimer = FindObjectOfType<GameTimer>();

        UpdateUiState(false);
        HideAllResultPanels();
    }

    public void OnBuy30Seconds()
    {
        StartCoroutine(ProcessTimePurchase(30, 50, panel_time30));
    }

    public void OnBuy60Seconds()
    {
        StartCoroutine(ProcessTimePurchase(60, 90, panel_time1mins));
    }

    public void OnBuy120Seconds()
    {
        StartCoroutine(ProcessTimePurchase(120, 167, panel_time2mins));
    }

    private IEnumerator ProcessTimePurchase(int seconds, int cost, GameObject resultPanel)
    {
        if (isProcessing)
            yield break;

        isProcessing = true;
        UpdateUiState(true);
        SetStatus("Processing purchase...");

        yield return null;

        if (currencyManager == null || gameTimer == null)
        {
            SetStatus("Cannot complete purchase: missing manager refs.");
            isProcessing = false;
            UpdateUiState(false);
            yield break;
        }

        if (!currencyManager.SpendFragments(cost))
        {
            SetStatus($"Purchase failed: not enough fragments (cost {cost}).");

            if (allowBlockchainFallback)
            {
                RequestBlockchainFallback(seconds, cost);
            }

            isProcessing = false;
            UpdateUiState(false);
            yield break;
        }

        gameTimer.AddTime(seconds);
        SetStatus($"Success! +{seconds} seconds added.");

        ShowResultPanel(resultPanel);

        isProcessing = false;
        UpdateUiState(false);

        yield return new WaitForSeconds(1f);
        SetStatus("");
    }

    private void UpdateUiState(bool processing)
    {
        if (processingPanel != null)
            processingPanel.SetActive(processing);

        if (buttonBuy30s != null)
            buttonBuy30s.interactable = !processing;

        if (buttonBuy60s != null)
            buttonBuy60s.interactable = !processing;

        if (buttonBuy120s != null)
            buttonBuy120s.interactable = !processing;
    }

    private void ShowResultPanel(GameObject resultPanel)
    {
        HideAllResultPanels();

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            UpdatePanelUI(resultPanel);
            Debug.Log("Showing result panel: " + resultPanel.name);
        }
    }

    private void UpdatePanelUI(GameObject resultPanel)
    {
        int currentFragments = currencyManager != null ? currencyManager.GetFragments() : 0;
        string timerDisplay = FormatTime(gameTimer != null ? gameTimer.timeRemaining : 0f);

        if (resultPanel == panel_time30)
        {
            if (panel_time30_fragmentsText != null)
                panel_time30_fragmentsText.text = "Color Fragments: " + currentFragments;
            if (panel_time30_timerText != null)
                panel_time30_timerText.text = "Time: " + timerDisplay;
        }
        else if (resultPanel == panel_time1mins)
        {
            if (panel_time1mins_fragmentsText != null)
                panel_time1mins_fragmentsText.text = "Color Fragments: " + currentFragments;
            if (panel_time1mins_timerText != null)
                panel_time1mins_timerText.text = "Time: " + timerDisplay;
        }
        else if (resultPanel == panel_time2mins)
        {
            if (panel_time2mins_fragmentsText != null)
                panel_time2mins_fragmentsText.text = "Color Fragments: " + currentFragments;
            if (panel_time2mins_timerText != null)
                panel_time2mins_timerText.text = "Time: " + timerDisplay;
        }
    }

    private string FormatTime(float seconds)
    {
        float clampedTime = Mathf.Max(0f, seconds);
        int minutes = Mathf.FloorToInt(clampedTime / 60);
        int secs = Mathf.FloorToInt(clampedTime % 60);
        return string.Format("{0:00}:{1:00}", minutes, secs);
    }

    private void HideAllResultPanels()
    {
        if (panel_time30 != null)
            panel_time30.SetActive(false);

        if (panel_time1mins != null)
            panel_time1mins.SetActive(false);

        if (panel_time2mins != null)
            panel_time2mins.SetActive(false);
    }

    public void OnContinueButtonClicked()
    {
        HideAllResultPanels();
        ResumeGame();
    }

    private void ResumeGame()
    {
        if (gameTimer == null)
            gameTimer = FindObjectOfType<GameTimer>();

        if (gameTimer != null)
        {
            gameTimer.SaveTime();
            float savedTime = PlayerPrefs.GetFloat("SavedTimeRemaining", 0f);
            Debug.Log("Time saved to PlayerPrefs: " + savedTime + " seconds (" + FormatTime(savedTime) + ")");
            Debug.Log("[EDITABLE] You can modify this value in code: PlayerPrefs.SetFloat(\"SavedTimeRemaining\", newValue);");
        }

        MenuManage menu = FindObjectOfType<MenuManage>();
        if (menu != null)
        {
            menu.ResumeGame();
            Debug.Log("Game resumed via ShopManager");
        }
        else
        {
            Debug.LogWarning("MenuManage not found to resume game");
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        Debug.Log("ShopManager status: " + message);
    }

    public void OnBlockchainTimePurchaseSuccess(int seconds, int paymentAmount)
    {
        if (seconds <= 0)
        {
            SetStatus("Blockchain purchase callback invalid time amount.");
            return;
        }

        if (gameTimer == null)
            gameTimer = FindObjectOfType<GameTimer>();

        if (gameTimer != null)
        {
            gameTimer.AddTime(seconds);
        }

        if (paymentAmount > 0 && currencyManager != null)
        {
            currencyManager.AddFragments(paymentAmount);
        }

        SetStatus($"Blockchain payment success: +{seconds}s time and +{paymentAmount} fragments.");
        UpdateUiState(false);

        StartCoroutine(ClearStatusAfterDelay(2f));
    }

    private IEnumerator ClearStatusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetStatus("");
    }

    public void RequestBlockchainFallback(int seconds, int cost)
    {
        SetStatus("Not enough fragments. Initiate blockchain purchase via existing payment provider.");
        UpdateUiState(true);

        // TODO: Wire this to your external blockchain purchase process
        Debug.Log($"Blockchain fallback requested for {seconds}s (cost {cost} fragments equivalent)");

        // Leave processing state active until callback from PaymentListener
    }
}