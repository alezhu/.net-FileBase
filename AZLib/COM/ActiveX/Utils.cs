using System.Runtime.InteropServices;
using System.Security;
using System;
namespace AZLib.COM.ActiveX
{
    public class Utils
    {
        [DllImport("Ole32.dll")]
        public static extern int StgOpenStorage([MarshalAs(UnmanagedType.LPWStr)] string wcsName, IStorage pstgPriority, int grfMode, IntPtr snbExclude, int reserved, [Out] out IStorage storage);
    }

    public interface IStream : UCOMIStream { };

    [ComImport, SuppressUnmanagedCodeSecurity, Guid("0000000B-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStorage
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        UCOMIStream CreateStream([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.U4)] int grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved1, [In, MarshalAs(UnmanagedType.U4)] int reserved2);

        [return: MarshalAs(UnmanagedType.Interface)]
        UCOMIStream OpenStream([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, IntPtr reserved1, [In, MarshalAs(UnmanagedType.U4)] int grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved2);

        [return: MarshalAs(UnmanagedType.Interface)]
        IStorage CreateStorage([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.U4)] int grfMode, [In, MarshalAs(UnmanagedType.U4)] int reserved1, [In, MarshalAs(UnmanagedType.U4)] int reserved2);

        [return: MarshalAs(UnmanagedType.Interface)]
        IStorage OpenStorage([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.Interface)]  IStorage pstgPriority, [In, MarshalAs(UnmanagedType.U4)] int grfMode, [In, MarshalAs(UnmanagedType.BStr)] string snbExclude, [In, MarshalAs(UnmanagedType.U4)] int reserved);

        void CopyTo(int ciidExclude, [In, MarshalAs(UnmanagedType.LPArray)] Guid[] rgiidExclude, IntPtr snbExclude, [In, MarshalAs(UnmanagedType.Interface)] IStorage pstgDest);
        void MoveElementTo([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In, MarshalAs(UnmanagedType.Interface)] IStorage pstgDest, [In, MarshalAs(UnmanagedType.BStr)] string pwcsNewName, [In, MarshalAs(UnmanagedType.U4)] int grfFlags);
        void Commit(int grfCommitFlags);
        void Revert();
        int EnumElements([In, MarshalAs(UnmanagedType.U4)] int reserved1, IntPtr reserved2, [In, MarshalAs(UnmanagedType.U4)] int reserved3, [Out, MarshalAs(UnmanagedType.Interface)] out IEnumSTATSTG ppenum);
        void DestroyElement([In, MarshalAs(UnmanagedType.BStr)] string pwcsName);
        void RenameElement([In, MarshalAs(UnmanagedType.BStr)] string pwcsOldName, [In, MarshalAs(UnmanagedType.BStr)] string pwcsNewName);
        void SetElementTimes([In, MarshalAs(UnmanagedType.BStr)] string pwcsName, [In] FILETIME pctime, [In] FILETIME patime, [In] FILETIME pmtime);
        void SetClass(ref Guid clsid);
        void SetStateBits(int grfStateBits, int grfMask);
        int Stat([Out] out STATSTG pStatStg, int grfStatFlag);
    }

    [ComImport, Guid("0000000D-0000-0000-C000-000000000046"), SuppressUnmanagedCodeSecurity, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumSTATSTG
    {
        [PreserveSig]
        int Next(int celt, [Out] out STATSTG rgVar, [Out] out int pceltFetched);
        [PreserveSig]
        int Skip(int celt);
        [PreserveSig]
        int Reset();
        int Clone([Out] out IEnumSTATSTG newEnum);
    }


    public class Storage
    {
        protected IStorage _storage;
        public Storage(IStorage storage)
        {
            _storage = storage;
        }

        public IStream CreateStream(string name, int grfMode)
        {
            return (IStream)_storage.CreateStream(name, grfMode, 0, 0);
        }

        public UCOMIStream OpenStream(string name, int grfMode)
        {
            while (true)
            {
                try
                {
                    UCOMIStream stream = _storage.OpenStream(name, IntPtr.Zero, grfMode, 0);
                    return stream;
                }
                catch (COMException)
                {
                    GC.Collect();
                }
            }
        }

        public IStorage CreateStorage(string name, int grfMode)
        {
            return _storage.CreateStorage(name, grfMode, 0, 0);
        }

        public IStorage OpenStorage(string name, IStorage priority, int grfMode, string snbExclude)
        {
            while (true)
            {
                try
                {
                    IStorage stg = _storage.OpenStorage(name, priority, grfMode, snbExclude, 0);
                    return stg;
                }
                catch (COMException)
                {
                    GC.Collect();
                }
            }
        }

        public IEnumSTATSTG EnumElements()
        {
            //		int EnumElements([In, MarshalAs(UnmanagedType.U4)] int reserved1, IntPtr reserved2, [In, MarshalAs(UnmanagedType.U4)] int reserved3, [Out, MarshalAs(UnmanagedType.Interface)] out IEnumSTATSTG ppenum);
            IEnumSTATSTG iEnum;
            if (_storage.EnumElements(0, IntPtr.Zero, 0, out iEnum) == 0)/*S_OK*/
            {
                return iEnum;
            }
            return null;
        }

    };

    public class Stream
    {
        private UCOMIStream _stream = null;
        public Stream(UCOMIStream stream)
        {
            _stream = stream;
        }

        unsafe public void Read(ref byte[] bytes, int count, out int readed)
        {
            int foo_readed = 0;
            IntPtr p = new IntPtr((void*)&foo_readed);
            _stream.Read(bytes, count, p);
            readed = foo_readed;
        }

        public void Read(ref byte[] bytes)
        {
            int readed;
            Read(ref bytes, bytes.Length, out readed);
        }

        public Int32 ReadInt32()
        {
            byte[] bytes = new byte[Marshal.SizeOf(typeof(Int32))];
            Read(ref bytes);
            IntPtr p = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
            return Marshal.ReadInt32(p);
        }

        public Int64 ReadInt64()
        {
            byte[] bytes = new byte[Marshal.SizeOf(typeof(Int64))];
            Read(ref bytes);
            IntPtr p = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
            return Marshal.ReadInt64(p);
        }


        public double ReadDouble()
        {
            byte[] bytes = new byte[Marshal.SizeOf(typeof(double))];
            Read(ref bytes);
            IntPtr p = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
            unsafe
            {
                Double d = (*(double*)p);
                return d;
            }
        }
    };
}