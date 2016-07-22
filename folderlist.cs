using System;
using System.Collections;
namespace Filebase {
	class FolderList: ArrayList {
		public new Folder this[int index] {
			get { return (Folder) base[index];}
		}
	}
}