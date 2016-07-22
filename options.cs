using System;
using System.IO;
using System.Reflection;
namespace Filebase {
	class Options
	{
		public String[] Folders = {};

		private string _CurDir = null;
		public string CurDir {
			get {
				if (_CurDir == null){
					_CurDir = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
					if (_CurDir == String.Empty)
					{
						_CurDir = Environment.CurrentDirectory;
					}
				}
				return _CurDir;
			}
		}

		public int AddMethodVersion = 0;


		public bool Load() {
			try
			{
				Folders = AZLib.StringUtils.LoadStringFromFile(Path.Combine(CurDir,"folders.cfg"),true);
			}
			catch (FileNotFoundException)
			{
				//Folders = new String[];
			}
			return true;
		}

		public string Version {
			get {
				AssemblyVersionAttribute att = (AssemblyVersionAttribute)Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(),typeof(AssemblyVersionAttribute)); 
				if (att != null){
					return att.Version;
				} 
				return "1.0.1";
				
			}
		}
	}
}