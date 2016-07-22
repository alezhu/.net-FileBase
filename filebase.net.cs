using System;
using System.IO;
namespace Filebase
{
    class Application
    {
        public static Options options = new Options();
        public static void Main()
        {
            Console.WriteLine("AZ Filebase.NET\nVersion {0}\nCopiright © 2007 by AZ",options.Version);
            options.Load();
            string OldBaseName = Path.Combine(options.CurDir, "filebase.fbs");
            FileBase OldBase = null;
            if (System.IO.File.Exists(OldBaseName))
            {
                OldBase = new FileBase();
                Console.Write("Old filebase loading...");
                OldBase.Load(OldBaseName);
                Console.WriteLine("OK");
            }
            else
            {
                string DelphiBaseName = Path.Combine(options.CurDir, "oldbase.fbs");
                if (System.IO.File.Exists(DelphiBaseName))
                {
                    OldBase = new FileBase();
                    Console.Write("Delphi filebase loading...");
                    OldBase.LoadOldBase(DelphiBaseName);
                    Console.WriteLine("OK");
                }
            }
            Console.WriteLine("New filebase creating...:");
            DateTime now = DateTime.Now;
            FileBase NewBase = new FileBase();
            foreach (string folder in options.Folders)
            {
                Console.WriteLine("Folder: {0}", folder);
                Root root = null;
                try
                {
                    root = new Root(NewBase, folder);
                }
                catch (DirectoryNotFoundException)
                {
                    Console.WriteLine("Error Folder: {0}", folder);
                    root = new Root(NewBase);
                    root.Name = folder;
                }
                NewBase.AddRoot(root);
            }
            foreach (Root root in NewBase.Roots)
            {
                Console.WriteLine("NewRoot: {0}", root.Name);
            }
            TimeSpan ts = DateTime.Now - now;
            Console.WriteLine("New filebase has been created in {0} seconds\nFind differenses...", ts.Seconds);
            Changes changes = null;
            if (OldBase != null)
            {
                changes = Changes.MakeChanges(NewBase, OldBase);
            }
            else
            {
                changes = Changes.MakeAsNew(NewBase);
            }
			if (changes.Count > 0){
				Console.Write("Creating text report...");
				changes.SaveTxt(Path.Combine(options.CurDir, String.Format("{0}.txt", DateTime.Now.ToString("yyyy-MM-dd"))));
				Console.WriteLine("OK");
				Console.Write("Creating lsp-files...");
				changes.SaveLst(Path.Combine(options.CurDir, DateTime.Now.ToString("yyyy-MM-dd")));
				Console.WriteLine("OK");
			} else {
				Console.WriteLine("No differences found since last search");
			}

            string NewBaseName = OldBaseName;
            if (System.IO.File.Exists(OldBaseName))
            {
                Console.Write("Backup old base...");
                int i = 0;
                string Dir = System.IO.Path.GetDirectoryName(OldBaseName);
                while (System.IO.File.Exists(OldBaseName))
                {
                    OldBaseName = Path.Combine(Dir, String.Format("{0}.{1}.bak",
                        System.IO.Path.GetFileNameWithoutExtension(NewBaseName), i++));
                }
                System.IO.File.Move(NewBaseName, OldBaseName);
                Console.WriteLine("OK");
            }

            Console.Write("Save new base...");
            NewBase.Save(NewBaseName);
            Console.WriteLine("OK");
/*            Console.WriteLine("All Done. Press Enter for exit.");
            Console.ReadLine();*/
        }


    };
}