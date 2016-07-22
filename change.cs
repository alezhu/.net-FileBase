
using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace Filebase {
	public enum ChangeType {New,Delete,Modify};
	class Change {
		public readonly ChangeType Type;
		public readonly File NewFile = null;
		public readonly File OldFile = null;
		public readonly Folder NewFolder = null;
		public readonly Folder OldFolder = null;
		public Change(ChangeType type, File newFile, File oldFile, Folder newFolder, Folder oldFolder) {
			Type = type;
			NewFile = newFile;
			OldFile = oldFile;
			NewFolder = newFolder;
			OldFolder = oldFolder;
		}
		public bool IsFolder {
			get {return (NewFolder != null) || (OldFolder !=null);}
		}
		public string GetFullPath() {
			return (NewFile != null)?NewFile.FullPath:(OldFile !=null)?OldFile.FullPath:(NewFolder != null)?NewFolder.FullPath:(OldFolder != null)?OldFolder.FullPath:null; 
		}

		public Root GetRoot() {
			Folder p = (NewFile != null)?NewFile.Parent:(OldFile !=null)?OldFile.Parent:(NewFolder != null)?NewFolder.Parent:(OldFolder != null)?OldFolder.Parent:null; 
			while (p.Parent != null){
				p = p.Parent;
			}
			return (Root)p;
		}
	}

	class Changes: ArrayList, IComparer {
		public new Change this[int index] {
			get { return (Change)base[index];}
		}

		public static Changes MakeChanges(FileBase newBase, FileBase oldBase) {
			Changes changes = new Changes();
			Hashtable hash = new Hashtable();
			foreach (Root root in oldBase.Roots){
				hash.Add(root.Name,root);
			}
			foreach (Root root in newBase.Roots){
				Console.WriteLine("Root: {0}",root.Name);
				Root oldRoot = (Root)hash[root.Name];
				if (oldRoot != null){
					if (root.Builded){
						changes.FindChanges(root,oldRoot);
					} else {
						root.Assign(oldRoot);
					}
					hash.Remove(root.Name);
				} else {
					changes.MakeFolderAsNew(root);
				}

			}
			return changes;
		}

		public static Changes  MakeAsNew(FileBase filebase) {
			Changes changes = new Changes();
			foreach (Root root in filebase.Roots){
				changes.MakeFolderAsNew(root);
			}
			return changes;
		}

		public void FindChanges(Folder newFolder, Folder oldFolder) {
			//StringDictionary dic = new  StringDictionary();
			//SortedList dic = new SortedList(this as IComparer);
			Hashtable dic = new Hashtable();
			foreach (File file in oldFolder.Files){
				dic.Add(file.Name.ToUpper(), file);
			}
			foreach (File file in newFolder.Files){
				string Name = file.Name.ToUpper();
				File oldFile = (File)dic[Name];
				if (oldFile != null){
					//found old
					FindChanges(file,oldFile);
					dic.Remove(Name);
				} else {
					//not found old
					MakeFileAsNew(file);
				}
			}
			if (dic.Count > 0){
				// deleted
				foreach (File file in dic.Values){
					 MakeFileAsDeleted(file);
				}
			}

			dic.Clear();
			foreach (Folder folder in oldFolder.Folders){
				dic.Add(folder.Name.ToUpper(), folder);
			}
			
			foreach (Folder folder in newFolder.Folders){
				string Name = folder.Name.ToUpper();
				Folder oldSubFolder = (Folder)dic[Name];
				if (oldSubFolder != null){
					//found old
					FindChanges(folder,oldSubFolder);
					dic.Remove(Name);
				} else {
					//not found old
					MakeFolderAsNew(folder);
				}
			}
			if (dic.Count > 0){
				// deleted
				foreach (Folder folder in dic.Values){
					 MakeFolderAsDeleted(folder);
				}
			}
		}

		public void MakeFolderAsNew(Folder folder) {
			Add(new Change(ChangeType.New,null,null,folder,null));
			foreach(File file in folder.Files) {
				MakeFileAsNew(file);
			}
			foreach(Folder subFolder in folder.Folders) {
				MakeFolderAsNew(subFolder);
			}
		}

		public void MakeFileAsNew(File file) {
			Add(new Change(ChangeType.New,file,null,null,null));
		}

		public void MakeFolderAsDeleted(Folder folder) {
			Add(new Change(ChangeType.Delete,null,null,null,folder));
			foreach(File file in folder.Files) {
				MakeFileAsDeleted(file);
			}
			foreach(Folder subFolder in folder.Folders) {
				MakeFolderAsDeleted(subFolder);
			}
		}

		public void MakeFileAsDeleted(File file) {
			Add(new Change(ChangeType.Delete,null,file,null,null));
		}

		public void FindChanges(File newFile, File oldFile) {
			if (!newFile.IsEqualTo(oldFile)){
				Add(new Change(ChangeType.Modify,newFile,oldFile,null,null));
			}
		}

		public void SaveTxt(string fileName) {
			using (FileStream stream = new FileStream(fileName,FileMode.OpenOrCreate,FileAccess.Write)) {
				using(StreamWriter writer = new StreamWriter(stream,Encoding.GetEncoding(Encoding.Default.WindowsCodePage))) {
					Sort();
					foreach(Change change in this) {
						string fullPath = change.GetFullPath();
						if (change.IsFolder){
							fullPath = fullPath + Path.DirectorySeparatorChar;
						}
						switch (change.Type){
							case ChangeType.New : {
								fullPath += "\t|new";
								break;
							}
							case ChangeType.Delete : {
								fullPath += "\t|delete";
								break;
							}
							default : {
								if (change.NewFile.Size != change.OldFile.Size){
									fullPath += String.Format("\t|size: {0} => {1}",change.OldFile.Size,change.NewFile.Size);
								}
								if (change.NewFile.WriteTime != change.OldFile.WriteTime){
									fullPath += String.Format("\t|time: {0} => {1}",change.OldFile.WriteTime,change.NewFile.WriteTime);
								}
								break;
							}
						}
						writer.WriteLine(fullPath);
					}
				}
			}
		}

		public override void Sort () {
			base.Sort(this as IComparer);
		}

		public virtual System.Int32 Compare (System.Object x, System.Object y) {
			Change c1 = (Change)x;
			Change c2 = (Change)y;
			string Name1 = c1.GetFullPath();
			string Name2 = c2.GetFullPath();
			return String.Compare(Name1, Name2);
		}

		public void SaveLst(string folderPath) {
			Sort();
			if (!Directory.Exists(folderPath)){
				Directory.CreateDirectory(folderPath);
			}
			string LastRoot = "";
			string LastFolder = @"\";
			StreamWriter writer = null;
			foreach (Change change in this){
				if (change.Type == ChangeType.Delete){
					continue;
				}
				string fullPath = change.GetFullPath();
				string Root = ExtractRoot(fullPath);
				if (Root != LastRoot){
					StringBuilder sb = new StringBuilder(Root);
					foreach (Char ch in Path.InvalidPathChars){
						sb = sb.Replace(ch, ' ');
					}
					char[] InvalidNameChars = {Path.PathSeparator,Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar,Path.VolumeSeparatorChar};
					foreach (Char ch in InvalidNameChars){
						sb = sb.Replace(ch, ' ');
					}
					string s = sb.ToString().Trim();
					string streamName = Path.Combine(folderPath,String.Format("{0}.lst",s));
					//Console.WriteLine("\nFolderPath: {0}\nName: {1}\nResult: {2}",folderPath,s,streamName);
					if (writer != null){
						writer.Close();
						writer = null;
					}
					writer = new StreamWriter(streamName,false,Encoding.GetEncoding(Encoding.Default.WindowsCodePage));
					writer.WriteLine(Root);
					LastRoot = Root;
					LastFolder = @"\";
				}

				string Folder = fullPath.Substring(Root.Length);
				if (!change.IsFolder){
					Folder = Path.GetDirectoryName(Folder);
				}
				Folder += Path.DirectorySeparatorChar;
				string fileName = Path.GetFileName(fullPath);
				if (Folder != LastFolder){
					writer.WriteLine(Folder);
					LastFolder = Folder;
				}
				if (!change.IsFolder){
					writer.WriteLine("{0}\t{1}\t{2}\t{3}",fileName,change.NewFile.Size.ToString(),change.NewFile.WriteTime.ToString("yyyy.MM.dd"),
						change.NewFile.WriteTime.ToString("hh:mm:ss"));
				}
			}
			if (writer != null){
				writer.Close();
			}
		}
		
		public string ExtractRoot(string fullPath) {
			int p = fullPath.IndexOf(Path.PathSeparator,2);
			if (p < 0){
				p = fullPath.IndexOf(Path.DirectorySeparatorChar,2);
			}
			if (p < 0){
				p = fullPath.IndexOf(Path.AltDirectorySeparatorChar,2);
			}
			if (p < 0){
				return "";
			}
			return fullPath.Substring(0,p+1);
		}

	};
}