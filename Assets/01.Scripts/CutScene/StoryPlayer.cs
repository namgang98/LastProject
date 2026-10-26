using System.Collections;
using UnityEngine;

public class StoryPlayer : MonoBehaviour
{
    [SerializeField] TypewriterText typewriter;
    [SerializeField] GameObject nextIndicator;
    [SerializeField, TextArea(2, 4)] string[] lines;
    [SerializeField] float autoAdvanceDelay = 0f;

    public IEnumerator Play()
    {
        typewriter.Clear();

        foreach(string line in lines)
        {
            yield return PlayLine(line);
        }

        SetIndicator(false);
    }

    private IEnumerator PlayLine(string line)
    {
        SetIndicator(false);
        StartCoroutine(typewriter.Type(line));

        while (typewriter.IsTyping)
        {
            if(AdvanceInput.WasPressed())
                typewriter.Skip();

            yield return null;
        }

        SetIndicator(true);
        yield return null;
        yield return WaitAdvance();
    }

    private IEnumerator WaitAdvance()
    {
        float timer = 0f;

        while(!AdvanceInput.WasPressed())
        {
            timer += Time.unscaledDeltaTime;

            if(autoAdvanceDelay > 0f && timer >= autoAdvanceDelay)
                yield break;

            yield return null;
        }
    }

    private void SetIndicator(bool show)
    {
        if(nextIndicator == null)
            return;

        nextIndicator.SetActive(show);
    }
}