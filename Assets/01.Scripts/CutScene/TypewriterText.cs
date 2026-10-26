using System.Collections;
using TMPro;
using UnityEngine;

public class TypewriterText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] float charsPerSecond = 22f;
    [SerializeField] float punctuationPause = 0.3f;
    [SerializeField] SfxType tickSfx = SfxType.None;
    [SerializeField] int tickEvery = 2;
    [SerializeField, Range(0f, 1f)] float tickVolume = 0.5f;

    private bool skipRequested;

    public bool IsTyping { get; private set; }

    public void Clear()
    {
        label.text = string.Empty;
        label.maxVisibleCharacters = 0;
    }

    public void Skip()
    {
        if(IsTyping)
            skipRequested = true;
    }

    public IEnumerator Type(string line)
    {
        IsTyping = true;
        skipRequested = false;

        label.text = line;
        label.maxVisibleCharacters = 0;
        label.ForceMeshUpdate();

        int total = label.textInfo.characterCount;
        float baseDelay = 1f / Mathf.Max(1f, charsPerSecond);

        for(int i = 0; i < total; i++)
        {
            if(skipRequested)
                break;

            label.maxVisibleCharacters = i + 1;
            char c = label.textInfo.characterInfo[i].character;
            PlayTick(i, c);

            yield return new WaitForSecondsRealtime(GetDelay(c, baseDelay));
        }

        label.maxVisibleCharacters = total;
        IsTyping = false;
    }

    private float GetDelay(char c, float baseDelay)
    {
        if(c == '.' || c == ',' || c == '!' || c == '?' || c == '…')
            return baseDelay + punctuationPause;

        return baseDelay;
    }

    private void PlayTick(int index, char c)
    {
        if(tickSfx == SfxType.None || char.IsWhiteSpace(c))
            return;

        if(index % Mathf.Max(1, tickEvery) != 0)
            return;

        if(!SoundManager.HasInstance)
            return;

        SoundManager.instance.PlaySfx(tickSfx, tickVolume);
    }
}
