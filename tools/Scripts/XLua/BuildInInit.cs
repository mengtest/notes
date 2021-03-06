namespace XLua.LuaDLL
{
using XLua;
    using System.Runtime.InteropServices;

[Hotfix]
    public partial class Lua
    {
        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_rapidjson(System.IntPtr L);

        [MonoPInvokeCallbackAttribute(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadRapidJson(System.IntPtr L)
        {
            return luaopen_rapidjson(L);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_lpeg(System.IntPtr L);

        [MonoPInvokeCallbackAttribute(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadLpeg(System.IntPtr L)
        {
            return luaopen_lpeg(L);
        }

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern int luaopen_protobuf_c(System.IntPtr L);

        [MonoPInvokeCallbackAttribute(typeof(LuaDLL.lua_CSFunction))]
        public static int LoadProtobufC(System.IntPtr L)
        {
            return luaopen_protobuf_c(L);
        }
    }
}

