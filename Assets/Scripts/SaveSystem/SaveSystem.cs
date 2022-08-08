using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Xml.Serialization;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string SaveExtension = "tcsave";
    private static string SaveName = "PlayerData";

    ///<summary>
    /// Guid used to encrypt the save files, this is used soley as a deterant and not as a fullproof way of preventing tampering as the encryption key could be accessed.
    ///</summary>
    public static Guid encryptionGuid = Guid.Parse("2f1706131be0410ea572536ce7ccbfc4");

    ///<summary>
    /// Sets whether or not saves must match the current project version
    ///</summary>
    public static bool VersioningMustMatch = true;

    ///<summary>
    /// Sets whether or not saves should by default use encryption
    ///</summary>
    public static bool ShouldUseEncryption = false;


    ///<summary>
    /// Sets whether or not a save file saved as an XML file must have the XML extension.
    ///</summary>
    public static bool PrefixAllXml = false;

    private static string SaveLocation => Application.persistentDataPath + "/" + SaveName + "." + SaveExtension;
    private static string TutorialDataLocation => Application.persistentDataPath + "/" + SaveName + "." + "TutData";

    ///<summary>
    /// Saves data to file included in the path (This version uses BinaryFormatter, it is recommended that you use _SaveDataToXMLFile() this method has security vulnerabilities).
    ///</summary>
    private static void _SaveDataToFile<TSerializableObject>(TSerializableObject serializableObject, string path)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        using (FileStream stream = new FileStream(path, File.Exists(path) ? FileMode.Create : FileMode.CreateNew))
        {
            formatter.Serialize(stream, serializableObject);
        }

        Debug.Log(serializableObject.GetType().ToString() + " File saved: " + path);
    }
    ///<summary>
    /// Load data from file included in the path (This version uses BinaryFormatter, it is recommended that you use _TryLoadDataFromXMLFile() as this method has security vulnerabilities).
    ///</summary>
    private static bool _TryLoadDataFromFile<TSerializableObject>(string path, out TSerializableObject serializableObject)
    {
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            serializableObject = (TSerializableObject)formatter.Deserialize(stream);
            stream.Close();
            return true;
        }
        else
        {
            Debug.LogWarning("Save file not found in " + path + " , Please create a save before trying to load.");
            serializableObject = default(TSerializableObject);
            return false;
        }
    }

    ///<summary>
    /// Saves data to an XML file included in the path with an optional parameter to encrypt this file to deter outside tampering.
    /// useEncryptionOverride is set to null, this function uses the class level setting
    ///</summary>
    private static void _SaveDataToXMLFile<TSerializableObject>(TSerializableObject serializableObject, string path, bool? useEncryptionOverride = null)
    {
        if (PrefixAllXml) path += ".xml";

        bool useEncryption = useEncryptionOverride != null && useEncryptionOverride.HasValue ? useEncryptionOverride.Value : ShouldUseEncryption;

        XmlSerializer serializer = new XmlSerializer(typeof(SerializedXmlWithMetaData<TSerializableObject>));

        using (Stream stream = new FileStream(path, File.Exists(path) ? FileMode.Create : FileMode.CreateNew))
        {
            if (useEncryption)
            {
                using (Stream encryptedStream = _GetEncryptedXMLStream(stream, CryptoStreamMode.Write))
                {
                    serializer.Serialize(encryptedStream, new SerializedXmlWithMetaData<TSerializableObject>(serializableObject));
                }
            }
            else
            {
                serializer.Serialize(stream, new SerializedXmlWithMetaData<TSerializableObject>(serializableObject));
            }
        }

        Debug.Log(serializableObject.GetType().ToString() + " File saved: " + path);
    }

    ///<summary>
    /// Load data from an XML file included in the path with an optional parameter to encrypt this file to deter outside tampering (This encryption option should match the saved file so that there aren't any issues).
    /// if versioningMustMatchOverride or useEncryptionOverride are set to null, this function uses the class level settings
    ///</summary>
    private static bool _TryLoadDataFromXMLFile<TSerializableObject>(string path, out TSerializableObject serializableObject, bool? versioningMustMatchOverride = null, bool? useEncryptionOverride = null)
    {
        bool result = _TryLoadDataFromXMLFileWithMetaData(path, out SerializedXmlWithMetaData<TSerializableObject> serializableObjectWithMetaData, versioningMustMatchOverride, useEncryptionOverride);
        serializableObject = serializableObjectWithMetaData.SerializedObject;
        return result;
    }


    ///<summary>
    /// Load data from an XML file included in the path (keeping SerializedXmlWithMetaData wrapper) with an optional parameter to encrypt this file to deter outside tampering (This encryption option should match the saved file so that there aren't any issues).
    /// if versioningMustMatchOverride or useEncryptionOverride are set to null, this function uses the class level settings
    ///</summary>
    private static bool _TryLoadDataFromXMLFileWithMetaData<TSerializableObject>(string path, out SerializedXmlWithMetaData<TSerializableObject> serializableObjectWithMetaData, bool? versioningMustMatchOverride = null, bool? useEncryptionOverride = null)
    {
        if (PrefixAllXml) path += ".xml";

        //Check if versioning is going to need to match
        bool versioningMustMatch = versioningMustMatchOverride != null && versioningMustMatchOverride.HasValue ? versioningMustMatchOverride.Value : VersioningMustMatch;
        bool useEncryption = useEncryptionOverride != null && useEncryptionOverride.HasValue ? useEncryptionOverride.Value : ShouldUseEncryption;

        if (File.Exists(path) && ReadValidObjectWithMetaData(out SerializedXmlWithMetaData<TSerializableObject> deserializedObjectWithMetaData))
        {
            serializableObjectWithMetaData = deserializedObjectWithMetaData;
            return true;
        }
        else
        {
            Debug.LogWarning("Valid Save File not found in " + path + ". VersioningMustMatch is set to: " + versioningMustMatch.ToString());
            serializableObjectWithMetaData = default(SerializedXmlWithMetaData<TSerializableObject>);
            return false;
        }

        //Returns true if the metadata meets the metadata requirements, i.e. matching versions
        bool ReadValidObjectWithMetaData(out SerializedXmlWithMetaData<TSerializableObject> deserializedObject)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializedXmlWithMetaData<TSerializableObject>));

            bool success = false;

            using (Stream stream = new FileStream(path, FileMode.Open))
            {
                try
                {
                    if (useEncryption)
                    {
                        try
                        {
                            using (Stream encryptedStream = _GetEncryptedXMLStream(stream, CryptoStreamMode.Read))
                            {
                                deserializedObject = (SerializedXmlWithMetaData<TSerializableObject>)serializer.Deserialize(encryptedStream);
                            }

                            success = versioningMustMatch ? deserializedObject.Version == Application.version : true;
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Failed to decrypt file: " + path + ". Could be that this file wasn't encrypted? New File will be created. Error: " + e);
                            deserializedObject = default(SerializedXmlWithMetaData<TSerializableObject>);
                        }
                    }
                    else
                    {
                        deserializedObject = (SerializedXmlWithMetaData<TSerializableObject>)serializer.Deserialize(stream);
                        success = versioningMustMatch ? deserializedObject.Version == Application.version : true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Failed to Deserialize file: " + path + " but exception was handled, Invalid XML, could be an old save file?, Error: " + e);
                    deserializedObject = default(SerializedXmlWithMetaData<TSerializableObject>);
                }
            }

            return success;
        }
    }


    ///<summary>
    /// Used internally to encrypt a stream for use.
    ///</summary>
    private static Stream _GetEncryptedXMLStream(Stream unEncryptedStream, CryptoStreamMode cryptoStreamMode)
    {
        byte[] encryptionGuidAsBytes = encryptionGuid.ToByteArray();

        AesCryptoServiceProvider aes = new AesCryptoServiceProvider()
        {
            Key = encryptionGuidAsBytes,
            IV = encryptionGuidAsBytes
        };


        ICryptoTransform cryptoTransform = cryptoStreamMode == CryptoStreamMode.Write ? aes.CreateEncryptor() : aes.CreateDecryptor();

        CryptoStream cryptoStream = new CryptoStream(
            unEncryptedStream,
            cryptoTransform,
            cryptoStreamMode
            );

        return cryptoStream; //return to use
    }

    //Main Game saves
    //New implementation

    public static void SaveGameNew(Dictionary<int, LevelData> saveData)
    {
        _SaveDataToXMLFile(new SerializableDictionary<int, LevelData>(saveData), SaveLocation, false);
    }

    public static Dictionary<int, LevelData> LoadGameNew()
    {
        TryLoadGameNew(out Dictionary<int, LevelData> saveData);
        return saveData;
    }

    public static bool TryLoadGameNew(out Dictionary<int, LevelData> saveData)
    {
        bool result = _TryLoadDataFromXMLFile(SaveLocation, out SerializableDictionary<int, LevelData> serializedSavedData);
        saveData = result && serializedSavedData != null ? serializedSavedData.Deserialize() : null;
        return result;
    }

    public static bool TryLoadGameWithMetaData(out SerializedXmlWithMetaData<SerializableDictionary<int, LevelData>> saveWithMetaData)
    {
        bool result = _TryLoadDataFromXMLFileWithMetaData(SaveLocation, out SerializedXmlWithMetaData<SerializableDictionary<int, LevelData>> serializedSaveWithMetaData);
        saveWithMetaData = serializedSaveWithMetaData;
        return result;
    }

    //Tutorial data



    //Gross unity prefs
    public static bool TryLoadUnityPrefInt(string prefName, out int pref)
    {
        pref = 0;
        if (PlayerPrefs.HasKey(prefName)) 
        { 
            pref = PlayerPrefs.GetInt(prefName);
            return true;
        }

        return false;
    }

    public static bool TryLoadUnityPrefString(string prefName, out string pref)
    {
        pref = null;
        if (PlayerPrefs.HasKey(prefName))
        {
            pref = PlayerPrefs.GetString(prefName);
            return true;
        }

        return false;
    }

    public static bool TryLoadUnityPrefFloat(string prefName, out float pref)
    {
        pref = 0;
        if (PlayerPrefs.HasKey(prefName))
        {
            pref = PlayerPrefs.GetFloat(prefName);
            return true;
        }

        return false;
    }

}

public enum GameLoadType
{
    New,
    Existing
}

///<summary>
/// Serialized XML wrapper that stores application version (And potentially other data that we might want in addition to the save data)
///</summary>
public struct SerializedXmlWithMetaData<TSerializableObject>
{
    public string Version;
    public System.DateTime SaveDateTimeUTC;
    public TSerializableObject SerializedObject;

    public SerializedXmlWithMetaData(TSerializableObject SerializedObject)
    {
        this.SerializedObject = SerializedObject;
        Version = Application.version;
        SaveDateTimeUTC = System.DateTime.UtcNow;
    }
}