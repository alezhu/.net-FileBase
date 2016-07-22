using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
namespace AZLib {
	class StringUtils
	{
		static public String[] LoadStringFromFile(String fileName) {
			return LoadStringFromFile(fileName,false);
		}

		static public String[] LoadStringFromFile(String fileName, Boolean SkipBlank) {
			StringCollection sc = new StringCollection();
			using(StreamReader reader = new StreamReader(fileName,System.Text.Encoding.Default)) {
				string line = null;
				 while ((line = reader.ReadLine()) != null) {
					 if (!SkipBlank || (line.Trim() != String.Empty) )
					 {
						sc.Add(line);
					 }
				}
			}
			String[] result = new String[sc.Count];
			for (int i=0;i<sc.Count ;result[i] = sc[i], i++ );
			return result;
		}

		static public void SaveString(BinaryWriter writer, string AString) {
			writer.Write((UInt32)AString.Length);
			if (AString.Length != 0){
				writer.Write(AString);
			}
			
		}

		static public void SaveString(Stream stream, string AString) {
			using(BinaryWriter writer = new BinaryWriter(stream)) {
				SaveString(writer,AString);
			}
		}

		static public String LoadString(BinaryReader reader) {
			UInt32 Len = reader.ReadUInt32();
			String res = null;
			if (Len != 0){
				res = reader.ReadString();
			}
			return res;
		}

		static public String LoadString(Stream stream) {
			String res = null;
			using(BinaryReader reader = new BinaryReader(stream)) {
				res = LoadString(reader);
			}
			return res;
		}




	}
}