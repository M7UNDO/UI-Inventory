using UnityEngine;
using UnityEngine.UI;

public class CurrencyUIScript : MonoBehaviour
{
    public PlayerCurrency playerCurrency;
    public Text currencyText;

    void Update()
    {
        // Check if the playerCurrency reference is set and the currencyText is not null
        if (playerCurrency != null && currencyText != null)
        {
            // Update the text content with the player's currency amount
            currencyText.text = "Bank: R" + playerCurrency.amount.ToString();
        }
    }
}
