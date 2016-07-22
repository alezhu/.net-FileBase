using System;
using System.Collections;
using System.IO;
using AZLib.COM.ActiveX;
using System.Text;
using System.Runtime.InteropServices;
namespace Filebase {
	class Folder {
		const string sStoragePropertyName = "123465789012345678901234657890";
		private FolderList _folders = null;
		private FileList _files = null;
		public string Name = null;
		public Folder Parent = null;

		public FolderList Folders {
			get {
				if (_folders == null){
					_folders = new FolderList();
				}
				return _folders;
			}
		}
		public FileList Files{
			get {
				if (_files == null){
					_files = new FileList ();
				}
				return _files;
			}
		}

		public Folder(Folder parent) {
			_Init();
			Parent = parent;
		}

		public Folder(Folder parent, string name) {
			_Init();
			Parent = parent;
			Name = name;
			Build(FullPath);
		}

		public Folder(Folder parent, DirectoryInfo info) {
			_Init();
			Parent = parent;
			Name = info.Name;
			Build(FullPath);
		}


		protected virtual void _Init() {
			_folders = null;
			_files = null;
			//Name = null;
		}

		public Folder(Folder parent, BinaryReader reader) {
			_Init();
			Parent = parent;
			Load(reader);
		}

		public Folder(Folder parent, IStorage storage) {
			_Init();
			Parent = parent;
			Load(storage);
		}


		public void Save(BinaryWriter writer) {
			AZLib.StringUtils.SaveString(writer,Name);
			UInt32 count = (UInt32)Files.Count;
			writer.Write(count);
			if (count > 0){
				foreach (File file in Files){
					file.Save(writer);
				}
			}
			count = (UInt32)Folders.Count;
			writer.Write(count);
			if (count > 0){
				foreach (Folder folder in Folders){
					folder.Save(writer);
				}
			}
		}

		public void Load(BinaryReader reader) {
			_Init();
			Name = AZLib.StringUtils.LoadString(reader);
			UInt32 count = reader.ReadUInt32();
			while (count-- >0){
				Files.Add(new File(this,reader));
			}
			count = reader.ReadUInt32();
			while (count-- >0){
				Folders.Add(new Folder(this,reader));
			}
		}
	
		private string _fullPath = String.Empty;
		public string FullPath {
			get {
				if (_fullPath == String.Empty){
					if (Parent == null){
						_fullPath = Name;
					} else {
						_fullPath = Path.Combine(Parent.FullPath,Name);
					}
				}
				return _fullPath;
			}
		}

		public virtual void Build(string Path) {
			_Init();
			Console.WriteLine(Path);
			DirectoryInfo Info = new DirectoryInfo(Path);
			if (Info.Exists){
				try {
					FileInfo[] files = Info.GetFiles();
					
					if (Application.options.AddMethodVersion == 1){
						File[] fs = new File[files.Length];
						for (int i = 0;i<files.Length ;i++ ){
							fs[i] = new File(this,files[i]);
						}
						Files.AddRange(fs as ICollection);
					} else {
						foreach (FileInfo fileInfo in files){
							Files.Add(new File(this,fileInfo));
						}
					}
					DirectoryInfo[] folders = Info.GetDirectories();
					if (Application.options.AddMethodVersion == 0){
						foreach (DirectoryInfo dInfo in folders){
							Folders.Add(new Folder(this,dInfo));
						}
					} else {
						Folder[] fs = new Folder[folders.Length];
						for (int i=0;i<folders.Length ;i++ ){
							fs[i] = new Folder(this,folders[i]);
						}
						Folders.AddRange(fs as ICollection);
					}
				}
				catch ( UnauthorizedAccessException) {
					return;
				}
			} else {
				throw new DirectoryNotFoundException();
			}
		}

		public void Assign(Folder folder) {
			using(MemoryStream ms = new MemoryStream()) {
				using(BinaryWriter writer = new BinaryWriter(ms)) {
					folder.Save(writer);
				}
				ms.Position = 0;
				using(BinaryReader reader = new BinaryReader(ms)) {
					this.Load(reader);
				}
			}
		}

		public void Load(IStorage _storage) {
			_Init();
			Storage storage = new Storage(_storage);
			UCOMIStream istream = storage.OpenStream(sStoragePropertyName,0x00000010/*STGM_READ or STGM_SHARE_EXCLUSIVE*/);
			if (istream != null){
				AZLib.COM.ActiveX.Stream stream = new AZLib.COM.ActiveX.Stream(istream);
				int Len = stream.ReadInt32();
				if (Len != 0){
					byte[] bytes = new byte[Len];
					stream.Read(ref bytes);
					Encoding enc = Encoding.GetEncoding(Encoding.Default.WindowsCodePage);
					string name = enc.GetString(bytes,0,Len);
					Name = name;
				}
				Double d = stream.ReadDouble();
				Int32 Size  = stream.ReadInt32();
				Size  = stream.ReadInt32();
			}

			IEnumSTATSTG iEnum = storage.EnumElements();
			if (iEnum != null){
				iEnum.Reset();
				STATSTG st;

				int fetched = 0;
				while (iEnum.Next(1,out st,out fetched)== 0 /*S_OK*/){
//					Console.WriteLine("Name {1}: Item {0}",st.pwcsName,FullPath);
					if (st.type == 1 /*STGTY_STORAGE*/){
						IStorage stg = storage.OpenStorage(st.pwcsName,null,0x00000010/*STGM_READ or STGM_SHARE_EXCLUSIVE*/,null);
						if (stg != null){
							Folders.Add(new Folder(this,stg));
						}
					} else {
						/*STGTY_STREAM*/
						if (st.pwcsName == sStoragePropertyName){
							continue;
						}
						istream = storage.OpenStream(st.pwcsName,0x00000010/*STGM_READ or STGM_SHARE_EXCLUSIVE*/);
						if (istream != null){
							Files.Add(new File(this,istream));
						}
					}
				}
				//System.GC.Collect();
			}

		}


	}
}