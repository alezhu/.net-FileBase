using System;
using System.Collections;
namespace Filebase {
	class FileList: ArrayList {
		public new File this[int index] {
			get {return (File) base[index];}
		}
	}
}
