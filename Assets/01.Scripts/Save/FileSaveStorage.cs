using System.IO;
using UnityEngine;

public interface ISaveStorage
{
    bool TryLoad(out string json);
    void Save(string json);
    void Delete();
}

public class FileSaveStorage : ISaveStorage
{
    private readonly string path;
    private readonly string tempPath;

    public FileSaveStorage(string fileName)
    {
        path = Path.Combine(Application.persistentDataPath, fileName);
        tempPath = path + ".tmp";
    }

    public bool TryLoad(out string json)
    {
        json = null;

        if(!File.Exists(path))
            return false;

        json = File.ReadAllText(path);
        return !string.IsNullOrEmpty(json);
    }

    public void Save(string json)
    {
        File.WriteAllText(tempPath, json);

        if(File.Exists(path))
        {
            File.Replace(tempPath, path, null);
            return;
        }

        File.Move(tempPath, path);
    }

    public void Delete()
    {
        if(File.Exists(path))
            File.Delete(path);
    }
}
