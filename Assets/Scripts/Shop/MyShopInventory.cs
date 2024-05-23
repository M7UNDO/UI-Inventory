using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyShopInventory : MonoBehaviour
{
    [SerializeField] private List<ShopItemClass> shopItems = new List<ShopItemClass>();
    [SerializeField] private GameObject shopSlotHolder;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private PlayerCurrency playerCurrency;

    private GameObject[] shopSlots;

    private void Start()
    {


        shopSlots = new GameObject[shopSlotHolder.transform.childCount];

        for (int i = 0; i < shopSlots.Length; i++)
        {
            shopSlots[i] = shopSlotHolder.transform.GetChild(i).gameObject;
            int index = i; //for the button click listener instead of my having to do it in Hierachy
            shopSlots[i].GetComponent<Button>().onClick.AddListener(() => PurchaseItem(index));
        }

        RefreshShopUI();
    }

    public void PurchaseItem(int index)
    {
        ShopItemClass shopItem = shopItems[index];
        if (shopItem.quantity > 0 && playerCurrency.amount >= shopItem.price)
        {
            if (inventoryManager.Add(shopItem.item, 1))
            {
                shopItem.quantity--;
                playerCurrency.amount -= shopItem.price;
                RefreshShopUI();
                inventoryManager.RefreshUI();
            }
        }
        else
        {
            Debug.Log("Not enough currency or item out of stock.");
        }
    }

    public void SellItem(SlotClass slot)
    {
        ShopItemClass shopItem = shopItems.Find(item => item.item == slot.item);
        if (shopItem != null)
        {
            shopItem.quantity += slot.quantity;
            playerCurrency.amount += slot.quantity * shopItem.price;
            slot.Clear();
            RefreshShopUI();
        }
        else
        {
            Debug.Log("Item not found in shop inventory.");
        }
    }


    public void RefreshShopUI()
    {
        for (int i = 0; i < shopSlots.Length; i++)
        {
            RefreshSlot(shopItems[i], shopSlots[i].transform);
        }
    }

    private void RefreshSlot(ShopItemClass shopItem, Transform slotUI)
    {
        if (shopItem.item == null)
        {
            slotUI.GetChild(0).GetComponent<Image>().sprite = null;
            slotUI.GetChild(0).GetComponent<Image>().enabled = false;
            slotUI.GetChild(1).GetComponent<Text>().text = "";
            slotUI.GetChild(2).GetComponent<Text>().text = "";
            slotUI.GetChild(3).GetComponent<Text>().text = "";
            return;
        }

        slotUI.GetChild(0).GetComponent<Image>().enabled = true;
        slotUI.GetChild(0).GetComponent<Image>().sprite = shopItem.item.itemIcon;
        slotUI.GetChild(1).GetComponent<Text>().text = shopItem.item.itemName;
        slotUI.GetChild(2).GetComponent<Text>().text = "R: " + shopItem.price;
        slotUI.GetChild(3).GetComponent<Text>().text = "Q: " + shopItem.quantity;
    }
}

[System.Serializable]
public class ShopItemClass
{
    public ItemClass item;
    public int quantity;
    public int price;
}
