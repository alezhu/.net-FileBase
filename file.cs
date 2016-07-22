using System;
using System.IO;
using AZLib;
using AZLib.COM.ActiveX;
using System.Runtime.InteropServices;
using System.Text;
namespace Filebase {
	class File {
		public UInt64 Size;
		public DateTime CreateTime;
		public DateTime AccessTime;
		public DateTime WriteTime;
		public string Name;
		public Folder Parent = null;

		public File(Folder parent) {
			_Init();
			Parent = parent;
		}

		protected virtual void _Init() {
			Name = null;
			Size = 0;
			CreateTime = DateTime.MinValue;
			AccessTime = DateTime.MinValue;
			WriteTime = DateTime.MinValue;
		}

		public File(Folder parent, FileSystemInfo fileInfo) {
			Parent = parent;
			Load(fileInfo);
		}

		public File(Folder parent, String fullFileName) {
			Parent = parent;
			FileInfo fileInfo = new FileInfo(fullFileName);
			Load(fileInfo);
		}

		public File(Folder parent, BinaryReader reader) {
			_Init();
			Parent = parent;
			Load(reader);
		}

		public File(Folder parent, UCOMIStream stream) {
			_Init();
			Parent = parent;
			Load(stream);
		}

		public void Save(BinaryWriter writer) {
			StringUtils.SaveString(writer, Name);
			writer.Write(Size);
			writer.Write(CreateTime.ToFileTime());
			writer.Write(AccessTime.ToFileTime());
			writer.Write(WriteTime.ToFileTime());
		}

		public void Load(BinaryReader reader) {
			Name = StringUtils.LoadString(reader);
			Size = reader.ReadUInt64();
			CreateTime = DateTime.FromFileTime(reader.ReadInt64());
			AccessTime = DateTime.FromFileTime(reader.ReadInt64());
			WriteTime = DateTime.FromFileTime(reader.ReadInt64());
		}

		public void Load(FileSystemInfo fileInfo) {
			Name = fileInfo.Name;
			FileInfo fi = new FileInfo(FullPath);
			Size = (UInt32)fi.Length;
			CreateTime = fileInfo.CreationTime;
			AccessTime = fileInfo.LastAccessTime;
			WriteTime = fileInfo.LastWriteTime;
		}

		public bool IsEqualTo(File file) {
			return (Name == file.Name) && (Size == file.Size) && (WriteTime == file.WriteTime);
		}
		private string _fullPath = null;
		public string FullPath {
			get {
				if (_fullPath == null){
					_fullPath = Path.Combine(Parent.FullPath,Name);
				}
				return _fullPath;
			}
		}

		public void Load(UCOMIStream _stream) {
			AZLib.COM.ActiveX.Stream stream = new AZLib.COM.ActiveX.Stream(_stream);
			int Len = stream.ReadInt32();
			if (Len != 0){
				byte[] bytes = new byte[Len];
				stream.Read(ref bytes);
				Encoding enc = Encoding.GetEncoding(Encoding.Default.WindowsCodePage);
				string name = enc.GetString(bytes,0,Len);
				Name = name;
			}
			Double d = stream.ReadDouble();
			//Console.WriteLine("Double {0}",d);
			WriteTime = DateTime.FromOADate(d); 
			Size  = (UInt64) stream.ReadInt64();
		}

	}
}