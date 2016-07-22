using System;
using System.IO;
using System.Collections;
using AZLib.COM.ActiveX;
using System.Text;
using System.Runtime.InteropServices;
namespace Filebase {
	class FileBase {
		public RootList Roots = new RootList();
		public int AddRoot(Root root) {
			return Roots.Add(root);
		}

		public void Load(string fileName) {
			Roots.Clear();
			System.GC.Collect();
			using (FileStream stream = new FileStream(fileName,FileMode.Open,FileAccess.Read)) {
				using(BinaryReader reader = new BinaryReader(stream,Encoding.GetEncoding(Encoding.Default.WindowsCodePage))) {
					UInt32 count = reader.ReadUInt32();
					while (count-- > 0 ) {
						AddRoot(new Root(this,reader));
					}
				}
			}
		}

		public void Save(string fileName) {
			using (FileStream stream = new FileStream(fileName,FileMode.OpenOrCreate,FileAccess.Write)) {
				using(BinaryWriter writer = new BinaryWriter(stream,Encoding.GetEncoding(Encoding.Default.WindowsCodePage))) {
					UInt32 count = (UInt32)Roots.Count;
					writer.Write(count);
					foreach (Root root in Roots){
						root.Save(writer);
					}
				}
			}
		}

		public void LoadOldBase(string fileName) {
//		public static extern int StgOpenStorage([MarshalAs(UnmanagedType.LPWStr)] string wcsName, IStorage pstgPriority, int grfMode, IntPtr snbExclude, int reserved, [Out] out IStorage storage);
			IStorage storage = null;
			if (0 == Utils.StgOpenStorage(fileName,null,0x00000010/*STGM_READ or STGM_SHARE_EXCLUSIVE*/,IntPtr.Zero,0, out storage)){
				Load(storage);
			}
		}
		
		public void Load(IStorage _storage) {
			Storage storage = new Storage(_storage);
			IEnumSTATSTG iEnum = storage.EnumElements();
			if (iEnum != null){
				iEnum.Reset();
				STATSTG st;
				int fetched = 0;
				while (iEnum.Next(1,out st,out fetched)== 0 /*S_OK*/){
					if (st.type == 1 /*STGTY_STORAGE*/){
						IStorage stg = storage.OpenStorage(st.pwcsName,null,0x00000010/*STGM_READ or STGM_SHARE_EXCLUSIVE*/,null);
						if (stg != null){
							AddRoot(new Root(this,stg));
							System.GC.Collect();
						}
					}
				}
			}
		}
	};

}