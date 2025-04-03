using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public class ShapeAccessor
{
    
    public static void SerializeShapeToFile(ShapeJSON shape, string filePath)
    {
        File.WriteAllText(filePath, JsonConvert.SerializeObject(shape));
    }

    /// <summary>
    /// Deserialize a shape JSON from a file. Uses Newtonsoft to do so.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static ShapeJSON DeserializeShapeFromFile(string filePath)
    {
        //Load file.
        string contents;
        try
        {
            contents = File.ReadAllText(filePath);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("Could not read file contents at " + filePath + ". Exception:" + e.Message);
            return null;
        }
        
        //Deserialize.
        return JsonConvert.DeserializeObject<ShapeJSON>(contents);
    }


}
