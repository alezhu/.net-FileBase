using System;
using System.IO;
using System.Collections;
using AZLib.COM.ActiveX;
namespace Filebase {
	class Root: Folder {
		public new FileBase Parent = null;
		public bool Builded = false;

		public Root(FileBase parent):base(null) {
			Parent = parent;
		}

		public Root(FileBase parent, string path):base(null,path) {
			Parent = parent ;
		}
		public Root(FileBase parent, BinaryReader reader):base(null,reader) {
			Parent = parent ;
		}

		public Root(FileBase parent, IStorage storage):base(null,storage) {
			Parent = parent ;
		}
		
		public override void Build(string Path) {
			base.Build(Path);
			Builded= true;
		}
	}
}