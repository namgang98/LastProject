public static class Sfx
{
    public static void Play(SfxType type)
    {
        if(type == SfxType.None || !SoundManager.HasInstance)
            return;

        SoundManager.instance.PlaySfx(type);
    }
}