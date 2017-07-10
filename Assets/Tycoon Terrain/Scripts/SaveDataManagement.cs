using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;




namespace TycoonTerrain{
	public static class SaveDataManagement {

		const string KEY_LASTSAVE_VERSION = "version";
		/// <summary>
		/// Increase this number if a change has been made so that the save from previous version cannot be used. 
		/// </summary>
		const int saveVersion = 0;


		#region SaveMethods
		public static bool HasSavedData(string dataKey){
			return IsVersionOK(dataKey) && PlayerPrefs.HasKey(dataKey);
		}

		public static T LoadData<T>(string dataKey){
			string dataString = PlayerPrefs.GetString(dataKey);
			T data = DeserializeObjectFromString<T>(dataString);
			
			return data;
		}
		
		public static void SaveData<T>(T data, string dataKey){
			PlayerPrefs.SetString(dataKey, SerializeObjectToString<T>(data));
			PlayerPrefs.Save();
			SetSaveVersion(dataKey);
		}
		
		public static void DeleteData(string dataKey){
			PlayerPrefs.DeleteKey(dataKey);
			PlayerPrefs.Save ();
		}

		public static void DeleteAll(){
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save ();
		}
		#endregion

		#region VersionControl
		private static bool IsVersionOK(){
			return PlayerPrefs.HasKey(KEY_LASTSAVE_VERSION) && PlayerPrefs.GetInt(KEY_LASTSAVE_VERSION) == saveVersion;
		}
		private static void SetSaveVersion(){
			PlayerPrefs.SetInt(KEY_LASTSAVE_VERSION, saveVersion);
			PlayerPrefs.Save();
		}

		private static bool IsVersionOK(string dataKey){
			return PlayerPrefs.HasKey(KEY_LASTSAVE_VERSION + dataKey) && PlayerPrefs.GetInt(KEY_LASTSAVE_VERSION + dataKey) == saveVersion;
		}
		private static void SetSaveVersion(string dataKey){
			PlayerPrefs.SetInt(KEY_LASTSAVE_VERSION + dataKey, saveVersion);
			PlayerPrefs.Save();
		}
		#endregion



		#region Serialization
		/// <summary>
		/// Serializes an object into an xml string
		/// </summary>
		private static string SerializeObjectToString<T>(T obj)
		{ 	
			//TODO: Add encryption
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(T)); 
			StringWriter stringWriter = new StringWriter();
			XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
			xmlSerializer.Serialize(xmlWriter, obj);		
			string xml = stringWriter.ToString();
			
			xmlWriter.Close();
			stringWriter.Close();

			//Debug.Log(xml);
			return xml;		
		}

		/// <summary>
		/// Deserializes an xml string into an object
		/// </summary>
		private static T DeserializeObjectFromString<T>(string xml)
		{ 
			XmlSerializer serializer = new XmlSerializer(typeof(T));        
			StringReader stringReader = new StringReader(xml);        
			XmlTextReader xmlReader = new XmlTextReader(stringReader);        
			T obj = (T)serializer.Deserialize(xmlReader);
			
			xmlReader.Close();
			stringReader.Close();
			
			return obj;
		}
		#endregion
	}

	public class TransformData{
		public Quaternion rotation;
		public Vector3 position;
		public Vector3 scale;

		public static TransformData TransformToSerializable(Transform trans){
			TransformData data = new TransformData();
			data.rotation = trans.rotation;
			data.position = trans.position;
			data.scale = trans.localScale;
			return data;
		}

		public static void SetTransformFromData(Transform trans, TransformData data){
			trans.rotation = data.rotation;
			trans.position = data.position;
			trans.localScale = data.scale;
		}
	}
}