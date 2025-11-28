using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChecklistManager : MonoBehaviour
{
    public static ChecklistManager Instance;
    public GameObject allCompleteUI;    // Assign a panel or text that says "Objective Complete!"

    [System.Serializable]
    public class ChecklistItem
    {
        public string objectiveID;     // Matches NPC objective
        public TMP_Text uiText;        // UI element for that objective
        public bool isCompleted = false;
        
    }

    public ChecklistItem[] items;

    void Awake()
    {
        Instance = this;

        if (allCompleteUI != null)
        {
            allCompleteUI.SetActive(false);
        }
    }

    public void MarkCompleted(string id)
    {
        foreach (var item in items)
        {
            if (item.objectiveID == id && !item.isCompleted)
            {
                item.isCompleted = true;
                item.uiText.text = $"<s>{item.uiText.text}</s> \u2713";
                item.uiText.color = new Color(0.1f, 0.5f, 0.1f); // dark green

            }
        }

        // Check if all objectives are done
        bool allDone = true;

        foreach (var i in items)
        {
            if (!i.isCompleted)
            {
                allDone = false;
                break;
            }
        }

        if (allDone && allCompleteUI != null)
        {
            allCompleteUI.SetActive(true);
        }
    }
}