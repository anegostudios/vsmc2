using Newtonsoft.Json;
using System;
using System.IO;
using VSMC;
using UnityEngine;
using Newtonsoft.Json.Serialization;

public class ShapeAccessor
{
    
    public static void SerializeShapeToFile(Shape shape, string filePath)
    {
        //Save some things to the shape itself first...
        TextureManager.main.ApplyTexturesIntoShape(shape);
        shape.ResolveForBeforeSerialization();

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy(false, false) }
        };
        File.WriteAllText(filePath, JsonConvert.SerializeObject(shape, Formatting.Indented, settings));
        SaveManager.main.OnModelSave(filePath);
    }

    /// <summary>
    /// Deserialize a shape JSON from a file. Uses Newtonsoft to do so.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static Shape DeserializeShapeFromFile(string filePath)
    {
        //Load file.
        string contents;
        try
        {
            contents = File.ReadAllText(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Could not read file contents at " + filePath + ". Exception:" + e.Message);
            return null;
        }
        try //backup.
        {
            //Perform an immediate backup.
            string path = Application.persistentDataPath + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(filePath) + DateTime.Now.ToString("s") + ".json";

            if (Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            File.WriteAllText(path, contents);
            Debug.Log("Written file to " + path);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Could not write file backup. Exception:" + e.Message);
        }

        //Deserialize.
        return JsonConvert.DeserializeObject<Shape>(contents);
    }


}
