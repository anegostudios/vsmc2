using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


/// <summary>
/// Manages a PlayerPreference based on a code and its type.
/// Automatically caches the value and only loads when needed.
/// </summary>
/// <typeparam name="T">Must be int, float, string, or bool.</typeparam>
public class Preference<T>
{

    string playerPrefCode;
    //Turns out we need to also store if the value is cached or not, since generics cannot be null.
    T cached;
    bool isCached;
    T defaultVal;

    public Preference(string code, T defaultVal)
    {
        playerPrefCode = code;
        this.defaultVal = defaultVal;
        isCached = false;
    }

    /// <summary>
    /// Returns the cached value for the preference, or loads it from disk if needed.
    /// </summary>
    public T GetValue()
    {
        //If key doesn't exist, return the default.
        if (!PlayerPrefs.HasKey(playerPrefCode))
        {
            cached = defaultVal;
            isCached = true;
            return cached;
        }
        if (!isCached)
        {
            if (typeof(T) == typeof(int))
            {
                Debug.Log("Getting value as integer from code " + playerPrefCode);
                cached = (T)(System.Object)PlayerPrefs.GetInt(playerPrefCode);
            }
            else if (typeof(T) == typeof(float))
            {
                cached = (T)(System.Object)PlayerPrefs.GetFloat(playerPrefCode);
            }
            else if (typeof(T) == typeof(string))
            {
                cached = (T)(System.Object)PlayerPrefs.GetString(playerPrefCode);
            }
            else if (typeof(T) == typeof(bool))
            {
                //Booleans can't be stored in player prefs, so I use an integer. 1 is true, 0 is false.
                cached = (T)(System.Object)(PlayerPrefs.GetInt(playerPrefCode) > 0);
            }
            else
            {
                throw new InvalidOperationException("Invalid type provided for ProgramPreference. Please use int, float, string, or bool.");
            }
            isCached = true;
        }
        return cached;
    }

    /// <summary>
    /// Updates the cache of the value, and then saves it to disk.
    /// </summary>
    public void SetValue(T value, bool save = true)
    {
        cached = value;
        isCached = true;
        if (value is int i)
        {
            Debug.Log("Setting value of "+value.ToString() +" as integer into code "+playerPrefCode);
            PlayerPrefs.SetInt(playerPrefCode, i);
        }
        else if (value is float f)
        {
            PlayerPrefs.SetFloat(playerPrefCode, f);
        }
        else if (value is string s)
        {
            PlayerPrefs.SetString(playerPrefCode, s);
        }
        else if (value is bool b)
        {
            //Booleans can't be stored in player prefs, so I use an integer. 1 is true, 0 is false.
            PlayerPrefs.SetInt(playerPrefCode, b ? 1 : 0);
        }
        else
        {
            throw new InvalidOperationException("Invalid type provided for ProgramPreference. Please use int, float, string or bool.");
        }
        if (save) PlayerPrefs.Save();
    }

}
