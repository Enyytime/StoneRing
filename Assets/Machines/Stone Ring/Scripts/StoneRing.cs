using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIButton = UnityEngine.UI.Button;

public class StoneRing : MonoBehaviour
{
    [System.Serializable]
    public class ButtonPosition
    {
        public GameObject buttonObject;
        public Transform slot;
        public UIButton Button => buttonObject.GetComponent<UIButton>();
    }
    
    public Machine machine;
    
    [System.Serializable]
    public class ButtonAnswer
    {
        public string buttonName;
        public Transform correctSlot;
    }
    
    public ButtonPosition[] startingButtonPositions;
    public UIButton[] buttons;
    public ButtonAnswer[] buttonAnswers;
    public UIButton checkButton;
    public UIButton resetButton;
    public UIButton rotateRightButton;
    public UIButton rotateLeftButton;
    public UIButton switchSlot4toEmptySlot1;
    public UIButton switchSlot10toEmptySlot2;
    private Dictionary<Transform, string> slotToButtonNameMap;

    private UIButton selectedButton = null;
    private Transform selectedButtonParent = null;

    private void Awake()
    {
        SetStartingButtonPositions();
        CreateSlotToButtonNameMap();
        SetButtonAnswers();
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
        }

        checkButton.onClick.AddListener(CheckAnswer);
        resetButton.onClick.AddListener(ResetButtons);
        Debug.Log("Rotate button onClick event set up"); 
        rotateRightButton.onClick.AddListener(RotateRight);
        rotateLeftButton.onClick.AddListener(RotateLeft);
        Debug.Log("Rotate button onClick event set up"); // This line will print a message in the console
        switchSlot4toEmptySlot1.onClick.AddListener(() => SwapSlots(3, 13)); 
        switchSlot10toEmptySlot2.onClick.AddListener(() => SwapSlots(9, 12));
    }
    private void SetStartingButtonPositions()
    {
        foreach (ButtonPosition buttonPosition in startingButtonPositions)
        {
            buttonPosition.Button.transform.SetParent(buttonPosition.slot);
            buttonPosition.Button.transform.localPosition = Vector3.zero;
        }
    }

    private void CreateSlotToButtonNameMap()
    {
        slotToButtonNameMap = new Dictionary<Transform, string>();

        foreach (ButtonAnswer buttonAnswer in buttonAnswers)
        {
            if (!slotToButtonNameMap.ContainsKey(buttonAnswer.correctSlot))
            {
                slotToButtonNameMap.Add(buttonAnswer.correctSlot, buttonAnswer.buttonName);
            }
        }
    }

    

    private void SetButtonAnswers()
    {
        foreach (UIButton button in buttons)
        {
            button.onClick.AddListener(() => CheckButtonAnswer(button));
        }
    }

    private void CheckButtonAnswer(UIButton button)
    {
        string buttonName = button.name;
        foreach (KeyValuePair<Transform, string> pair in slotToButtonNameMap){
            Transform correctSlot = pair.Key;
            string correctButtonName = pair.Value;
            if (IsButtonNameMatch(buttonName, correctButtonName) && correctSlot.childCount == 0)
            {
                button.transform.SetParent(correctSlot, false);
                // Debug.Log("Winner!");
                return;
            }
        }
    }
    private void SwapSlots(int indexSlotA, int indexSlotB)
{
    Transform slotA = startingButtonPositions[indexSlotA].slot;
    Transform slotB = startingButtonPositions[indexSlotB].slot;
    UIButton buttonA = slotA.GetComponentInChildren<UIButton>();
    UIButton buttonB = slotB.GetComponentInChildren<UIButton>();

    // Swap the positions of the buttons
    Vector3 positionA = slotA.position;
    Vector3 positionB = slotB.position;

    buttonA.transform.SetParent(slotB, false);
    buttonB.transform.SetParent(slotA, false);

    buttonA.transform.position = positionB;
    buttonB.transform.position = positionA;

    LayoutRebuilder.MarkLayoutForRebuild((RectTransform)slotA);
    LayoutRebuilder.MarkLayoutForRebuild((RectTransform)slotB);
}
    

    private void OnButtonClick(int buttonIndex){
        if (selectedButton == null){
            // Any button can be selected
            selectedButton = buttons[buttonIndex];
            selectedButtonParent = selectedButton.transform.parent;
            selectedButton.interactable = false;
        }
        else {
            // Determine the indexes for the current and target slots
            int selectedIndex = Array.IndexOf(startingButtonPositions, startingButtonPositions.FirstOrDefault(bp => bp.slot == selectedButtonParent));
            int targetIndex = Array.IndexOf(startingButtonPositions, startingButtonPositions.FirstOrDefault(bp => bp.slot == buttons[buttonIndex].transform.parent));

            // Check if the selected button is trying to swap with the 13th or 14th button
            if ((selectedIndex == 4-1 && targetIndex == 13) || // Slot 4 can swap with the button in slot 13
                (selectedIndex == 10-1 && targetIndex == 12)) // Slot 10 can swap with the button in slot 14
            {
                // Perform the swap
                UIButton targetButton = buttons[buttonIndex];
                Transform targetParent = targetButton.transform.parent;
                Vector3 targetPosition = targetButton.transform.localPosition;

                targetButton.transform.SetParent(selectedButtonParent);
                targetButton.transform.localPosition = Vector3.zero;

                selectedButton.transform.SetParent(targetParent);
                selectedButton.transform.localPosition = targetPosition;

                selectedButton.interactable = true;
                selectedButton = null;

                LayoutRebuilder.MarkLayoutForRebuild(selectedButtonParent.GetComponent<RectTransform>());
                LayoutRebuilder.MarkLayoutForRebuild(targetParent.GetComponent<RectTransform>());
            }
            else{
                // If no valid swap, re-enable the selected button
                selectedButton.interactable = true;
                selectedButton = null;
            }
        }
    }

    private void RotateRight()
{
    Debug.Log("Rotate right button pressed");
    int numButtons = 12; // Only rotate the first 12 buttons

    // Temporary storage for the first child to wrap it around to the end
    Transform firstChild = startingButtonPositions[0].slot.GetChild(0);

    // Rotate all buttons to the right
    for (int i = 0; i < numButtons - 1; i++)
    {
        Transform nextChild = startingButtonPositions[i + 1].slot.GetChild(0);
        nextChild.SetParent(startingButtonPositions[i].slot, false);
        nextChild.localPosition = Vector3.zero;
    }

    // Wrap the first child to the last slot of the rotation
    firstChild.SetParent(startingButtonPositions[numButtons - 1].slot, false);
    firstChild.localPosition = Vector3.zero;
}

private void RotateLeft()
{
    Debug.Log("Rotate left button pressed");
    int numButtons = 12; // Only rotate the first 12 buttons

    // Temporary storage for the last child to wrap it around to the beginning
    Transform lastChild = startingButtonPositions[numButtons - 1].slot.GetChild(0);

    // Rotate all buttons to the left
    for (int i = numButtons - 1; i > 0; i--)
    {
        Transform previousChild = startingButtonPositions[i - 1].slot.GetChild(0);
        previousChild.SetParent(startingButtonPositions[i].slot, false);
        previousChild.localPosition = Vector3.zero;
    }

    // Wrap the last child to the first slot of the rotation
    lastChild.SetParent(startingButtonPositions[0].slot, false);
    lastChild.localPosition = Vector3.zero;
}




    private bool IsButtonNameMatch(string buttonName, string correctButtonName)
    {
        // Compare button names ignoring case and ignoring spaces
        return buttonName.Equals(correctButtonName, System.StringComparison.OrdinalIgnoreCase)
            || buttonName.Replace(" ", "").Equals(correctButtonName.Replace(" ", ""), System.StringComparison.OrdinalIgnoreCase);
    }

    private void CheckAnswer()
    {
        List<string> orderedButtonNames = new List<string>();

        // Iterate over the buttonAnswers array to get the correct order of button names
        foreach (ButtonAnswer buttonAnswer in buttonAnswers)
        {
            orderedButtonNames.Add(buttonAnswer.buttonName);
        }

        // Check if the buttons are in the correct order
        for (int i = 0; i < orderedButtonNames.Count; i++)
        {
            Transform correctSlot = buttonAnswers[i].correctSlot;
            string correctButtonName = orderedButtonNames[i];

            if (correctSlot.childCount > 0)
            {
                UIButton button = correctSlot.GetChild(0).GetComponent<UIButton>();
                string buttonName = button.name;

                if (!IsButtonNameMatch(buttonName, correctButtonName))
                {
                    Debug.Log("Incorrect order!");
                    ResetButtons();
                    return;
                }
            }
            else
            {
                Debug.Log("Incomplete answer!");
                ResetButtons();
                return;
            }
        }

        // If all buttons are in the correct order, print "Winner!"
        Debug.Log("Winner!");
        machine.Solved();
    }

    private void ResetButtons()
    {
        // Reset each button to its starting position
        foreach (ButtonPosition buttonPosition in startingButtonPositions)
        {
            buttonPosition.Button.transform.SetParent(buttonPosition.slot);
            buttonPosition.Button.transform.localPosition = Vector3.zero;
            buttonPosition.Button.interactable = true;
        }
    
        // Clear the selected button
        selectedButton = null;
        selectedButtonParent = null;
    }



}
