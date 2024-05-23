using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private List<CraftingRecipeClass> craftingRecipes = new List<CraftingRecipeClass>();

    [SerializeField] private GameObject itemCursor;

    [SerializeField] private GameObject slotHolder;
  
    [SerializeField] private ItemClass itemToAdd;
    [SerializeField] private ItemClass itemToRemove;

    [SerializeField] private SlotClass[] startingItems;

    private SlotClass[] items;

    private GameObject[] slots;


    private SlotClass movingSlot;
    private SlotClass tempSlot;
    private SlotClass originalSlot;
    bool isMovingItem;


    [SerializeField] private MyShopInventory myShopInventory;
    [SerializeField] private ChestInventory chestInventory; 
    private SlotClass movingSlotChest;

    private void Start()
    {
        slots = new GameObject[slotHolder.transform.childCount];
        items = new SlotClass[slots.Length];



        for (int i = 0; i < items.Length; i++)
            items[i] = new SlotClass();
        
        for (int i = 0; i < slotHolder.transform.childCount; i++)
            slots[i] = slotHolder.transform.GetChild(i).gameObject;
       
        for (int i = 0; i < startingItems.Length; i++)
            Add(startingItems[i].item, startingItems[i].quantity);

        RefreshUI();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)) 
            Craft(craftingRecipes[0]);

        itemCursor.SetActive(isMovingItem);
        itemCursor.transform.position = Input.mousePosition;
        if (isMovingItem)
            itemCursor.GetComponent<Image>().sprite = movingSlot.item.itemIcon;

        if (Input.GetMouseButtonDown(0)) 
        {
            
           
            if (isMovingItem)
                
                EndItemMove();
            else
                BeginItemMove();
        }
        else if (Input.GetMouseButtonDown(1)) 
        {
            
            if (isMovingItem)
            {
                
                EndItemMove_Single();
            }
            else
                BeginItemMove_Half();
        }
        else if (Input.GetKeyDown(KeyCode.S)) // Handle selling
        {
            if (isMovingItem)
            {
                myShopInventory.SellItem(movingSlot);
                isMovingItem = false;
                RefreshUI();
                chestInventory.RefreshChestUI();
                myShopInventory.RefreshShopUI();
            }
        }

    }
    private void Craft(CraftingRecipeClass recipe)
    {
        if (recipe.CanCraft(this))
            recipe.Craft(this);
        else
            //show error msg
            Debug.Log("Can't craft that item!");
    }

    #region Inventory Utils
    public void RefreshUI()
    {
        for (int i = 0; i < slots.Length; i++)
            RefreshSlot(items[i], slots[i].transform);

    }
    private void RefreshSlot(SlotClass slot, Transform slotUI)
    {
        if (slot.item is null)
        {
            //item is null in the slot
            slotUI.transform.GetChild(0).GetComponent<Image>().sprite = null;
            slotUI.transform.GetChild(0).GetComponent<Image>().enabled = false;
            slotUI.transform.GetChild(1).GetComponent<Text>().text = "";
            return;
        }

        //item exists in slot
        slotUI.transform.GetChild(0).GetComponent<Image>().enabled = true;
        slotUI.transform.GetChild(0).GetComponent<Image>().sprite = slot.item.itemIcon;

        if (slot.item.isStackable)
            slotUI.transform.GetChild(1).GetComponent<Text>().text = slot.quantity + "";
        else
            slotUI.transform.GetChild(1).GetComponent<Text>().text = "";
    }

    public bool Add(ItemClass item, int quantity)
    {
        
        SlotClass slot = Contains(item);

        if (slot != null)
        {
            
            var quantityCanAdd = slot.item.stackSize - slot.quantity;
            var quantityToAdd = Mathf.Clamp(quantity, 0, quantityCanAdd);

            var remainder = quantity - quantityCanAdd; 

            slot.AddQuantity(quantityToAdd);
            if (remainder > 0) Add(item, remainder);
        }
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].item == null) 
                {
                    var quantityCanAdd = item.stackSize - items[i].quantity; 
                    var quantityToAdd = Mathf.Clamp(quantity, 0, quantityCanAdd);

                    var remainder = quantity - quantityCanAdd; 

                    items[i].AddItem(item, quantityToAdd);
                    if (remainder > 0) Add(item, remainder);

                 
                    break;
                }
            }
        }

        RefreshUI();
        return true;
    }
    public bool Remove(ItemClass item, int quantity = 1)
    {
        // items.Remove(item);
        SlotClass temp = Contains(item);
        if (temp != null)
        {
            if (temp.quantity > 1)
                temp.SubQuantity(quantity);
            else
            {
                int slotToRemoveIndex = 0;

                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].item == item)
                    {
                        slotToRemoveIndex = i;
                        break;
                    }
                }

                items[slotToRemoveIndex].Clear();
            }
        }
        else
        {
            return false;
        }

        RefreshUI();
        return true;
    }
    public void UseSelected()
    {

        RefreshUI();
    }
    public bool isFull()
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].item == null)
                return false;
        }
        return true;
    }
    public SlotClass Contains(ItemClass item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].item == item && items[i].item.isStackable && items[i].quantity < items[i].item.stackSize)
                return items[i];
        }

        return null;
    }
    public bool Contains(ItemClass item, int quantity)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].item == item && items[i].quantity >= quantity)
                return true;
        }

        return false;
    }
    #endregion Inventoy Utils

    #region Moving Stuff
    private bool BeginItemMove()
    {
        originalSlot = GetClosestSlot();
        if (originalSlot == null || originalSlot.item == null)
        {
            originalSlot = chestInventory.GetClosestChestSlot();
            if (originalSlot == null || originalSlot.item == null)
                return false; // Remember Mfundo!there is no item to move!
        }

        movingSlot = new SlotClass(originalSlot);
        originalSlot.Clear();
        isMovingItem = true;
        RefreshUI();
        chestInventory.RefreshChestUI();
        return true;
    }
    private bool BeginItemMove_Half()
    {
        originalSlot = GetClosestSlot();
        if (originalSlot == null || originalSlot.item == null)
            return false; //Remember Mfundo! there is not item to move!

        movingSlot = new SlotClass(originalSlot.item, Mathf.CeilToInt(originalSlot.quantity / 2f));
        originalSlot.SubQuantity(Mathf.CeilToInt(originalSlot.quantity / 2f));
        if (originalSlot.quantity == 0)
            originalSlot.Clear();

        isMovingItem = true;
        RefreshUI();
        return true;
    }
    private bool EndItemMove()
    {
        originalSlot = GetClosestSlot();
        SlotClass chestSlot = chestInventory.GetClosestChestSlot();

        if (originalSlot == null && chestSlot == null)
        {
            Add(movingSlot.item, movingSlot.quantity);
            movingSlot.Clear();
        }
        else if (originalSlot != null)
        {
            AddToSlotOrChest(originalSlot);
        }
        else if (chestSlot != null)
        {
            AddToSlotOrChest(chestSlot);
        }

        isMovingItem = false;
        RefreshUI();
        chestInventory.RefreshChestUI();
        return true;
    }

    private void AddToSlotOrChest(SlotClass targetSlot)
    {
        if (targetSlot.item != null)
        {
            if (targetSlot.item == movingSlot.item && targetSlot.item.isStackable && targetSlot.quantity < targetSlot.item.stackSize)
            {
                var quantityCanAdd = targetSlot.item.stackSize - targetSlot.quantity;
                var quantityToAdd = Mathf.Clamp(movingSlot.quantity, 0, quantityCanAdd);
                var remainder = movingSlot.quantity - quantityToAdd;

                targetSlot.AddQuantity(quantityToAdd);
                if (remainder == 0)
                    movingSlot.Clear();
                else
                    movingSlot.SubQuantity(quantityCanAdd);
            }
            else
            {
                tempSlot = new SlotClass(targetSlot);
                targetSlot.AddItem(movingSlot.item, movingSlot.quantity);
                movingSlot.AddItem(tempSlot.item, tempSlot.quantity);
            }
        }
        else
        {
            targetSlot.AddItem(movingSlot.item, movingSlot.quantity);
            movingSlot.Clear();
        }
    }

    private bool EndItemMove_Single()
    {
        originalSlot = GetClosestSlot();
        if (originalSlot is null)
            return false;
        if (originalSlot.item is not null &&
            (originalSlot.item != movingSlot.item || originalSlot.quantity >= originalSlot.item.stackSize))
            return false;

        movingSlot.SubQuantity(1);
        if (originalSlot.item != null && originalSlot.item == movingSlot.item)
            originalSlot.AddQuantity(1);
        else
            originalSlot.AddItem(movingSlot.item, 1);

        if (movingSlot.quantity < 1)
        {
            isMovingItem = false;
            movingSlot.Clear();
        }
        else
            isMovingItem = true;

        RefreshUI();
        return true;
    }
    private SlotClass GetClosestSlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (Vector2.Distance(slots[i].transform.position, Input.mousePosition) <= 32)
                return items[i];
        }
        return null;
    }
    #endregion
}
