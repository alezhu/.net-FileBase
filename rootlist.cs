using System;
using System.Collections;
namespace Filebase {
	class RootList: ArrayList {
		public new Root this[int index] {
			get { return (Root) base[index];}
		}
	}
}