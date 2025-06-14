/*
 *      MIT License
 *      # Copyright (c) 2025 OnistDerFalke
 *      # This software is provided "as is", without any warranty. You can use, modify, and share it freely.
 */

using TMPro;
using UnityEngine;

/// <summary>
/// Simple tmp text autosizing class.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPAutoSizeHeight : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    private RectTransform rectTransform;
    private float minHeight;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        minHeight = rectTransform.rect.height;  // zapamiêtujemy startow¹ wysokoœæ
    }

    public void SetText(string text)
    {
        tmp.text = text;
        UpdateHeight();
    }

    private void UpdateHeight()
    {
        tmp.ForceMeshUpdate();
        float textHeight = tmp.textBounds.size.y;
        float padding = 5f;

        float newHeight = Mathf.Max(minHeight, textHeight + padding);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
    }
}